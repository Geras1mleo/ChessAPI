namespace ChessAPI.Controllers;

[Route("Lobby")]
[ApiController]
public class LobbiesController : ControllerBase
{
    private readonly IMediator mediator;

    public LobbiesController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost("Create")]
    [ProducesResponseType(typeof(IChessResponse<LobbyJoinedDTO>), StatusCodes.Status201Created)]
    public ActionResult<IChessResponse<BaseResponseData>> CreateLobby([Required] string username, int? lobbyId, SideDTO? side)
    {
        return Handle(() =>
        {
            return mediator.Send(new CreateLobbyCommand(username, lobbyId, side))
                            .GetAwaiter()
                            .GetResult();
        });
    }

    [HttpPost("Join")]
    [ProducesResponseType(typeof(IChessResponse<LobbyJoinedDTO>), StatusCodes.Status200OK)]
    public ActionResult<IChessResponse<BaseResponseData>> JoinLobby([Required] int lobbyId, [Required] string username)
    {
        return Handle(() =>
        {
            return mediator.Send(new JoinLobbyCommand(username, lobbyId))
                            .GetAwaiter()
                            .GetResult();
        });
    }

    [HttpPost("Leave")]
    public ActionResult<IChessResponse<BaseResponseData>> LeaveLobby([Required] int lobbyId, [FromHeader(Name = "key")][Required] Guid key)
    {
        return Handle(() =>
        {
            return mediator.Send(new LeaveLobbyCommand(lobbyId, key))
                            .GetAwaiter()
                            .GetResult();
        });
    }

    [HttpGet("{lobbyId}")]
    [ProducesResponseType(typeof(IChessResponse<ChessLobbyDTO>), StatusCodes.Status200OK)]
    public ActionResult<IChessResponse<BaseResponseData>> ExploreLobby([Required] int lobbyId)
    {
        return Handle(() =>
        {
            return mediator.Send(new ExploreLobbyQuery(lobbyId))
                       .GetAwaiter()
                       .GetResult();
        });
    }
    private ActionResult<IChessResponse<BaseResponseData>> Handle(Func<IChessResponse<BaseResponseData>> target)
    {
        try
        {
            return Ok(target());
        }
        catch (LobbyNotFoundException e)
        {
            return NotFound(ChessResponse.NotFound(e.Message));
        }
        catch (LobbyException e)
        {
            return BadRequest(ChessResponse.BadRequest(e.Message));
        }
        catch (Exception e)
        {
            return BadRequest(ChessResponse.BadRequest(e.Message));
        }
    }
}
