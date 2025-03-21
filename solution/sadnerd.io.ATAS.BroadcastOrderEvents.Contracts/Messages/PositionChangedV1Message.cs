namespace sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

public record PositionChangedV1Message(
    string PositionAccountId,
    decimal AveragePrice,
    string PositionSecurityId,
    decimal OpenVolume,
    decimal Volume
);
