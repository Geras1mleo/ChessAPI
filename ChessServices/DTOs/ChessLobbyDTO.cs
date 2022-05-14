namespace ChessServices.DTOs;

public class ChessLobbyDTO : BaseResponseData
{
    public PlayerFullDTO WhitePlayer { get; set; }
    public PlayerFullDTO BlackPlayer { get; set; }
    public ChessBoardDTO Board { get; set; }
    public int Spectators { get; set; }
}
