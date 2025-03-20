
using ATAS.DataFeedsCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using ATAS.Indicators;
using OFT.Attributes;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts;
using ServiceWire.TcpIp;
using Utils.Common.Logging;
using System.Net;

namespace sadnerd.io.ATAS.BroadcastOrderEvents
{
    [DisplayName("Broadcast Order Events")]
    [Display(Name = "Broadcast Order Events", Description = "Broadcast Order Events")]
    [HelpLink("https://github.com/sanderd/ATAS-Indicators/wiki/HTF-Candles")]
    public class BroadcastOrderEventsIndicator : Indicator
    {

        public BroadcastOrderEventsIndicator()
        {
            
        }

        protected override void OnNewOrder(Order order)
        {
            this.LogWarn("OnNewOrder: " + order.ToString());

            var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 12345);
            using (var client = new TcpClient<IMyService>(ipEndpoint))
            {
                var message = new NewOrderEventV1Message(
                    order.AccountID,
                    order.Id,
                    //order.Type,
                    order.Price,
                    order.QuantityToFill,
                    order.SecurityId,
                    //order.Direction,
                    order.TriggerPrice
                );

                client.Proxy.NewOrder(true);
                client.Proxy.NewOrder(message);
            }
        }

        protected override void OnOrderChanged(Order order)
        {
            this.LogWarn("OnOrderChanged: " + order.ToString());
        }

        //protected override void OnNewMyTrade(MyTrade myTrade)
        //{
        //    this.LogWarn("OnNewMyTrade: " + myTrade.ToString());
        //}

        //protected override void OnPortfolioChanged(Portfolio portfolio)
        //{
        //    this.LogWarn("OnPortfolioChanged: " + portfolio.ToString());
        //}

        //protected override void OnPositionChanged(Position position)
        //{
        //    this.LogWarn("OnPositionChanged: " + position.ToString());
        //}
        protected override void OnCalculate(int bar, decimal value)
        {
        }
    }

    
}