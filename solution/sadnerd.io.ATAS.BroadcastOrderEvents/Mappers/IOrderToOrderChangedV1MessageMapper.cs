using ATAS.DataFeedsCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

public interface IOrderToOrderChangedV1MessageMapper
{
    OrderChangedV1Message Map(Order order);
}