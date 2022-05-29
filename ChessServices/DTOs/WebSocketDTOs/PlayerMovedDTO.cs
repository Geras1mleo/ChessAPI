namespace ChessServices.DTOs.WebSocketDTOs;

public class OpponentMovedDTO : SocketNotificationDTO
{
    public string Move { get; set; }
    public ChessBoardDTO Board { get; set; }
}

/// <summary>
/// DTO for spectators
/// </summary>
public class PlayerMovedDTO : OpponentMovedDTO
{
    public PlayerDTO Player { get; set; }
    public SideDTO Side { get; set; }
}