namespace ChessServices.Commands.Lobby;

public class LeaveLobbyCommand : IChessRequest<ChessResponseDTO>
{
    public int LobbyId { get; set; }
    public Guid Key { get; set; }

    public LeaveLobbyCommand(int lobbyId, Guid key)
    {
        LobbyId = lobbyId;
        Key = key;
    }
}

public class LeaveLobbyCommandHandler : IChessRequestHandler<LeaveLobbyCommand, ChessResponseDTO>
{
    private readonly LobbyValidator validator;

    public LeaveLobbyCommandHandler(LobbyValidator validator)
    {
        this.validator = validator;
    }

    public Task<IChessResponse<ChessResponseDTO>> Handle(LeaveLobbyCommand request, CancellationToken cancellationToken)
    {
        var lobby = validator.GetLobby(request.LobbyId);
        lobby.LeaveLobby(request.Key);

        // If both players left => delete lobby
        if (lobby.WhitePlayer is null && lobby.BlackPlayer is null)
        {
            validator.Lobbies.Remove(lobby);
            lobby.CloseHosts();
        }


        return Task.FromResult(ChessResponse.Ok<ChessResponseDTO>($"Left from Lobby {request.LobbyId} successfully!"));
    }
}
