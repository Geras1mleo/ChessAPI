namespace ChessServices.Models;

public class Lobby
{
    // todo: 
    // Board events onEndgame etc.
    // Timeouts for pending draws/rematches

    public int LobbyId { get; }
    public Player WhitePlayer { get; private set; }
    public Player BlackPlayer { get; private set; }
    public ChessBoard Board { get; private set; }

    public List<Channel<string>> Spectators { get; }

    public Lobby(int lobbyId, Player player, PieceColor? side)
    {
        Spectators = new List<Channel<string>>();
        LobbyId = lobbyId;
        switch (side)
        {
            case var e when e.Equals(PieceColor.White):
                WhitePlayer = player;
                WhitePlayer.Color = PieceColor.White;
                break;
            case var e when e.Equals(PieceColor.Black):
                BlackPlayer = player;
                BlackPlayer.Color = PieceColor.Black;
                break;
            default:
                WhitePlayer = player;
                WhitePlayer.Color = PieceColor.White;
                break;
        }
        InitializeBoard();
    }

    ~Lobby()
    {
        CloseHosts();
    }

    public void CloseHosts()
    {
        foreach (var channel in Spectators)
        {
            if (!channel.Reader.Completion.IsCompleted)
            {
                channel.Writer.Complete();
            }
        }
    }

    public Task Notify(string body)
    {
        foreach (var channel in Spectators)
        {
            if (!channel.Reader.Completion.IsCompleted)
            {
                channel.Writer.WriteAsync(body);
            }
        }
        return Task.CompletedTask;
    }

    public void InitializeBoard()
    {
        Board = new ChessBoard();
        Board.OnEndGame += BoardOnEndGame;
    }

    private void BoardOnEndGame(object sender, EndgameEventArgs e)
    {
        // todo
    }

    public void Join(Player player)
    {
        if (WhitePlayer == null)
        {
            WhitePlayer = player;
            WhitePlayer.Color = PieceColor.White;
            InitializeBoard();

            BlackPlayer?.Notify("todo generate opponent joined DTO");
        }
        else if (BlackPlayer == null)
        {
            BlackPlayer = player;
            BlackPlayer.Color = PieceColor.Black;
            InitializeBoard();

            WhitePlayer?.Notify("todo generate opponent joined DTO");
        }
        else throw new LobbyException($"Lobby {LobbyId} is full.");

        Notify("Somethind happened: joined");
    }

    public void LeaveLobby(Guid key)
    {
        var player = GetPlayer(key);

        if (player == WhitePlayer)
        {
            WhitePlayer.CloseHosts();
            WhitePlayer = null;

            if (BlackPlayer != null)
            {
                BlackPlayer.Notify("todo body left");
                BlackPlayer.Score = 0;
                BlackPlayer.ResetPendings();
            }
        }
        else if (player == BlackPlayer)
        {
            BlackPlayer.CloseHosts();
            BlackPlayer = null;

            if (WhitePlayer != null)
            {
                WhitePlayer.Notify("todo body left");
                WhitePlayer.Score = 0;
                WhitePlayer.ResetPendings();
            }
        }
    }

    public void MakeMove(string move, Guid key)
    {
        var (player, opponent) = ValidatePlayer(key);
        ValidateOpponentLeft(opponent);

        if (player.Color != Board.Turn)
            throw new LobbyException("Attempt to move pieces of opponent.");

        if (Board.Move(move))
        {
            player.Notify("todo body move");
            opponent.Notify("todo body move");
        }
        else throw new LobbyException($"Move {move} is not valid.") { Board = Board };

        Notify("Somethind happened: move made");
    }

    public void Resign(Guid key)
    {
        var (player, opponent) = ValidatePlayer(key);
        ValidateOpponentLeft(opponent);

        Board.Resign(player.Color);
    }

