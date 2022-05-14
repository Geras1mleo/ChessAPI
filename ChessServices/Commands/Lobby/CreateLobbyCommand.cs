namespace ChessServices.Commands.Lobby;

public class CreateLobbyCommand : IChessRequest<LobbyJoinedDTO>
{
    public string Username { get; set; }
    public int? LobbyId { get; set; }
    public SideDTO? Side { get; set; }

    public CreateLobbyCommand(string username, int? lobbyId, SideDTO? side)
    {
        Username = username;
        LobbyId = lobbyId;
        Side = side;
    }
}

public class CreateLobbyCommandHandler : IChessRequestHandler<CreateLobbyCommand, LobbyJoinedDTO>
{
    private readonly LobbyValidator validator;

    public CreateLobbyCommandHandler(LobbyValidator validator)
    {
        this.validator = validator;
    }

    public Task<IChessResponse<LobbyJoinedDTO>> Handle(CreateLobbyCommand request, CancellationToken cancellationToken)
    {
        request.Username = validator.ValidateUsername(request.Username);
        var newLobbyId = validator.ValidateLobbyId(request.LobbyId);

        var key = Guid.NewGuid();
        var player = new Player(request.Username, key);
        var lobby = new Models.Lobby(newLobbyId, player, validator.ValidateSide(request.Side));

        validator.Lobbies.Add(lobby);

        return Task.FromResult(
               ChessResponse.Created("Lobby created successfully!",
               new LobbyJoinedDTO
               {
                   LobbyId = newLobbyId,
                   Key = key,
                   PlayingSide = lobby.GetSide(player),
                   White = lobby.GetPlayerDTO(lobby.WhitePlayer),
                   Black = lobby.GetPlayerDTO(lobby.BlackPlayer),
               }));
    }
}
