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
    private readonly ILobbyValidator validator;
    private readonly ILobbyRepository lobbyRepository;
    private readonly IChessResponseProvider chessResponseProvider;

    public LeaveLobbyCommandHandler(ILobbyValidator validator,
                                    ILobbyRepository lobbyRepository,
                                    IChessResponseProvider chessResponseProvider)
    {
        this.validator = validator;
        this.lobbyRepository = lobbyRepository;
        this.chessResponseProvider = chessResponseProvider;
    }

    public Task<IChessResponse<ChessResponseDTO>> Handle(LeaveLobbyCommand request, CancellationToken cancellationToken)
    {
        var lobby = validator.GetLobby(request.LobbyId);
        lobby.LeaveLobby(request.Key);

        // If both players left => delete lobby
        if (lobby.WhitePlayer is null && lobby.BlackPlayer is null)
        {
            lobbyRepository.Lobbies.Remove(lobby);
            lobby.CloseHosts();
        }


        return Task.FromResult(
        chessResponseProvider.Ok<ChessResponseDTO>($"Left from Lobby {request.LobbyId} successfully!"));
    }
}
