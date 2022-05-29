namespace ChessServices.Models;

public class Player
{
    public ConcurrentDictionary<Guid, ChannelWriter<string>> Channels { get; }

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
        Channels = new ConcurrentDictionary<Guid, ChannelWriter<string>>();
    }

    ~Player()
    {
        CloseHosts();
    }

    public void CloseHosts()
    {
        for (int i = Channels.Count - 1; i >= 0; i--)
        {
            Channels.ElementAt(i).Value.TryComplete();
        }
    }

    public void ResetPendings()
    {
        PendingDraw = false;
        PendingRematch = false;
    }

    public Task NotifyAsync(Func<string> generateBodyFunc)
    {
        return Task.Run(async () =>
        {
            var body = generateBodyFunc();
            foreach (var channel in Channels)
            {
                await channel.Value.WriteAsync(body);
            }
        });
    }
}
