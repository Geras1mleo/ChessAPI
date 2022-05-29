namespace ChessServices;

public static class ChessResponse
{
    public static IChessResponse<T> Ok<T>(string message, T data = default) where T : ChessResponseDTO => new ChessResponse<T>() { Message = message, Data = data, Status = 200 };
    public static IChessResponse<ChessResponseDTO> Ok(string message) => new ChessResponse<ChessResponseDTO>() { Message = message, Data = null, Status = 200 };


    public static IChessResponse<T> Created<T>(string message, T data = default) where T : ChessResponseDTO => new ChessResponse<T>() { Message = message, Data = data, Status = 201 };

    public static IChessResponse<T> BadRequest<T>(string message, T data = default) where T : ChessResponseDTO => new ChessResponse<T>() { Message = message, Data = data, Status = 400 };
    public static IChessResponse<ChessResponseDTO> BadRequest(string message) => new ChessResponse<ChessResponseDTO>() { Message = message, Data = null, Status = 400 };

    public static IChessResponse<T> NotFound<T>(string message, T data = default) where T : ChessResponseDTO => new ChessResponse<T>() { Message = message, Data = data, Status = 404 };
    public static IChessResponse<ChessResponseDTO> NotFound(string message) => new ChessResponse<ChessResponseDTO>() { Message = message, Data = null, Status = 404 };
}

public interface IChessResponse<out T> where T : ChessResponseDTO
{
    int Status { get; set; }
    string Message { get; set; }
    T Data { get; }
}

internal class ChessResponse<T> : IChessResponse<T> where T : ChessResponseDTO
{
    public int Status { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}