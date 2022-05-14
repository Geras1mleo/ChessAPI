namespace ChessServices.DTOs;

public class LobbyJoinedDTO : BaseResponseData
{
    public int LobbyId { get; set; }
    public Guid Key { get; set; }
    public SideDTO PlayingSide { get; set; }
    public PlayerDTO White { get; set; }
    public PlayerDTO Black { get; set; }
}
