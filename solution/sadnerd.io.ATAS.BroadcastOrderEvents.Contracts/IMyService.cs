namespace sadnerd.io.ATAS.BroadcastOrderEvents.Contracts;

public interface IMyService
{
    void NewOrder(NewOrderEventV1Message newOrder);
    void NewOrder(bool test);
}