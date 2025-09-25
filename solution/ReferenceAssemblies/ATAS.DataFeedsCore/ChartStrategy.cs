// Reference assembly for ATAS.DataFeedsCore
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ATAS.DataFeedsCore
{
    // Core Strategy Base Class
    public abstract class ChartStrategy
    {
        public TradingManager TradingManager { get; protected set; } = new TradingManager();
        
        protected virtual void OnInitialize() { }
        protected virtual void OnStarted() { }
        protected virtual void OnSuspended() { }
        protected virtual void OnStopping() { }
        protected virtual void OnCalculate(int bar, decimal value) { }
        protected virtual void OnNewOrder(Order order) { }
        protected virtual void OnOrderChanged(Order order) { }
        protected virtual void OnPositionChanged(Position position) { }
    }

    // Order Types and Enums
    public enum OrderTypes
    {
        Unknown = 0,
        Market = 1,
        Limit = 2,
        Stop = 3,
        StopLimit = 4
    }

    public enum OrderDirections
    {
        Buy = 0,
        Sell = 1
    }

    public enum OrderStates
    {
        Unknown = 0,
        PendingSubmit = 1,
        PendingCancel = 2,
        PendingReplace = 3,
        Working = 4,
        Filled = 5,
        Cancelled = 6,
        Rejected = 7,
        Expired = 8,
        Active = 9,      // Add missing enum values
        Done = 10,
        Failed = 11,
        None = 12
    }

    // Order Class
    public class Order
    {
        public string AccountID { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public OrderTypes Type { get; set; }
        public decimal Price { get; set; }
        public decimal QuantityToFill { get; set; }
        public decimal Unfilled { get; set; }
        public string SecurityId { get; set; } = string.Empty;
        public OrderDirections Direction { get; set; }
        public decimal TriggerPrice { get; set; }
        public OrderStates State { get; set; }
        public bool Canceled { get; set; }
        public string Comment { get; set; } = string.Empty;
        public object? ExtendedOptions { get; set; }

        public bool IsReduceOnly()
        {
            return ExtendedOptions?.ToString()?.Contains("ReduceOnly") == true;
        }
    }

    // Position Class
    public class Position
    {
        public string SecurityId { get; set; } = string.Empty;
        public decimal Volume { get; set; }
        public string AccountID { get; set; } = string.Empty;     // Add missing properties
        public decimal AveragePrice { get; set; }
        public decimal OpenVolume { get; set; }
        
        public override int GetHashCode()
        {
            return (SecurityId?.GetHashCode() ?? 0) ^ Volume.GetHashCode();
        }
    }

    // Portfolio Class
    public class Portfolio
    {
        public string AccountID { get; set; } = string.Empty;
    }

    // Security Class
    public class Security
    {
        public string SecurityId { get; set; } = string.Empty;
    }

    // Trading Manager
    public class TradingManager
    {
        public Portfolio? Portfolio { get; set; }
        public Security? Security { get; set; }
        
        public event Action<Portfolio>? PortfolioSelected;
        public event Action<Security>? SecuritySelected;
    }
}