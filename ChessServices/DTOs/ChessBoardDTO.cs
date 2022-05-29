namespace ChessServices.DTOs;

public class ChessBoardDTO : ChessResponseDTO
{
    public string FEN { get; set; }
    public string PGN { get; set; }
}
