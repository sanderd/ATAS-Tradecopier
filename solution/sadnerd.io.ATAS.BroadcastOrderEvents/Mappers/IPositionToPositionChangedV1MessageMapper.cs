using ATAS.DataFeedsCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

public interface IPositionToPositionChangedV1MessageMapper
{
    PositionChangedV1Message Map(Position position);
}