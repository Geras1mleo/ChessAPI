﻿namespace ChessAPI.Controllers;

[Route("Lobby")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IMediator mediator;

    public GameController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost("Move/{lobbyId}")]
    [ProducesResponseType(typeof(IChessResponse<ChessBoardDTO>), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ChessErrorDTO), StatusCodes.Status400BadRequest)]
    public ActionResult<IChessResponse<ChessResponseDTO>> MoveInLobby([Required] int lobbyId, [Required] string move, [FromHeader(Name = "key")][Required] Guid key)
    {
        return Handle(() =>
        {
            return mediator.Send(new MoveCommand(lobbyId, key, move))
                    .GetAwaiter()
                    .GetResult();
        });
    }

    [HttpGet("Board/{lobbyId}")]
    [ProducesResponseType(typeof(IChessResponse<ChessBoardDTO>), StatusCodes.Status200OK)]
    public ActionResult<IChessResponse<ChessResponseDTO>> ExploreLobby([Required] int lobbyId)
    {
        // todo fields (needed properties to send) 
        return Handle(() =>
        {
            return mediator.Send(new ExploreBoardQuery(lobbyId))
                       .GetAwaiter()
                       .GetResult();
        });
    }

    private ActionResult<IChessResponse<ChessResponseDTO>> Handle(Func<IChessResponse<ChessResponseDTO>> target)
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
        catch (ChessException e)
        {
            return BadRequest(ChessResponse.BadRequest(e.Message,
                                                       new ChessBoardDTO
                                                       {
                                                           FEN = e.Board?.ToFen(),
                                                           PGN = e.Board?.ToPgn()
                                                       }));
        }
        catch (Exception e)
        {
            return BadRequest(ChessResponse.BadRequest(e.Message));
        }
    }
}