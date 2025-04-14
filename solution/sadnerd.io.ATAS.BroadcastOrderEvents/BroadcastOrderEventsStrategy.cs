using ATAS.DataFeedsCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using OFT.Attributes;
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
        private readonly IOrderToOrderChangedV1MessageMapper _orderToOrderChangedEventMapper;
        private readonly IPositionToPositionChangedV1MessageMapper _positionToPositionChangedEventMapper;

        private readonly IDictionary<string, decimal> _lastReportedPosition = new Dictionary<string, decimal>();

        // Added cuz ATAS and DI duh.
        public BroadcastOrderEventsStrategy() : this(
            orderToNewOrderEventMapper: new OrderToNewOrderEventV1MessageMapper(),
            orderEventHubDispatchService: new ServiceWireClientOrderEventHubDispatchService(new IPEndPoint(IPAddress.Loopback, 12345)), 
            orderToOrderChangedEventMapper: new OrderToOrderChangedV1MessageMapper(),
            positionToPositionChangedEventMapper: new PositionToPositionChangedV1MessageMapper()
        )
        {
        }

        public BroadcastOrderEventsStrategy(
            IOrderToNewOrderEventV1MessageMapper orderToNewOrderEventMapper,
            IOrderEventHubDispatchService orderEventHubDispatchService,
            IOrderToOrderChangedV1MessageMapper orderToOrderChangedEventMapper,
            IPositionToPositionChangedV1MessageMapper positionToPositionChangedEventMapper
        )
        {
            _orderToNewOrderEventMapper = orderToNewOrderEventMapper;
            _orderEventHubDispatchService = orderEventHubDispatchService;
            _orderToOrderChangedEventMapper = orderToOrderChangedEventMapper;
            _positionToPositionChangedEventMapper = positionToPositionChangedEventMapper;
        }

        protected override void OnNewOrder(Order order)
        {
            if (!_isStarted) return;
            
            var mappedMessage = _orderToNewOrderEventMapper.Map(order);
            _orderEventHubDispatchService.NewOrder(mappedMessage);
        }

        protected override void OnOrderChanged(Order order)
        {
            if (!_isStarted) return;
            
            var mappedMessage = _orderToOrderChangedEventMapper.Map(order);
            _orderEventHubDispatchService.OrderChanged(mappedMessage);
        }

        //protected override void OnNewMyTrade(MyTrade myTrade)
        //{
        //    this.LogWarn("OnNewMyTrade: " + myTrade.ToString());
        //}

        //protected override void OnPortfolioChanged(Portfolio portfolio)
        //{
        //    this.LogWarn("OnPortfolioChanged: " + portfolio.ToString());
        //}

        protected override void OnPositionChanged(Position position)
        {
            if (!_isStarted) return;
            
            bool report = false;
            if (!_lastReportedPosition.ContainsKey(position.SecurityId))
            {
                _lastReportedPosition.Add(position.SecurityId, position.Volume);
                report = true;
            } else if (_lastReportedPosition[position.SecurityId] != position.Volume)
            {
                _lastReportedPosition[position.SecurityId] = position.Volume;
                report = true;
            }

            if (report)
            {
                var mappedMessage = _positionToPositionChangedEventMapper.Map(position);
                _orderEventHubDispatchService.PositionChanged(mappedMessage);
            }
        }

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
            _lastReportedPosition.Clear();

            base.OnStopping();
        }
    }
}