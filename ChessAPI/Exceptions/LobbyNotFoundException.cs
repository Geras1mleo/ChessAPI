namespace ChessAPI.Exceptions;

internal class LobbyNotFoundException : Exception
{
    public LobbyNotFoundException(int lobbyId) : base("Lobby: " + lobbyId + " not found") { }

    public LobbyNotFoundException(string message) : base(message) { }

    public LobbyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
