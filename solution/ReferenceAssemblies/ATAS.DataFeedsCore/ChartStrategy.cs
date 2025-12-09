// Reference assembly for ATAS.DataFeedsCore
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ATAS.DataFeedsCore
{
    // Order Types and Enums - matching real ATAS structure
    public enum OrderTypes
    {
        Limit,
        Market,
        Stop,
        StopLimit,
        Unknown,
    }

    public enum OrderDirections
    {
        Buy,
        Sell,
    }

    public enum OrderStates
    {
        None,
        Active,
        Done,
        Failed,
    }

    // Supporting enums from decompiled Order
    public enum TriggerPriceType { }
    public enum TimeInForce { }
    public enum EntityType { }
    
    // Interface from Extensions - needed for IsReduceOnly extension
    public interface IOrderOptionReduceOnly
    {
        bool ReduceOnly { get; set; }
    }
    
    public class OrderExtendedOptions { }

    // Order Class - EXACTLY matching the decompiled ATAS Order structure
    public class Order : INotifyPropertyChanged
    {
        public Order() { }

        // Properties from decompiled Order - exact match
        [Category("3. Extended")]
        [DisplayName("Route")]
        public string? Route { get; set; }

        [Category("3. Extended")]
        [DisplayName("OCO group")]
        public string? OCOGroup { get; set; }

        [Category("3. Extended")]
        [DisplayName("Comment")]
        public string? Comment { get; set; }

        [Browsable(false)]
        public DateTime Time { get; set; }

        [Browsable(false)]
        public OrderStates State { get; set; }

        public TriggerPriceType TriggerPriceType { get; set; }

        [Browsable(false)]
        public object? Parent { get; set; }

        [Category("2. Common")]
        [DisplayName("Time In Force")]
        public TimeInForce TimeInForce { get; set; }

        [Browsable(false)]
        public bool IsInPosition { get; set; }

        [Browsable(false)]
        public bool Canceled { get; }

        [Browsable(false)]
        public decimal AmountBefore { get; set; }

        [Browsable(false)]
        public bool? IsAttached { get; set; }

        [Browsable(false)]
        public OrderExtendedOptions? ExtendedOptions { get; set; }

        [Browsable(false)]
        public string? ExtendedOptionsJson { get; set; }

        [Browsable(false)]
        public decimal? QuoteVolume { get; set; }

        [Browsable(false)]
        public TimeSpan Latency { get; set; }

        [Browsable(false)]
        public bool WasActive { get; set; }

        public bool AutoCancel { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [Category("2. Common")]
        [DisplayName("Volume")]
        public decimal QuantityToFill { get; set; }

        [Browsable(false)]
        public EntityType EntityType { get; }

        [Browsable(false)]
        public string? Id { get; set; }

        [Browsable(false)]
        public long ExtId { get; set; }

        [Browsable(false)]
        public long UserExtId { get; set; }

        [Browsable(false)]
        public decimal Unfilled { get; set; }

        [Browsable(false)]
        public string? RoutedAccountId { get; set; }

        [Browsable(false)]
        public string? AccountID { get; set; }

        [Category("1. Security and portfolio")]
        [DisplayName("Security")]
        public Security? Security { get; set; }

        [Category("1. Security and portfolio")]
        [DisplayName("Account")]
        public Portfolio? Portfolio { get; set; }

        [Category("2. Common")]
        [DisplayName("Type")]
        public OrderTypes Type { get; set; }

        [Category("2. Common")]
        [DisplayName("Direction")]
        public OrderDirections Direction { get; set; }

        [Category("2. Common")]
        [DisplayName("Trigger price")]
        public decimal TriggerPrice { get; set; }

        [Category("2. Common")]
        [DisplayName("Price")]
        public decimal Price { get; set; }

        [Browsable(false)]
        public string? SecurityId { get; set; }

        // Events and methods
        public event PropertyChangedEventHandler? PropertyChanged;

        public Order Clone() { throw new NotImplementedException("Reference assembly"); }
        public override string ToString() { return "Reference assembly Order"; }
        protected void OnPropertyChanged(string name) { }

        // REMOVED: IsReduceOnly() instance method - it's an extension method in real ATAS
    }

    // Extensions class with IsReduceOnly extension method - matching real ATAS
    public static class Extensions
    {
        public static bool IsReduceOnly(this Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            
            // Match the real ATAS implementation
            var optionReduceOnly = order.ExtendedOptions as IOrderOptionReduceOnly;
            return optionReduceOnly != null && optionReduceOnly.ReduceOnly;
        }

        // Add other extension methods referenced in mappers if needed
        public static decimal Filled(this Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            return order.QuantityToFill - order.Unfilled;
        }
    }

    // Position Class - keeping existing structure since it works
    public class Position
    {
        public string SecurityId { get; set; } = string.Empty;
        public decimal Volume { get; set; }
        public string AccountID { get; set; } = string.Empty;
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
}