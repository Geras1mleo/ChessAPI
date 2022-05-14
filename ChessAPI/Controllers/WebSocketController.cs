namespace ChessAPI.Controllers;

[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly IMediator mediator;

    public WebSocketController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet("Player/Host")]
    public async Task<ActionResult> Host()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            return BadRequest("Web socket required...");
        }

        await mediator.Publish(new HostRequestQuery(await HttpContext.WebSockets.AcceptWebSocketAsync(), HostType.Client));

        return new EmptyResult();
    }


    [HttpGet("Lobby/Spectate")]
    public async Task<ActionResult> Spectate()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            return BadRequest("Web socket required...");
        }

        await mediator.Publish(new HostRequestQuery(await HttpContext.WebSockets.AcceptWebSocketAsync(), HostType.Spectator));

        return new EmptyResult();
    }
}
