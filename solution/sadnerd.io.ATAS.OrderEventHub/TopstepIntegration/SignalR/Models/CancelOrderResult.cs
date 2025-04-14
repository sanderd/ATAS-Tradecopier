namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR.Models;

public record CancelOrderResult(bool Success, bool AlreadyFilled = false);