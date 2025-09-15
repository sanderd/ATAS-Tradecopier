namespace sadnerd.io.ATAS.OrderEventHub.Data.Models;

public class CopyStrategy
{
    public int Id { get; set; }
    public string AtasAccountId { get; set; }
    public AtasAccount AttasAccount { get; set; }
    public string ProjectXAccountId { get; set; }
    public ProjectXAccount ProjectXAccount { get; set; }
    public string AtasContract { get; set; }
    public string ProjectXContract { get; set; }
    public int ContractMultiplier { get; set; }
}