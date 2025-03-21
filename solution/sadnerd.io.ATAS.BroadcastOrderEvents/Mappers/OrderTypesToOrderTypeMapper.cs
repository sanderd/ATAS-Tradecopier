using ATAS.DataFeedsCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

public static class OrderTypesToOrderTypeMapper
{
    public static OrderType Map(OrderTypes orderType)
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
}