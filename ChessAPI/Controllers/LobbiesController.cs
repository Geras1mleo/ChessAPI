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
    public Task<ActionResult<IChessResponse<ChessResponseDTO>>> CreateLobby([Required] string username, int? lobbyId, SideDTO? side)
    {
        return HandleError(async () =>
        {
            return await mediator.Send(new CreateLobbyCommand(username, lobbyId, side));
        });
    }

    [HttpPost("Join")]
    [ProducesResponseType(typeof(IChessResponse<LobbyJoinedDTO>), StatusCodes.Status200OK)]
    public Task<ActionResult<IChessResponse<ChessResponseDTO>>> JoinLobby([Required] int lobbyId, [Required] string username)
    {
        return HandleError(async () =>
        {
            return await mediator.Send(new JoinLobbyCommand(username, lobbyId));
        });
    }

    [HttpPost("Leave")]
    public Task<ActionResult<IChessResponse<ChessResponseDTO>>> LeaveLobby([Required] int lobbyId, [FromHeader(Name = "key")][Required] Guid key)
    {
        return HandleError(async () =>
        {
            return await mediator.Send(new LeaveLobbyCommand(lobbyId, key));
        });
    }

    [HttpGet("{lobbyId}")]
    [ProducesResponseType(typeof(IChessResponse<ChessLobbyDTO>), StatusCodes.Status200OK)]
    public Task<ActionResult<IChessResponse<ChessResponseDTO>>> ExploreLobby([Required] int lobbyId)
    {
        return HandleError(async () =>
        {
            return await mediator.Send(new ExploreLobbyQuery(lobbyId));
        });
    }

    private async Task<ActionResult<IChessResponse<ChessResponseDTO>>> HandleError(Func<Task<IChessResponse<ChessResponseDTO>>> target)
    {
        try
        {
            return Ok(await target());
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
