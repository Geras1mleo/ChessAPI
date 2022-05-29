namespace ChessServices.Queries.Lobby;

public class ExploreLobbyQuery : IChessRequest<ChessLobbyDTO>
{
    public int LobbyId { get; set; }

    public ExploreLobbyQuery(int lobbyId)
    {
        LobbyId = lobbyId;
    }
}

public class ExploreLobbyQueryHandler : IChessRequestHandler<ExploreLobbyQuery, ChessLobbyDTO>
{
    private readonly LobbyValidator validator;

    public ExploreLobbyQueryHandler(LobbyValidator validator)
    {
        this.validator = validator;
    }

    public Task<IChessResponse<ChessLobbyDTO>> Handle(ExploreLobbyQuery request, CancellationToken cancellationToken)
    {
        var lobby = validator.GetLobby(request.LobbyId);

        return Task.FromResult(
                ChessResponse.Ok("Exploring lobby...",
                new ChessLobbyDTO
                {
                    BlackPlayer = lobby.GetFullPlayerDTO(lobby.BlackPlayer),
                    WhitePlayer = lobby.GetFullPlayerDTO(lobby.WhitePlayer),
                    Board = lobby.GetBoardDTO(),
                    Spectators = lobby.SpectatorsChannels.Count,
                }));
    }
}