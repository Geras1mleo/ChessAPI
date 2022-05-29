namespace ChessServices.DTOs;

[JsonConverter(typeof(StringEnumConverter))]
public enum SideDTO
{
    White = 1,
    Black = 2,
}
