namespace ChessServices;

internal class ChessResponse<T> : IChessResponse<T> where T : ChessResponseDTO
{
    public int Status { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}