namespace ChessServices.DTOs;

public class ChessBoardDTO : BaseResponseData
{
    public string FEN { get; set; }
    public string PGN { get; set; }
}
