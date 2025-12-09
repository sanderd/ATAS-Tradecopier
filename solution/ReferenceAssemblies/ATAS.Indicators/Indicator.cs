// ATAS.Indicators reference assembly
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using ATAS.DataFeedsCore;

// Supporting types used by Indicator
public class MyTrade { }
public class MarketDataArg { }
public class IDataFeedConnector { }
public class TPlusLimits { }
public class IIndicatorContainer { }
public class IIndicatorDataProvider { }

namespace ATAS.Indicators
{
    // ITradingManager interface - this is where it actually belongs according to decompiled Indicator
    public interface ITradingManager
    {
        Portfolio? Portfolio { get; set; }
        Security? Security { get; set; }
        
        event Action<Portfolio>? PortfolioSelected;
        event Action<Security>? SecuritySelected;
    }

    // Base indicator class that ChartStrategy inherits from - matching decompiled structure exactly
    [Category("Technical indicators")]
    public abstract class Indicator
    {
        protected Indicator(bool useCandles = false)
        {
            throw new NotImplementedException("Reference assembly");
        }

        // ITradingManager property - matching decompiled Indicator exactly
        [Browsable(false)]
        protected ITradingManager? TradingManager { get; }
        
        // Virtual methods that ChartStrategy overrides
        protected virtual void OnBestBidAskChanged(MarketDataArg depth) { }
        protected virtual void OnContainerChanged(IIndicatorContainer container) { }
        protected virtual void OnDataProviderChanged(IIndicatorDataProvider oldDataProvider, IIndicatorDataProvider newDataProvider) { }
        protected virtual void OnNewMyTrade(MyTrade myTrade) { }
    }
}