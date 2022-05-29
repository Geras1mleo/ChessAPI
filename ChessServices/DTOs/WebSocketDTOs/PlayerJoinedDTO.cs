namespace ChessServices.DTOs.WebSocketDTOs;

/// <summary>
/// DTO for spectators
/// </summary>
public class PlayerJoinedDTO : SocketNotificationDTO
{
    public PlayerDTO JoinedPlayer { get; set; }
    public SideDTO Side { get; set; }
}

public class OpponentJoinedDTO : SocketNotificationDTO
{
    public PlayerDTO Opponent { get; set; }
}
