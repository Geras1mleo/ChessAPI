namespace ChessServices.Commands.Lobby;

public class LeaveLobbyCommand : IChessRequest<BaseResponseData>
{
    public int LobbyId { get; set; }
    public Guid Key { get; set; }

    public LeaveLobbyCommand(int lobbyId, Guid key)
    {
        LobbyId = lobbyId;
        Key = key;
    }
}

public class LeaveLobbyCommandHandler : IChessRequestHandler<LeaveLobbyCommand, BaseResponseData>
{
    private readonly LobbyValidator validator;

    public LeaveLobbyCommandHandler(LobbyValidator validator)
    {
        this.validator = validator;
    }

    public Task<IChessResponse<BaseResponseData>> Handle(LeaveLobbyCommand request, CancellationToken cancellationToken)
    {
        var lobby = validator.GetLobby(request.LobbyId);
        lobby.LeaveLobby(request.Key);

        // If both players left => delete lobby
        if (lobby.WhitePlayer is null && lobby.BlackPlayer is null)
            validator.Lobbies.Remove(lobby);

        return Task.FromResult(ChessResponse.Ok<BaseResponseData>($"Left from Lobby {request.LobbyId} successfully!"));
    }
}
