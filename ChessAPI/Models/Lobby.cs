namespace ChessAPI.Models;

public class Lobby
{
    // todo: 
    // Board events onEndgame etc.
    // Timeouts for pending draws/rematches

    public int LobbyId { get; }
    public Player FirstPlayer { get; private set; }
    public Player SecondPlayer { get; private set; }
    public ChessBoard Board { get; private set; }

    public Lobby(int lobbyId, Player player, PieceColor? side)
    {
        LobbyId = lobbyId;
        switch (side)
        {
            case var e when e.Equals(PieceColor.White):
                FirstPlayer = player;
                FirstPlayer.Color = PieceColor.White;
                break;
            case var e when e.Equals(PieceColor.Black):
                SecondPlayer = player;
                SecondPlayer.Color = PieceColor.Black;
                break;
            default:
                FirstPlayer = player;
                FirstPlayer.Color = PieceColor.White;
                break;
        }
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
        if (FirstPlayer == null)
        {
            FirstPlayer = player;
            FirstPlayer.Color = PieceColor.White;
            InitializeBoard();

            SecondPlayer?.Notify("todo generate opponent joined DTO");
        }
        else if (SecondPlayer == null)
        {
            SecondPlayer = player;
            SecondPlayer.Color = PieceColor.Black;
            InitializeBoard();

            FirstPlayer?.Notify("todo generate opponent joined DTO");
        }
        else throw new LobbyException($"Given lobby: {LobbyId} is full.");
    }

    public void LeaveLobby(Guid key)
    {
        var player = ValidatePlayer(key).player;

        if (player == FirstPlayer)
        {
            FirstPlayer = null;

            if (SecondPlayer != null)
            {
                SecondPlayer.Notify("todo body left");
                SecondPlayer.Score = 0;
                SecondPlayer.ResetPendings();
            }
        }
        else if (player == SecondPlayer)
        {
            SecondPlayer = null;

            if (FirstPlayer != null)
            {
                FirstPlayer.Notify("todo body left");
                FirstPlayer.Score = 0;
                FirstPlayer.ResetPendings();
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
        else throw new LobbyException($"Given move: {move} is not valid.") { Board = Board };
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
        var opponent = ValidatePlayer(key).opponent;
        ValidateOpponentLeft(opponent);

        if (opponent.PendingDraw)
            Board.Draw();
        else
            throw new LobbyException("Please use DrawOffer to offer draw.");
    }

    public void DrawDecline(Guid key)
    {
        var opponent = ValidatePlayer(key).opponent;
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
        var opponent = ValidatePlayer(key).opponent;
        ValidateOpponentLeft(opponent);

        if (opponent.PendingRematch)
            HandleRematchConfirmed();
        else
            throw new LobbyException("Please use RematchOffer to offer rematch.");
    }

    public void RematchDecline(Guid key)
    {
        var opponent = ValidatePlayer(key).opponent;
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
        FirstPlayer.Notify("todo rematch body");// black
        SecondPlayer.Notify("todo rematch body");// white

        // Swap
        (SecondPlayer, FirstPlayer) = (FirstPlayer, SecondPlayer);

        FirstPlayer.Color = PieceColor.White;
        SecondPlayer.Color = PieceColor.Black;

        FirstPlayer.ResetPendings();
        SecondPlayer.ResetPendings();

        InitializeBoard();
    }

    public Player GetOppositePlayer(Player player)
    {
        if (player == FirstPlayer) return SecondPlayer;
        else if (player == SecondPlayer) return FirstPlayer;
        else return null;
    }

    public PlayerDTO GetPlayerDTO(Player player)
    {
        if (player == FirstPlayer)
        {
            return new PlayerDTO
            {
                Username = FirstPlayer.Username,
                Side = Side.White
            };
        }
        else if (player == SecondPlayer)
        {
            return new PlayerDTO
            {
                Username = SecondPlayer.Username,
                Side = Side.Black
            };
        }
        else return null;
    }

    private (Player player, Player opponent) ValidatePlayer(Guid key)
    {
        if (FirstPlayer?.Key == key)
            return (FirstPlayer, SecondPlayer);

        else if (SecondPlayer?.Key == key)
            return (SecondPlayer, FirstPlayer);

        throw new LobbyException("Player not found.");
    }

    private static void ValidateOpponentLeft(Player opponent)
    {
        if (opponent == null)
            throw new LobbyException("Your opponent has left the lobby.");
    }
}
