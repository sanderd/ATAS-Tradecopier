using ATAS.DataFeedsCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

public class OrderToOrderChangedV1MessageMapper : IOrderToOrderChangedV1MessageMapper
{
    public OrderChangedV1Message Map(Order order)
    {
        return new OrderChangedV1Message(
            AccountId: order.AccountID,
            OrderId: order.Id,
            Type: OrderTypesToOrderTypeMapper.Map(order.Type),
            Price: order.Price,
            OriginalQuantity: order.QuantityToFill,
            UnfilledQuantity: order.Unfilled,
            SecurityId: order.SecurityId,
            Direction: OrderDirectionsToOrderDirectionMapper.Map(order.Direction),
            TriggerPrice: order.TriggerPrice,
            Status: OrderStatesToOrderStatusMapper.Map(order.State),
            Canceled: order.Canceled,
            IsReduceOnly: order.ExtendedOptions?.ToString().Contains("ReduceOnly") == true, // NOTE: Yeah... Didn't find a way to access the flags directly...
            Comment: order.Comment
        );
    }
}