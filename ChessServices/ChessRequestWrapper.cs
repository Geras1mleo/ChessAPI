namespace ChessServices;

public interface IChessRequest<TResponse> : IRequest<IChessResponse<TResponse>>
    where TResponse : ChessResponseDTO
{ }

public interface IChessRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, IChessResponse<TResponse>>
    where TResponse : ChessResponseDTO
    where TRequest : IChessRequest<TResponse>
{ }
