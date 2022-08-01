namespace ChessServices;

public interface IChessResponse<out T> where T : ChessResponseDTO
{
    int Status { get; set; }
    string Message { get; set; }
    T Data { get; }
}
