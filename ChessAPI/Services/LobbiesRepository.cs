namespace ChessAPI.Services;

public class LobbiesRepository
{
    readonly List<Lobby> lobbies;

    public LobbiesRepository(List<Lobby> lobbies)
    {
        this.lobbies = lobbies;
    }

    public LobbyJoinedDTO Create(string username, int? lobbyId, PieceColor? side)
    {
        username = ValidateUsername(username);
        lobbyId = ValidateLobbyId(lobbyId);

        var key = Guid.NewGuid();
        var player = new Player(username, key);
        var lobby = new Lobby((int)lobbyId, player, side);

        lobbies.Add(lobby);

        return new LobbyJoinedDTO
        {
            LobbyId = (int)lobbyId,
            User = lobby.GetPlayerDTO(player),
            Opponent = null,
            Key = key
        };
    }

    public LobbyJoinedDTO Join(int lobbyId, string username)
    {
        var lobby = GetLobby(lobbyId);
        username = ValidateUsername(username);

        var key = Guid.NewGuid();
        var player = new Player(username, key);

        lobby.Join(player);

        return new LobbyJoinedDTO
        {
            LobbyId = lobby.LobbyId,
            User = lobby.GetPlayerDTO(player),
            Opponent = lobby.GetPlayerDTO(lobby.GetOppositePlayer(player)),
            Key = key
        };
    }

    public Channel<string> Host(IdentifyPlayerDTO identify)
    {
        var player = GetLobby(identify.LobbyId).GetPlayer(identify.Key);

        var channel = Channel.CreateUnbounded<string>();

        player.Channels.Add(channel);

        return channel;
    }

    public void Leave(int lobbyId, Guid key)
    {
        var lobby = GetLobby(lobbyId);
        lobby.LeaveLobby(key);

        if (lobby.FirstPlayer is null && lobby.SecondPlayer is null)
            lobbies.Remove(lobby);
    }

    private int ValidateLobbyId(int? lobbyId)
    {
        if (lobbyId == null)
            lobbyId = GetNewLobbyID(0);

        else if (lobbies.Any(l => l.LobbyId == lobbyId))
            throw new Exception($"Lobby: {lobbyId} already exist.");

        if (lobbies.Count > 100)
            throw new Exception("Lobbies overflow");

        return (int)lobbyId;
    }

    private Lobby GetLobby(int lobbyId)
    {
        return lobbies.FirstOrDefault(l => l.LobbyId == lobbyId) ?? throw new LobbyNotFoundException($"Lobby {lobbyId} has been not found...");
    }

    private static string ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username));

        return username.Trim();
    }

    private int GetNewLobbyID(int id)
    {
        id++;
        return lobbies.Any(l => l.LobbyId == id) ? GetNewLobbyID(id) : id;
    }
}
