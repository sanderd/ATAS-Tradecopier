using ATAS.DataFeedsCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

public class PositionToPositionChangedV1MessageMapper : IPositionToPositionChangedV1MessageMapper
{
    public PositionChangedV1Message Map(Position position)
    {
        return new PositionChangedV1Message(
            PositionAccountId: position.AccountID,
            AveragePrice: position.AveragePrice,
            PositionSecurityId: position.SecurityId,
            OpenVolume: position.OpenVolume,
            Volume: position.Volume
        );
    }
}