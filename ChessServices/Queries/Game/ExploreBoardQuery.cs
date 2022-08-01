namespace ChessServices.Queries.Game;

public class ExploreBoardQuery : IChessRequest<ChessBoardDTO>
{
    public int LobbyId { get; set; }

    public ExploreBoardQuery(int lobbyId)
    {
        LobbyId = lobbyId;
    }
}

public class ExploreBoardQueryHandler : IChessRequestHandler<ExploreBoardQuery, ChessBoardDTO>
{
    private readonly ILobbyValidator validator;
    private readonly IChessResponseProvider chessResponseProvider;

    public ExploreBoardQueryHandler(ILobbyValidator validator,
                                    IChessResponseProvider chessResponseProvider)
    {
        this.validator = validator;
        this.chessResponseProvider = chessResponseProvider;
    }

    public Task<IChessResponse<ChessBoardDTO>> Handle(ExploreBoardQuery request, CancellationToken cancellationToken)
    {
        var board = validator.GetLobby(request.LobbyId).Board;

        return Task.FromResult(
        chessResponseProvider.Ok("Exploring board positions...",
        new ChessBoardDTO
        {
            FEN = board.ToFen(),
            PGN = board.ToPgn()
        }));
    }
}
