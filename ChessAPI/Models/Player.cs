namespace ChessAPI.Models;

public class Player
{
    public string Username { get; }
    public PieceColor Color { get; set; }
    public Guid Key { get; }

    public WebSocket Socket { get; set; }

    public double Score { get; set; }
    public bool PendingDraw { get; set; }
    public bool PendingRematch { get; set; }

    public Player(string username, Guid key, WebSocket socket = null)
    {
        Username = username;
        Key = key;
        Socket = socket;
        PendingDraw = false;
        PendingRematch = false;
    }

    public void ResetPendings()
    {
        PendingDraw = false;
        PendingRematch = false;
    }

    public void Notify(string body)
    {
        if (Socket != null)
        {
            if (Socket.State == WebSocketState.Open)
            {
                // todo
            }
        }
    }
}
