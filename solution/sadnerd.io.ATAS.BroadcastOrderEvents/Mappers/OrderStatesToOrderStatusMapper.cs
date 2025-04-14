using ATAS.DataFeedsCore;
using OrderStatus = sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages.OrderStatus;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

public static class OrderStatesToOrderStatusMapper
{
    public static OrderStatus Map(OrderStates orderState)
    {
        switch (orderState)
        {
            case OrderStates.Active:
                return OrderStatus.Active;
            case OrderStates.Done:
                return OrderStatus.Done;
            case OrderStates.Failed:
                return OrderStatus.Failed;
            case OrderStates.None:
                return OrderStatus.None;
            default:
                throw new ArgumentOutOfRangeException(nameof(orderState), orderState, "OrderState is not supported");
        }
    }
}