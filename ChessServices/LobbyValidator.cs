namespace ChessServices;

public class LobbyValidator
{
    public List<Lobby> Lobbies { get; }

    public LobbyValidator(List<Lobby> lobbies)
    {
        Lobbies = lobbies;
    }

    public string ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username));

        return username.Trim();
    }

    public int ValidateLobbyId(int? lobbyId)
    {
        if (lobbyId == null)
            lobbyId = GetNewLobbyID(0);

        else if (Lobbies.Any(l => l.LobbyId == lobbyId))
            throw new Exception($"Lobby: {lobbyId} already exist.");

        if (Lobbies.Count > 100)
            throw new Exception("Lobbies overflow");

        return (int)lobbyId;
    }

    private int GetNewLobbyID(int id)
    {
        id++;
        return Lobbies.Any(l => l.LobbyId == id) ? GetNewLobbyID(id) : id;
    }

    public PieceColor ValidateSide(SideDTO? side)
    {
        return side switch
        {
            SideDTO.White => PieceColor.White,
            SideDTO.Black => PieceColor.Black,
            _ => PieceColor.White
        };
    }

    public Lobby GetLobby(int lobbyId)
    {
        return Lobbies.FirstOrDefault(l => l.LobbyId == lobbyId) ?? throw new LobbyNotFoundException($"Lobby {lobbyId} has been not found...");
    }
}
