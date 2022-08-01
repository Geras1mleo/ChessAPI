namespace ChessServices;

public class ChessResponseProvider : IChessResponseProvider
{
    public IChessResponse<T> Ok<T>(string message, T data = default) where T : ChessResponseDTO
    {
        return new ChessResponse<T>()
        {
            Message = message,
            Data = data,
            Status = 200
        };
    }

    public IChessResponse<ChessResponseDTO> Ok(string message)
    {
        return new ChessResponse<ChessResponseDTO>()
        {
            Message = message,
            Data = null,
            Status = 200
        };
    }

    public IChessResponse<T> Created<T>(string message, T data = default) where T : ChessResponseDTO
    {
        return new ChessResponse<T>()
        {
            Message = message,
            Data = data,
            Status = 201
        };
    }

    public IChessResponse<T> BadRequest<T>(string message, T data = default) where T : ChessResponseDTO
    {
        return new ChessResponse<T>()
        {
            Message = message,
            Data = data,
            Status = 400
        };
    }

    public IChessResponse<ChessResponseDTO> BadRequest(string message)
    {
        return new ChessResponse<ChessResponseDTO>()
        {
            Message = message,
            Data = null,
            Status = 400
        };
    }

    public IChessResponse<T> NotFound<T>(string message, T data = default) where T : ChessResponseDTO
    {
        return new ChessResponse<T>()
        {
            Message = message,
            Data = data,
            Status = 404
        };
    }

    public IChessResponse<ChessResponseDTO> NotFound(string message)
    {
        return new ChessResponse<ChessResponseDTO>()
        {
            Message = message,
            Data = null,
            Status = 404
        };
    }
}
