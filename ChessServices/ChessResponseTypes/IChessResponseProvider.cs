namespace ChessServices
{
    public interface IChessResponseProvider
    {
        IChessResponse<ChessResponseDTO> BadRequest(string message);
        IChessResponse<T> BadRequest<T>(string message, T data = null) where T : ChessResponseDTO;
        IChessResponse<T> Created<T>(string message, T data = null) where T : ChessResponseDTO;
        IChessResponse<ChessResponseDTO> NotFound(string message);
        IChessResponse<T> NotFound<T>(string message, T data = null) where T : ChessResponseDTO;
        IChessResponse<ChessResponseDTO> Ok(string message);
        IChessResponse<T> Ok<T>(string message, T data = null) where T : ChessResponseDTO;
    }
}