namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

public abstract class GenericOrderEvent<T>
{
    protected GenericOrderEvent(T message)
    {
        Message = message;
    }

    public T Message { get; }
}