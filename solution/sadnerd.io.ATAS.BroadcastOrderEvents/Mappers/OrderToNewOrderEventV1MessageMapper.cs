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
            OrderType: Map(order.Type),
            OrderPrice: order.Price,
            OrderQuantityToFill: order.QuantityToFill,
            OrderSecurityId: order.SecurityId,
            OrderDirection: Map(order.Direction),
            OrderTriggerPrice: order.TriggerPrice
        );
    }

    private OrderType Map(OrderTypes orderType)
    {
        switch (orderType)
        {
            case OrderTypes.Limit:
                return OrderType.Limit;
            case OrderTypes.Market:
                return OrderType.Market;
            case OrderTypes.Stop:
                return OrderType.Stop;
            case OrderTypes.StopLimit:
                return OrderType.StopLimit;
            case OrderTypes.Unknown:
                return OrderType.Unknown;
            default:
                throw new ArgumentOutOfRangeException(nameof(orderType), orderType, "OrderType is not supported");
        }
    }

    private OrderDirection Map(OrderDirections orderDirection)
    {
        switch (orderDirection)
        {
            case OrderDirections.Buy:
                return OrderDirection.Buy;
            case OrderDirections.Sell:
                return OrderDirection.Sell;
            default:
                throw new ArgumentOutOfRangeException(nameof(orderDirection), orderDirection, "OrderDirection is not supported");
        }
    }

}