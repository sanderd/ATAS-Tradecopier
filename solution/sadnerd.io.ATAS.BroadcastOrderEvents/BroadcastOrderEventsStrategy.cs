using ATAS.DataFeedsCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using OFT.Attributes;
using Utils.Common.Logging;
using System.Net;
using ATAS.Strategies.Chart;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

namespace sadnerd.io.ATAS.BroadcastOrderEvents
{
    [DisplayName("Broadcast Order Events")]
    [Display(Name = "Broadcast Order Events", Description = "Broadcast Order Events")]
    [HelpLink("https://github.com/sanderd/ATAS-Indicators")]
    public class BroadcastOrderEventsStrategy : ChartStrategy
    {
        private bool _isStarted = false;
        private readonly IOrderToNewOrderEventV1MessageMapper _orderToNewOrderEventMapper;
        private readonly IOrderEventHubDispatchService _orderEventHubDispatchService;

        // Added cuz ATAS and DI duh.
        public BroadcastOrderEventsStrategy() : this(
            orderToNewOrderEventMapper: new OrderToNewOrderEventV1MessageMapper(),
            orderEventHubDispatchService:new ServiceWireClientOrderEventHubDispatchService(new IPEndPoint(IPAddress.Loopback, 12345))
        )
        {
        }

        public BroadcastOrderEventsStrategy(
            IOrderToNewOrderEventV1MessageMapper orderToNewOrderEventMapper,
            IOrderEventHubDispatchService orderEventHubDispatchService
        )
        {
            _orderToNewOrderEventMapper = orderToNewOrderEventMapper;
            _orderEventHubDispatchService = orderEventHubDispatchService;
        }

        protected override void OnNewOrder(Order order)
        {
            if (!_isStarted) return;
            
            this.LogWarn("OnNewOrder: " + order);

            var mappedMessage = _orderToNewOrderEventMapper.Map(order);
            _orderEventHubDispatchService.NewOrder(mappedMessage);
        }

        protected override void OnOrderChanged(Order order)
        {
            if (!_isStarted) return;
            
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

        protected override void OnStarted()
        {
            _isStarted = true;
         
            base.OnStarted();
        }

        protected override void OnSuspended()
        {
            _isStarted = false;

            base.OnSuspended();
        }

        protected override void OnStopping()
        {
            _isStarted = false;

            base.OnStopping();
        }
    }
}