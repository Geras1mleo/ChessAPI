namespace ChessAPI.Services;

public class LobbiesService
{
    readonly List<Lobby> lobbies;

    public LobbiesService(List<Lobby> lobbies)
    {
        this.lobbies = lobbies;
    }

    private Lobby GetLobby(int lobbyId)
    {
        return lobbies.FirstOrDefault(i => i.LobbyId == lobbyId) ?? throw new LobbyNotFoundException(lobbyId);
    }

    public ChessMoveDTO Move(int lobbyId, Guid key, string move)
    {
        var lobby = GetLobby(lobbyId);
        lobby.MakeMove(move, key);

        return new ChessMoveDTO
        {
            ExecutedMove = move,
            Fen = lobby.Board.ToFen()
        };
    }

    public ChessGameDTO Explore(int lobbyId)
    {
        var lobby = GetLobby(lobbyId);

        return new ChessGameDTO
        {
            WhitePlayer = lobby.FirstPlayer?.Username,
            BlackPlayer = lobby.SecondPlayer?.Username,
            PGN = lobby.Board?.ToPgn()
        };
    }
}
