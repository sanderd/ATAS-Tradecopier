using ATAS.DataFeedsCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

public class OrderToOrderChangedV1MessageMapper : IOrderToOrderChangedV1MessageMapper
{
    public OrderChangedV1Message Map(Order order)
    {
        return new OrderChangedV1Message(
            OrderAccountId: order.AccountID,
            OrderId: order.Id,
            OrderType: OrderTypesToOrderTypeMapper.Map(order.Type),
            OrderPrice: order.Price,
            OrderQuantityToFill: order.QuantityToFill,
            OrderSecurityId: order.SecurityId,
            OrderDirection: OrderDirectionsToOrderDirectionMapper.Map(order.Direction),
            OrderTriggerPrice: order.TriggerPrice,
            OrderStatus: OrderStatesToOrderStatusMapper.Map(order.State)
        );
    }
}