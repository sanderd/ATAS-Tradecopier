namespace sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

public enum OrderType : int
{
    Unknown = 0,
    Limit = 1,
    Market = 2,
    Stop = 3,
    StopLimit = 4,
}