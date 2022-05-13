namespace ChessAPI.Controllers;

[Route("Lobby/")]
[ApiController]
public class GameController : ControllerBase
{
    readonly LobbiesService service;

    public GameController(LobbiesService service)
    {
        this.service = service;
    }

    [HttpPost("Move/{lobbyId}")]
    [ProducesResponseType(typeof(ChessMoveDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ChessErrorDTO), StatusCodes.Status400BadRequest)]
    public ActionResult MoveInLobby([Required] int lobbyId, [Required] string move, [FromHeader(Name = "key")][Required] Guid key)
    {
        return Wrap(() =>
        {
            return Ok(service.Move(lobbyId, key, move));
        });
    }

    [HttpGet("Explore/{lobbyId}")]
    [ProducesResponseType(typeof(ChessGameDTO), StatusCodes.Status200OK)]
    public ActionResult ExploreLobby([Required] int lobbyId)
    {
        return Wrap(() =>
        {
            return Ok(service.Explore(lobbyId));
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
        catch (ChessException e)
        {
            return BadRequest(new ChessErrorDTO() { Error = e.Message, Fen = e.Board.ToFen() });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}