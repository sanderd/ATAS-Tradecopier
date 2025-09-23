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
        private bool _isRegistered = false;
        private IOrderEventHubDispatchService _orderEventHubDispatchService;
        private readonly IDictionary<string, decimal> _lastReportedPosition = new Dictionary<string, decimal>();
        private string _strategyKey = string.Empty;

        // Configuration properties
        private string _serverIpAddress = "127.0.0.1";
        private int _serverPort = 12345;

        [Parameter]
        [Display(Name = "Server IP Address", GroupName = "Connection Settings")]
        public string ServerIpAddress
        {
            get => _serverIpAddress;
            set
            {
                _serverIpAddress = value;
                ReconfigureConnection();
            }
        }

        [Parameter]
        [Display(Name = "Server Port", GroupName = "Connection Settings")]
        [Range(1, 65535)]
        public int ServerPort
        {
            get => _serverPort;
            set
            {
                _serverPort = value;
                ReconfigureConnection();
            }
        }

        public BroadcastOrderEventsStrategy()
        {
        }

        protected override void OnNewOrder(Order order)
        {
            if (!ShouldProcessEvents()) return;
            
            var mappedMessage = ServiceLocator.OrderToNewOrderEventMapper.Map(order);
            _orderEventHubDispatchService.NewOrder(mappedMessage);
        }

        protected override void OnOrderChanged(Order order)
        {
            if (!ShouldProcessEvents()) return;
            
            var mappedMessage = ServiceLocator.OrderToOrderChangedEventMapper.Map(order);
            _orderEventHubDispatchService.OrderChanged(mappedMessage);
        }

        protected override void OnPositionChanged(Position position)
        {
            if (!ShouldProcessEvents()) return;
            
            bool report = false;
            string positionKey = position.ToString(); // Simple fallback approach
            decimal positionVolume = 0; // Will need to be determined from actual Position properties
            
            try
            {
                // Try to access properties that might exist - this needs to be adjusted based on actual ATAS Position object
                var prop = position.GetType().GetProperty("SecurityId") ?? position.GetType().GetProperty("ContractId");
                if (prop != null)
                {
                    positionKey = prop.GetValue(position)?.ToString() ?? positionKey;
                }
                
                var volumeProp = position.GetType().GetProperty("Volume") ?? position.GetType().GetProperty("Size");
                if (volumeProp != null && volumeProp.PropertyType == typeof(decimal))
                {
                    positionVolume = (decimal)(volumeProp.GetValue(position) ?? 0);
                }
            }
            catch
            {
                // Fallback to using object reference
                positionKey = position.GetHashCode().ToString();
            }
            
            if (!_lastReportedPosition.ContainsKey(positionKey))
            {
                _lastReportedPosition.Add(positionKey, positionVolume);
                report = true;
            } else if (_lastReportedPosition[positionKey] != positionVolume)
            {
                _lastReportedPosition[positionKey] = positionVolume;
                report = true;
            }

            if (report)
            {
                var mappedMessage = ServiceLocator.PositionToPositionChangedEventMapper.Map(position);
                _orderEventHubDispatchService.PositionChanged(mappedMessage);
            }
        }

        protected override void OnCalculate(int bar, decimal value)
        {
        }

        protected override void OnStarted()
        {
            // Register this strategy instance to prevent multiple instances
            _strategyKey = $"{this.GetHashCode()}:{DateTime.UtcNow.Ticks}";
            _isRegistered = ServiceLocator.TryRegisterStrategy(_strategyKey);
            
            if (!_isRegistered)
            {
                System.Diagnostics.Debug.WriteLine("Failed to register strategy - another instance may be running");
                return;
            }

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
            
            if (_isRegistered && !string.IsNullOrEmpty(_strategyKey))
            {
                ServiceLocator.UnregisterStrategy(_strategyKey);
                _isRegistered = false;
            }

            base.OnStopping();
        }

        private bool ShouldProcessEvents()
        {
            return _isStarted;
        }

        private void ReconfigureConnection()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(_serverIpAddress), _serverPort);
            _orderEventHubDispatchService = ServiceLocator.GetDispatchService(endpoint);
        }
    }
}