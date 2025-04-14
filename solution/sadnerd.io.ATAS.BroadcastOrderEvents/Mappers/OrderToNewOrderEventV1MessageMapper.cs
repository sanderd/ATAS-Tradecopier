using ATAS.DataFeedsCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

public class OrderToNewOrderEventV1MessageMapper : IOrderToNewOrderEventV1MessageMapper
{
    public NewOrderEventV1Message Map(Order order)
    {
        return new NewOrderEventV1Message(
            OrderAccountId: order.AccountID,
            OrderId: order.Id,
            OrderType: OrderTypesToOrderTypeMapper.Map(order.Type),
            OrderPrice: order.Price,
            OrderQuantityToFill: order.QuantityToFill,
            OrderSecurityId: order.SecurityId,
            OrderDirection: OrderDirectionsToOrderDirectionMapper.Map(order.Direction),
            OrderTriggerPrice: order.TriggerPrice,
            IsReduceOnly: order.IsReduceOnly(),
            Comment: order.Comment
        );
    }
}