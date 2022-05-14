namespace ChessServices.DTOs;

public class PlayerDTO
{
    public string Username { get; set; }
}

public class PlayerFullDTO
{
    public string Username { get; set; }
    public double Score { get; set; }
    public bool PendingDraw { get; set; }
    public bool PendingRematch { get; set; }
    public int Connections { get; set; }
}