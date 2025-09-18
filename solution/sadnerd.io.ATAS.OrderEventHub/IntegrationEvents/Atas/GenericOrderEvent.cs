using MediatR;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.Atas;

public abstract class GenericOrderEvent<T> : IRequest
{
    protected GenericOrderEvent(T message)
    {
        Message = message;
    }

    public T Message { get; }
}