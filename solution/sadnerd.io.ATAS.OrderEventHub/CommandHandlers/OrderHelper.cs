namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers;

public static class OrderHelper
{
    public static bool IsTakeProfit(string comment, bool isReduceOnly)
    {
        return comment == "TP" || isReduceOnly;
    }

    public static bool IsStopLoss(string comment, bool isReduceOnly)
    {
        return comment == "SL" || isReduceOnly;
    }
}