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

    public ConcurrentDictionary<Guid, ChannelWriter<string>> SpectatorsChannels { get; }

    public Lobby(int lobbyId, Player player, PieceColor side)
    {
        SpectatorsChannels = new ConcurrentDictionary<Guid, ChannelWriter<string>>();
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
        for (int i = SpectatorsChannels.Count - 1; i >= 0; i--)
        {
            SpectatorsChannels.ElementAt(i).Value.TryComplete();
        }

        WhitePlayer?.CloseHosts();
        BlackPlayer?.CloseHosts();
    }

    public Task NotifySpectatorsAsync(Func<string> generateBodyFunc)
    {
        return Task.Run(async () =>
        {
            var body = generateBodyFunc();
            foreach (var channel in SpectatorsChannels)
            {
                await channel.Value.WriteAsync(body);
            }
        });
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

            BlackPlayer?.NotifyAsync(() => Tools.Serialize(
                        new OpponentJoinedDTO
                        {
                            NotificationType = NotificationType.Joined,
                            Opponent = GetPlayerDTO(player),
                        }));
        }
        else if (BlackPlayer == null)
        {
            BlackPlayer = player;
            BlackPlayer.Color = PieceColor.Black;
            InitializeBoard();

            WhitePlayer?.NotifyAsync(() => Tools.Serialize(
                        new OpponentJoinedDTO
                        {
                            NotificationType = NotificationType.Joined,
                            Opponent = GetPlayerDTO(player),
                        }));
        }
        else throw new LobbyException($"Lobby {LobbyId} is full.");

        NotifySpectatorsAsync(() => Tools.Serialize(
            new PlayerJoinedDTO
            {
                NotificationType = NotificationType.Joined,
                JoinedPlayer = GetPlayerDTO(player),
                Side = GetSide(player)
            }));
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
                BlackPlayer.NotifyAsync(() => "todo body left");
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
                WhitePlayer.NotifyAsync(() => "todo body left");
                WhitePlayer.Score = 0;
                WhitePlayer.ResetPendings();
            }
        }
        NotifySpectatorsAsync(() => "todo");
    }

    public void MakeMove(string move, Guid key)
    {
        var (player, opponent) = ValidatePlayer(key);
        ValidateOpponentLeft(opponent);

        if (player.Color != Board.Turn)
            throw new LobbyException("Attempt to move pieces of opponent.");

        if (Board.Move(move))
        {
            opponent.NotifyAsync(() => Tools.Serialize(
                new OpponentMovedDTO
                {
                    NotificationType = NotificationType.MovedPiece,
                    Move = Board.MovesToSan.Last(),
                    Board = GetBoardDTO(),
                }));
        }
        else throw new LobbyException($"Move {move} is not valid.") { Board = Board };

        NotifySpectatorsAsync(() => Tools.Serialize(
            new PlayerMovedDTO
            {
                NotificationType = NotificationType.MovedPiece,
                Move = Board.MovesToSan.Last(),
                Board = GetBoardDTO(),
                Player = GetPlayerDTO(player),
                Side = GetSide(player)
            }));
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
        opponent.NotifyAsync(() => "todo draw offer");
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
            opponent.NotifyAsync(() => "todo body draw declined");
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
        opponent.NotifyAsync(() => "todo body rematch offer");
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
            opponent.NotifyAsync(() => "todo body rematch declined");
        }
        else throw new LobbyException("Please use RematchOffer to offer rematch.");
    }

    private void HandleRematchConfirmed()
    {
        WhitePlayer.NotifyAsync(() => "todo rematch body");// black
        BlackPlayer.NotifyAsync(() => "todo rematch body");// white

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

    public PlayerFullDTO GetFullPlayerDTO(Player player)
    {
        if (player is null)
            return null;

        return new PlayerFullDTO
        {
            Username = player.Username,
            Score = player.Score,
            PendingDraw = player.PendingDraw,
            PendingRematch = player.PendingRematch,
            Connections = player.Channels.Count,
        };
    }

    public ChessBoardDTO GetBoardDTO()
    {
        if (Board is null)
            return null;

        lock (this)
        {
            return new ChessBoardDTO
            {
                FEN = Board.ToFen(),
                PGN = Board.ToPgn()
            };
        }
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
