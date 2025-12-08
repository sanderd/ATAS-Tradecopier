// ATAS.Strategies namespace
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using ATAS.DataFeedsCore;
using ATAS.Indicators;

// Supporting types
public class StrategyStateChangedEventArgs { }
public class StrategyNotificationEventArgs { }

namespace ATAS.Strategies.Chart
{
    // Supporting types and interfaces
    public interface IChartStrategy { }
    public interface IStrategy { }
    public interface ILoggerSource 
    {
        Guid LoggerId { get; set; }
        string LoggerName { get; set; }
    }

    // Enum types
    public enum StrategyStates { }
    public enum LoggingLevel { Info, Warning, Error, Critical }
    
    // Collections namespace mock
    namespace Utils.Common.Collections.Synchronized
    {
        public interface ICachedCollection<T> : IEnumerable<T> { }
    }

    // ChartStrategy - Inherits from ATAS.Indicators.Indicator (from separate assembly)
    [Category("Other")]
    public abstract class ChartStrategy : Indicator, IChartStrategy, IStrategy, INotifyPropertyChanged, ILoggerSource
    {
        // Constructor matching real ATAS
        protected ChartStrategy(bool useCandles = false) : base(useCandles)
        {
            throw new NotImplementedException("Reference assembly");
        }

        // Properties matching real ATAS ChartStrategy
        [Browsable(false)]
        public IEnumerable<MyTrade> MyTrades { get; }
        
        [Browsable(false)]
        public StrategyStates State { get; protected set; }
        
        public bool IsActivated { get; }
        
        [Browsable(false)]
        public decimal CurrentPosition { get; }
        
        [Browsable(false)]
        public decimal AveragePrice { get; }
        
        [Browsable(false)]
        public decimal OpenPnL { get; }
        
        [Browsable(false)]
        public decimal ClosedPnL { get; }
        
        [Browsable(false)]
        public Security Security { get; set; }
        
        [Browsable(false)]
        public Portfolio Portfolio { get; set; }
        
        [Browsable(false)]
        public TPlusLimits? TPlusLimit { get; set; }
        
        [Browsable(false)]
        public MarketDataArg BestBid { get; }
        
        [Browsable(false)]
        public MarketDataArg BestAsk { get; }
        
        [Browsable(false)]
        public IDataFeedConnector Connector { get; set; }
        
        [Browsable(false)]
        public Guid LoggerId { get; set; }
        
        [Browsable(false)]
        public string LoggerName { get; set; }
        
        [Browsable(false)]
        public LoggingLevel LoggingLevel { get; set; }
        
        [Browsable(false)]
        public ILoggerSource ParentLoggerSource { get; set; }
        
        [Browsable(false)]
        public Utils.Common.Collections.Synchronized.ICachedCollection<ILoggerSource> ChildLoggerSources { get; }
        
        [Browsable(false)]
        public IEnumerable<Order> Orders { get; }

        // Events
        public event EventHandler<StrategyStateChangedEventArgs> StateChanged;
        public event EventHandler<StrategyNotificationEventArgs> ShowNotification;
        public event Action LoggerSettingsChanged;
        public event PropertyChangedEventHandler? PropertyChanged; // For INotifyPropertyChanged

        // Methods matching real ATAS ChartStrategy
        public void CancelOrder(Order order) { throw new NotImplementedException("Reference assembly"); }
        public Task CancelOrderAsync(Order order) { throw new NotImplementedException("Reference assembly"); }
        public void ModifyOrder(Order order, Order newOrder) { throw new NotImplementedException("Reference assembly"); }
        public Task ModifyOrderAsync(Order order, Order newOrder) { throw new NotImplementedException("Reference assembly"); }
        public void OpenOrder(Order order) { throw new NotImplementedException("Reference assembly"); }
        public Task OpenOrderAsync(Order order) { throw new NotImplementedException("Reference assembly"); }
        
        [Obsolete("Use StartAsync instead.")]
        public void Start() { throw new NotImplementedException("Reference assembly"); }
        public Task StartAsync() { throw new NotImplementedException("Reference assembly"); }
        
        [Obsolete("Use StopAsync instead.")]
        public void Stop() { throw new NotImplementedException("Reference assembly"); }
        public Task StopAsync() { throw new NotImplementedException("Reference assembly"); }
        
        [Obsolete("Use StopAsync instead.")]
        public void StopWithNotification(string message) { throw new NotImplementedException("Reference assembly"); }
        public Task StopWithNotificationAsync(string message) { throw new NotImplementedException("Reference assembly"); }
        
        [Obsolete("Use SuspendAsync instead.")]
        public void Suspend() { throw new NotImplementedException("Reference assembly"); }
        public Task SuspendAsync() { throw new NotImplementedException("Reference assembly"); }
        
        // Virtual methods that can be overridden
        protected virtual bool CanProcess(int bar) { throw new NotImplementedException("Reference assembly"); }
        protected virtual void OnCurrentPositionChanged() { }
        protected virtual void OnStarted() { }
        protected virtual void OnStopped() { }
        protected virtual void OnStopping() { }
        protected virtual void OnSuspended() { }
        protected virtual void OnInitialize() { }
        protected virtual void OnCalculate(int bar, decimal value) { }
        protected virtual void OnNewOrder(Order order) { }
        protected virtual void OnOrderChanged(Order order) { }
        protected virtual void OnPositionChanged(Position position) { }
        
        // Utility methods
        protected void RaiseShowNotification(string message, string title = null, LoggingLevel level = LoggingLevel.Info) { }
        protected bool SetProperty<TValue>(ref TValue storage, TValue newValue, string propertyName, Action<TValue, TValue> onChanged = null) { return false; }
        protected decimal ShrinkPrice(decimal price) { return price; }

        // TradingManager property - inherited from ATAS.Indicators.Indicator
        // No need to redeclare it here since it's inherited from Indicator
    }
}