namespace ChessServices.DTOs.WebSocketDTOs;

[JsonConverter(typeof(StringEnumConverter))]
public enum NotificationType
{
    None,
    Joined,
    Left,
    MovedPiece,
    // todo...
}

public class SocketNotificationDTO
{
    public NotificationType NotificationType { get; set; }
}
