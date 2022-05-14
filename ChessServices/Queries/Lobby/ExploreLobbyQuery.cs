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
                    BlackPlayer = new()
                    {
                        Username = lobby.BlackPlayer.Username,
                        Score = lobby.BlackPlayer.Score,
                        PendingDraw = lobby.BlackPlayer.PendingDraw,
                        PendingRematch = lobby.BlackPlayer.PendingRematch,
                        Connections = lobby.BlackPlayer.Channels.Count,
                    },
                    WhitePlayer = new()
                    {
                        Username = lobby.WhitePlayer.Username,
                        Score = lobby.WhitePlayer.Score,
                        PendingDraw = lobby.WhitePlayer.PendingDraw,
                        PendingRematch = lobby.WhitePlayer.PendingRematch,
                        Connections = lobby.WhitePlayer.Channels.Count,
                    },
                    Board = new()
                    {
                        FEN = lobby.Board.ToFen(),
                        PGN = lobby.Board.ToFen(),
                    },
                    Spectators = lobby.Spectators.Count,
                }));
    }
}