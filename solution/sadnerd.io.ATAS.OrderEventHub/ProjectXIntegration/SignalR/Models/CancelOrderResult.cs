namespace sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.SignalR.Models;

public record CancelOrderResult(bool Success, bool AlreadyFilled = false);