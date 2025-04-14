namespace sadnerd.io.ATAS.OrderEventHub.Data.Models;

public class CopyStrategy
{
    public int Id { get; set; }
    public string AtasAccountId { get; set; }
    public AtasAccount AttasAccount { get; set; }
    public string TopstepAccountId { get; set; }
    public TopstepAccount TopstepAccount { get; set; }
    public string AtasContract { get; set; }
    public string TopstepContract { get; set; }
    public int ContractMultiplier { get; set; }
}