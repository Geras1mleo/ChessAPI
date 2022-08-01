namespace ChessServices
{
    public interface ILobbyValidator
    {
        Lobby GetLobby(int lobbyId);
        int ValidateLobbyId(int? lobbyId);
        PieceColor ValidateSide(SideDTO? side);
        string ValidateUsername(string username);
    }
}