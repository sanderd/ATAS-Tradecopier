using ATAS.DataFeedsCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

public static class OrderDirectionsToOrderDirectionMapper {
    public static OrderDirection Map(OrderDirections orderDirection)
    {
        switch (orderDirection)
        {
            case OrderDirections.Buy:
                return OrderDirection.Buy;
            case OrderDirections.Sell:
                return OrderDirection.Sell;
            default:
                throw new ArgumentOutOfRangeException(nameof(orderDirection), orderDirection, "Direction is not supported");
        }
    }
}