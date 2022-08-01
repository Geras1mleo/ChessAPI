namespace ChessServices;

public interface ILobbyRepository
{
    public List<Lobby> Lobbies { get; }
}

public class LobbyRepository : ILobbyRepository
{
    public List<Lobby> Lobbies { get; } = new();
}