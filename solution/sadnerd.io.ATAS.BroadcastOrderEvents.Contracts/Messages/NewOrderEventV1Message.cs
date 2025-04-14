
namespace sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

public record NewOrderEventV1Message(
    string OrderAccountId,
    string OrderId,
    OrderType OrderType,
    decimal OrderPrice,
    decimal OrderQuantityToFill,
    string OrderSecurityId,
    OrderDirection OrderDirection,
    decimal OrderTriggerPrice,
    bool IsReduceOnly,
    string? Comment
);