    public void DrawOffer(Guid key)
    {
        var (player, opponent) = ValidatePlayer(key);
        ValidateOpponentLeft(opponent);

        // todo timeout

        if (opponent.PendingDraw)
            throw new LobbyException("Please use DrawConfirm to respond to pending draw request.");

        player.PendingDraw = true;
        opponent.Notify("todo draw offer");
    }

    public void DrawConfirm(Guid key)
    {
        var opponent = GetOppositePlayer(GetPlayer(key));
        ValidateOpponentLeft(opponent);

        if (opponent.PendingDraw)
            Board.Draw();
        else
            throw new LobbyException("Please use DrawOffer to offer draw.");
    }

    public void DrawDecline(Guid key)
    {
        var opponent = GetOppositePlayer(GetPlayer(key));
        ValidateOpponentLeft(opponent);

        if (opponent.PendingDraw)
        {
            opponent.PendingDraw = false;
            opponent.Notify("todo body draw declined");
        }
        else throw new LobbyException("Please use DrawOffer to offer draw.");
    }

    public void RematchOffer(Guid key)
    {
        var (player, opponent) = ValidatePlayer(key);
        ValidateOpponentLeft(opponent);

        // todo timeout

        if (opponent.PendingRematch)
            throw new LobbyException("Please use RematchConfirm to respond to pending rematch request.");

        player.PendingRematch = true;
        opponent.Notify("todo body rematch offer");
    }

    public void RematchConfirm(Guid key)
    {
        var opponent = GetOppositePlayer(GetPlayer(key));
        ValidateOpponentLeft(opponent);

        if (opponent.PendingRematch)
            HandleRematchConfirmed();
        else
            throw new LobbyException("Please use RematchOffer to offer rematch.");
    }

    public void RematchDecline(Guid key)
    {
        var opponent = GetOppositePlayer(GetPlayer(key));
        ValidateOpponentLeft(opponent);

        if (opponent.PendingRematch)
        {
            opponent.PendingRematch = false;
            opponent.Notify("todo body rematch declined");
        }
        else throw new LobbyException("Please use RematchOffer to offer rematch.");
    }

    private void HandleRematchConfirmed()
    {
        WhitePlayer.Notify("todo rematch body");// black
        BlackPlayer.Notify("todo rematch body");// white

        // Swap
        (BlackPlayer, WhitePlayer) = (WhitePlayer, BlackPlayer);

        WhitePlayer.Color = PieceColor.White;
        BlackPlayer.Color = PieceColor.Black;

        WhitePlayer.ResetPendings();
        BlackPlayer.ResetPendings();

        InitializeBoard();
    }

    public Player GetOppositePlayer(Player player)
    {
        if (player == WhitePlayer)
            return BlackPlayer;
        else if (player == BlackPlayer)
            return WhitePlayer;

        else return null;
    }

    public Player GetPlayer(Guid key)
    {
        if (WhitePlayer?.Key == key)
            return WhitePlayer;

        else if (BlackPlayer?.Key == key)
            return BlackPlayer;

        throw new LobbyNotFoundException($"Player \"{key}\" not found...");
    }

    public PlayerDTO GetPlayerDTO(Player player)
    {
        if (player is null)
            return null;

        return new PlayerDTO
        {
            Username = player.Username,
        };
    }

    public SideDTO GetSide(Player player)
    {
        if (player.Color == PieceColor.White)
            return SideDTO.White;

        else if (player.Color == PieceColor.Black)
            return SideDTO.Black;

        throw new LobbyNotFoundException($"Player \"{player.Username}\" not found...");
    }

    private (Player player, Player opponent) ValidatePlayer(Guid key)
    {
        if (WhitePlayer?.Key == key)
            return (WhitePlayer, BlackPlayer);

        else if (BlackPlayer?.Key == key)
            return (BlackPlayer, WhitePlayer);

        throw new LobbyNotFoundException($"Player \"{key}\" not found...");
    }

    private static void ValidateOpponentLeft(Player opponent)
    {
        if (opponent == null)
            throw new LobbyException("Your opponent has left the lobby.");
    }
}
