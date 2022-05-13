namespace ChessAPI.Controllers;

[Route("Lobby/")]
[ApiController]
public class LobbiesController : ControllerBase
{
    readonly LobbiesRepository repository;

    public LobbiesController(LobbiesRepository repository)
    {
        this.repository = repository;
    }

    [HttpPost("Create")]
    [ProducesResponseType(typeof(LobbyJoinedDTO), StatusCodes.Status201Created)]
    public ActionResult CreateLobby([Required] string username, int? lobbyId, Side? side)
    {
        return Wrap(() =>
        {
            return Created("Lobby/Create", repository.Create(username, lobbyId, side == null ? null : PieceColor.FromValue((int)side)));
        });
    }

    [HttpPost("Join")]
    [ProducesResponseType(typeof(LobbyJoinedDTO), StatusCodes.Status200OK)]
    public ActionResult JoinLobby([Required] int lobbyId, [Required] string username)
    {
        return Wrap(() =>
        {
            return Ok(repository.Join(lobbyId, username));
        });
    }

    [HttpPost("Leave")]
    public ActionResult LeaveLobby([Required] int lobbyId, [FromHeader(Name = "key")][Required] Guid key)
    {
        return Wrap(() =>
        {
            repository.Leave(lobbyId, key);
            return Ok($"Left from lobby {lobbyId} successfully!");
        });
    }

    private ActionResult Wrap(Func<ActionResult> target)
    {
        try
        {
            return target();
        }
        catch (LobbyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (LobbyException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
