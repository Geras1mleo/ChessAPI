namespace ChessServices;

public interface IChessRequest<TResponse> : IRequest<IChessResponse<TResponse>>
    where TResponse : BaseResponseData
{ }

public interface IChessRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, IChessResponse<TResponse>>
    where TResponse : BaseResponseData
    where TRequest : IChessRequest<TResponse>
{ }
