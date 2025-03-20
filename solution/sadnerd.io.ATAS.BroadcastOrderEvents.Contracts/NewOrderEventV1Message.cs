namespace sadnerd.io.ATAS.BroadcastOrderEvents.Contracts;

public record NewOrderEventV1Message(
    string OrderAccountId,
    string OrderId,
    //OrderTypes OrderType,
    decimal OrderPrice,
    decimal OrderQuantityToFill,
    string OrderSecurityId,
    //OrderDirections OrderDirection,
    decimal OrderTriggerPrice);