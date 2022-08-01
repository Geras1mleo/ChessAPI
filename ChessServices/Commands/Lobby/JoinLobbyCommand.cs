namespace ChessServices.Commands.Lobby;

public class JoinLobbyCommand : IChessRequest<LobbyJoinedDTO>
{
    public string Username { get; set; }
    public int LobbyId { get; set; }

    public JoinLobbyCommand(string username, int lobbyId)
    {
        Username = username;
        LobbyId = lobbyId;
    }
}

public class JoinLobbyCommandHandler : IChessRequestHandler<JoinLobbyCommand, LobbyJoinedDTO>
{
    private readonly ILobbyValidator validator;
    private readonly IChessResponseProvider chessResponseProvider;

    public JoinLobbyCommandHandler(ILobbyValidator validator,
                                   IChessResponseProvider chessResponseProvider)
    {
        this.validator = validator;
        this.chessResponseProvider = chessResponseProvider;
    }

    public Task<IChessResponse<LobbyJoinedDTO>> Handle(JoinLobbyCommand request, CancellationToken cancellationToken)
    {
        var lobby = validator.GetLobby(request.LobbyId);
        request.Username = validator.ValidateUsername(request.Username);

        var key = Guid.NewGuid();
        var player = new Player(request.Username, key);

        lobby.Join(player);

        return Task.FromResult(
        chessResponseProvider.Ok("Lobby joined successfully!",
        new LobbyJoinedDTO
        {
            LobbyId = lobby.LobbyId,
            Key = key,
            PlayingSide = lobby.GetSide(player),
            White = lobby.GetPlayerDTO(lobby.WhitePlayer),
            Black = lobby.GetPlayerDTO(lobby.BlackPlayer),
        }));
    }
}
