namespace ChessAPI.Controllers;

[Route("Lobby")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly IChessResponseProvider chessResponseProvider;

    public GameController(IMediator mediator,
                          IChessResponseProvider chessResponseProvider)
    {
        this.mediator = mediator;
        this.chessResponseProvider = chessResponseProvider;
    }

    [HttpPost("Move/{lobbyId}")]
    [ProducesResponseType(typeof(IChessResponse<ChessBoardDTO>), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ChessErrorDTO), StatusCodes.Status400BadRequest)]
    public Task<ActionResult<IChessResponse<ChessResponseDTO>>> MoveInLobby([Required] int lobbyId, [Required] string move, [FromHeader(Name = "key")][Required] Guid key)
    {
        return HandleError(async () =>
        {
            return await mediator.Send(new MoveCommand(lobbyId, key, move));
        });
    }

    [HttpGet("Board/{lobbyId}")]
    [ProducesResponseType(typeof(IChessResponse<ChessBoardDTO>), StatusCodes.Status200OK)]
    public Task<ActionResult<IChessResponse<ChessResponseDTO>>> ExploreLobby([Required] int lobbyId)
    {
        // todo fields (needed properties to return) 
        return HandleError(async () =>
        {
            return await mediator.Send(new ExploreBoardQuery(lobbyId));
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
            return NotFound(chessResponseProvider.NotFound(e.Message));
        }
        catch (LobbyException e)
        {
            return BadRequest(chessResponseProvider.BadRequest(e.Message));
        }
        catch (ChessException e)
        {
            return BadRequest(chessResponseProvider.BadRequest(e.Message,
            new ChessBoardDTO
            {
                FEN = e.Board?.ToFen(),
                PGN = e.Board?.ToPgn()
            }));
        }
        catch (Exception e)
        {
            return BadRequest(chessResponseProvider.BadRequest(e.Message));
        }
    }
}