namespace sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

public record OrderChangedV1Message(
    string AccountId,
    string OrderId,
    OrderType Type,
    decimal Price,
    decimal OriginalQuantity,
    decimal UnfilledQuantity,
    string SecurityId,
    OrderDirection Direction,
    decimal TriggerPrice,
    OrderStatus Status,
    bool Canceled,
    bool IsReduceOnly,
    string? Comment);