using ATAS.DataFeedsCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

public interface IOrderToNewOrderEventV1MessageMapper
{
    NewOrderEventV1Message Map(Order order);
}