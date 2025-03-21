namespace sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

public record OrderChangedV1Message(
    string OrderAccountId,
    string OrderId,
    OrderType OrderType,
    decimal OrderPrice,
    decimal OrderQuantityToFill,
    string OrderSecurityId,
    OrderDirection OrderDirection,
    decimal OrderTriggerPrice
);