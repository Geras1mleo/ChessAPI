namespace ChessAPI.Models;

public class Player
{
    public List<Channel<string>> Channels { get; }

    public string Username { get; }
    public PieceColor Color { get; set; }
    public Guid Key { get; }

    public double Score { get; set; }
    public bool PendingDraw { get; set; }
    public bool PendingRematch { get; set; }

    public Player(string username, Guid key)
    {
        Username = username;
        Key = key;
        PendingDraw = false;
        PendingRematch = false;
        Channels = new List<Channel<string>>();
    }

    ~Player()
    {
        CloseHosts();
    }

    public void CloseHosts()
    {
        foreach (var item in Channels)
        {
            if (!item.Reader.Completion.IsCompleted)
            {
                item.Writer.Complete();
            }
        }
    }

    public void ResetPendings()
    {
        PendingDraw = false;
        PendingRematch = false;
    }

    public async Task Notify(string body)
    {
        foreach (var channel in Channels)
        {
            if (!channel.Reader.Completion.IsCompleted)
            {
                await channel.Writer.WriteAsync(body);
            }
        }
    }
}
