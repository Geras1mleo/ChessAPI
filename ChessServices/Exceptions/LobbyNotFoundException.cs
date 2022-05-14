namespace ChessServices.Exceptions;

public class LobbyNotFoundException : LobbyException
{
    public LobbyNotFoundException(int id) : base($"Lobby {id} has been not found...") { }

    public LobbyNotFoundException(string message) : base(message) { }

    public LobbyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
