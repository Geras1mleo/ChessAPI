namespace ChessServices.Notifications;

public enum HostType
{
    Client = 1,
    Spectator = 2,
}

public class HostRequestQuery : INotification
{
    public WebSocket Socket { get; set; }
    public HostType Type { get; set; }

    public HostRequestQuery(WebSocket socket, HostType type)
    {
        Socket = socket;
        Type = type;
    }
}

public class HostRequestQueryHandler : INotificationHandler<HostRequestQuery>
{
    private readonly LobbyValidator validator;

    public HostRequestQueryHandler(LobbyValidator validator)
    {
        this.validator = validator;
    }

    public async Task Handle(HostRequestQuery request, CancellationToken cancellationToken)
    {
        var webSocket = request.Socket;
        Channel<string> channel = null;

        try
        {
            if (request.Type == HostType.Client)
            {
                // Sending message to client to specify lobbyID and identity key
                await SendTextAsync(webSocket, "LobbyID and player key required...");

                // Response with expected id and key
                var identifyObj = JsonConvert.DeserializeObject<IdentifyPlayerDTO>(ReceiveTextAsync(webSocket));

                if (identifyObj is null)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData,
                        $"Please specify \"lobbyId\" and player \"key\" in JSON format. See: {nameof(IdentifyPlayerDTO)}", CancellationToken.None);
                    return;
                }

                channel = HostPlayer(identifyObj);
            }
            else
            {
                // Sending message to client to specify lobbyID
                await SendTextAsync(webSocket, "LobbyID required...");

                // Response with expected id and key
                var identifyObj = JsonConvert.DeserializeObject<IdentifyLobbyDTO>(ReceiveTextAsync(webSocket));

                if (identifyObj is null)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData,
                        $"Please specify \"lobbyId\" in JSON format. See: {nameof(IdentifyLobbyDTO)}", CancellationToken.None);
                    return;
                }

                channel = HostSpectator(identifyObj);
            }

            await SendTextAsync(webSocket, "Connection established!");

            var closeCancelToken = new CancellationTokenSource();
            var channelCancelToken = new CancellationTokenSource();

            var closeTask = Task.Run(async () =>
            {
                // Running till client disconnects / till MessageType of received data is not "Close" action
                while (webSocket.State == WebSocketState.Open &&
                        (await webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[1]), closeCancelToken.Token))
                            .MessageType != WebSocketMessageType.Close)
                { }
                // We don't care what client sends to server, but we need to handle closing of socket
            }, cancellationToken);

            while (!webSocket.CloseStatus.HasValue && !channel.Reader.Completion.IsCompleted)
            {
                // Waiting till client disconnects, OR! till we get a message that has to be send to client
                Task.WaitAny(new[] {
                Task.Run(async () =>
                {
                    await channel.Reader.WaitToReadAsync(channelCancelToken.Token);
                }, cancellationToken),
                closeTask }, cancellationToken);

                // If able to read => send; otherwise => channel OR socket connection closed (next iteration will break the loop)
                if (!channel.Reader.Completion.IsCompleted && channel.Reader.TryRead(out var message))
                {
                    await SendTextAsync(webSocket, message);
                }
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);

            // At the end one of tasks keeps running, so we cancel both tasks here
            closeCancelToken.Cancel();
            channelCancelToken.Cancel();
        }
        catch (LobbyNotFoundException e)
        {
            if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseSent || webSocket.State == WebSocketState.CloseReceived)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, e.Message, CancellationToken.None);
            }
        }
        catch (Exception)
        {
            if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseSent || webSocket.State == WebSocketState.CloseReceived)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Connection was enforced closed", CancellationToken.None);
            }
        }
        finally
        {
            // Notify Player object that this channel is completed/ended
            if (channel is not null && !channel.Reader.Completion.IsCompleted)
            {
                channel.Writer.Complete();
            }
        }
    }

    public Channel<string> HostPlayer(IdentifyPlayerDTO identify)
    {
        // Bind player notifications to the channel
        // If lobbyID or player is not found => exception is thrown

        var player = validator.GetLobby(identify.LobbyId).GetPlayer(identify.Key);

        var channel = Channel.CreateUnbounded<string>();

        player.Channels.Add(channel);

        return channel;
    }

    public Channel<string> HostSpectator(IdentifyLobbyDTO identify)
    {
        // Bind spectator notifications to the channel
        // If lobbyID is not found => exception is thrown

        var lobby = validator.GetLobby(identify.LobbyId);

        var channel = Channel.CreateUnbounded<string>();

        lobby.Spectators.Add(channel);

        return channel;
    }

    private async Task SendTextAsync(WebSocket webSocket, string text)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private string ReceiveTextAsync(WebSocket webSocket)
    {
        // Don't pass cancellationToken to ReceiveAsync func bc it sets web socket state to aborted whitch doesn't allow to close connection properly 

        var cancellationToken = new CancellationTokenSource();

        var buffer = new byte[1024 * 4];

        WebSocketReceiveResult receiveResult = null;

        // Waiting for "credentials" for 5 sec and cancelling operation
        Task.WaitAny(
            Task.Delay(5000),
            Task.Run(async () => receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None), cancellationToken.Token)
            );

        if (receiveResult is null)
        {
            cancellationToken.Cancel();
            return "";
        }

        return Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
    }
}
