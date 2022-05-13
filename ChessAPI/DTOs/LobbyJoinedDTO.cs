namespace ChessAPI.DTOs;

public class LobbyJoinedDTO
{
    public int LobbyId { get; set; }
    public Guid Key { get; set; }
    public PlayerDTO User { get; set; }
    public PlayerDTO Opponent { get; set; }
}
