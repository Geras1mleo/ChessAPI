namespace ChessServices.DTOs.WebSocketDTOs;

public class IdentifyPlayerDTO : IdentifyLobbyDTO
{
    public Guid Key { get; set; }
}
