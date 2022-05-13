namespace ChessAPI.Controllers;

[Route("LobbyHost/")]
[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly ILogger<WebSocketController> logger;
    private readonly LobbiesRepository repository;

    public WebSocketController(ILogger<WebSocketController> logger, LobbiesRepository repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    [HttpGet("/Host")]
    public async Task<ActionResult> Index()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            return BadRequest("Web socket required...");
        }

        await HostClient(await HttpContext.WebSockets.AcceptWebSocketAsync());

        return new EmptyResult();
    }

    private async Task HostClient(WebSocket webSocket)
    {
        Channel<string> channel = null;

        try
        {
            // Sending message to client to specify lobbyID and identity key
            await SendTextAsync(webSocket, "LobbyID and player key required...");

            // Response with expected id and key
            IdentifyPlayerDTO identifyObj = JsonConvert.DeserializeObject<IdentifyPlayerDTO>(await ReceiveTextAsync(webSocket));

            if (identifyObj is null)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Please specify \"lobbyId\" and player \"key\" in JSON format. See: IdentifyPlayerDTO", CancellationToken.None);
                return;
            }

            // Bind player notifications to the channel
            // If lobbyID or player is not found => exception is thrown
            channel = repository.Host(identifyObj);

            await SendTextAsync(webSocket, "Connection established!");

            var closeCancelToken = new CancellationTokenSource();
            var channelCancelToken = new CancellationTokenSource();

            var closeTask = Task.Run(async () =>
            {
                // Running till client disconnects / till MessageType of received data is not "Close" action
                while (webSocket.State == WebSocketState.Open &&
                        (await webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[1/*024 * 4*/]), closeCancelToken.Token))
                            .MessageType != WebSocketMessageType.Close)
                { }
                // We don't care what client sends to server, but we need to handle closing of socket
            });

            while (!webSocket.CloseStatus.HasValue && !channel.Reader.Completion.IsCompleted)
            {
                // Waiting till client disconnects, OR! till we get a message that has to be send to client
                Task.WaitAny(
                Task.Run(async () =>
                {
                    await channel.Reader.WaitToReadAsync(channelCancelToken.Token);
                }),
                closeTask);

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

            logger.LogError($"Not Found while hosting client:\nMessage: {e.Message}");
        }
        catch (Exception e)
        {
            if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseSent || webSocket.State == WebSocketState.CloseReceived)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Connection was enforced closed", CancellationToken.None);
            }

            logger.LogError($"Error while hosting client:\nMessage: {e.Message}");
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

    private async Task SendTextAsync(WebSocket webSocket, string text)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task<string> ReceiveTextAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        return Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
    }
}
