namespace ChessServices;

public class LobbyValidator : ILobbyValidator
{
    private readonly ILobbyRepository lobbyRepository;

    public LobbyValidator(ILobbyRepository lobbyRepository)
    {
        this.lobbyRepository = lobbyRepository;
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

        else if (lobbyRepository.Lobbies.Any(l => l.LobbyId == lobbyId))
            throw new Exception($"Lobby: {lobbyId} already exist.");

        if (lobbyRepository.Lobbies.Count > 100)
            throw new Exception("Lobbies overflow");

        return (int)lobbyId;
    }

    private int GetNewLobbyID(int id)
    {
        id++;
        return lobbyRepository.Lobbies.Any(l => l.LobbyId == id) ? GetNewLobbyID(id) : id;
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
        return lobbyRepository.Lobbies.FirstOrDefault(l => l.LobbyId == lobbyId)
            ?? throw new LobbyNotFoundException($"Lobby {lobbyId} has been not found...");
    }
}
