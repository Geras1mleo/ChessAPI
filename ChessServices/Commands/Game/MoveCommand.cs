namespace ChessServices.Commands.Game;

public class MoveCommand : IChessRequest<ChessBoardDTO>
{
    public int LobbyId { get; set; }
    public Guid Key { get; set; }
    public string Move { get; set; }

    public MoveCommand(int lobbyId, Guid key, string move)
    {
        LobbyId = lobbyId;
        Key = key;
        Move = move;
    }
}

public class MoveCommandHandler : IChessRequestHandler<MoveCommand, ChessBoardDTO>
{
    private readonly ILobbyValidator validator;
    private readonly IChessResponseProvider chessResponseProvider;

    public MoveCommandHandler(ILobbyValidator validator,
                              IChessResponseProvider chessResponseProvider)
    {
        this.validator = validator;
        this.chessResponseProvider = chessResponseProvider;
    }

    public Task<IChessResponse<ChessBoardDTO>> Handle(MoveCommand request, CancellationToken cancellationToken)
    {
        var lobby = validator.GetLobby(request.LobbyId);
        lobby.MakeMove(request.Move, request.Key);

        return Task.FromResult(
        chessResponseProvider.Ok("Move executed successfully!", new ChessBoardDTO
        {
            PGN = lobby.Board.ToPgn(),
            FEN = lobby.Board.ToFen()
        }));
    }
}