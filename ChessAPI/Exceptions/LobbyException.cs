namespace ChessAPI.Exceptions;

internal class LobbyException : Exception
{
    public ChessBoard Board { get; set; }

    public LobbyException(string message) : base(message) { }

    public LobbyException(string message, Exception innerException) : base(message, innerException) { }
}
