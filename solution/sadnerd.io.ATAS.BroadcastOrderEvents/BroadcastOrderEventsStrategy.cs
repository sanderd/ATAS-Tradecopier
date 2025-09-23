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
        private string _currentAccountId = string.Empty;
        private string _currentSecurityId = string.Empty;

        // Configuration properties
        private string _serverIpAddress = "127.0.0.1";
        private int _serverPort = 12345;

        [Parameter]
        [Display(Name = "Server IP Address", GroupName = "Connection Settings", Description = "IP address of the ServiceWire backend server")]
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
        [Display(Name = "Server Port", GroupName = "Connection Settings", Description = "Port of the ServiceWire backend server")]
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
            
            // Try to register strategy on first order if not already registered
            TryRegisterFromOrder(order);
            
            var mappedMessage = ServiceLocator.OrderToNewOrderEventMapper.Map(order);
            _orderEventHubDispatchService.NewOrder(mappedMessage);
        }

        protected override void OnOrderChanged(Order order)
        {
            if (!ShouldProcessEvents()) return;
            
            // Try to register strategy on first order if not already registered
            TryRegisterFromOrder(order);
            
            var mappedMessage = ServiceLocator.OrderToOrderChangedEventMapper.Map(order);
            _orderEventHubDispatchService.OrderChanged(mappedMessage);
        }

        protected override void OnPositionChanged(Position position)
        {
            if (!ShouldProcessEvents()) return;
            
            // Try to register strategy on first position if not already registered
            TryRegisterFromPosition(position);
            
            bool report = false;
            string positionKey = GetPositionKey(position);
            decimal positionVolume = GetPositionVolume(position);
            
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

        private void TryRegisterFromOrder(Order order)
        {
            if (_isRegistered) return;

            try
            {
                var accountId = GetOrderProperty(order, "AccountId", "Account")?.ToString() ?? "";
                var securityId = GetOrderProperty(order, "ContractId", "SecurityId", "Symbol")?.ToString() ?? "";
                
                if (!string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(securityId))
                {
                    TryRegisterStrategy(accountId, securityId);
                }
            }
            catch
            {
                // Fallback registration if we can't get proper IDs
                if (string.IsNullOrEmpty(_strategyKey))
                {
                    _strategyKey = $"strategy_{this.GetHashCode()}_{DateTime.UtcNow.Ticks}";
                    _isRegistered = ServiceLocator.TryRegisterStrategy(_strategyKey);
                }
            }
        }

        private void TryRegisterFromPosition(Position position)
        {
            if (_isRegistered) return;

            try
            {
                var accountId = GetPositionProperty(position, "AccountId", "Account")?.ToString() ?? "";
                var securityId = GetPositionProperty(position, "SecurityId", "ContractId", "Symbol")?.ToString() ?? "";
                
                if (!string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(securityId))
                {
                    TryRegisterStrategy(accountId, securityId);
                }
            }
            catch
            {
                // Fallback registration if we can't get proper IDs
                if (string.IsNullOrEmpty(_strategyKey))
                {
                    _strategyKey = $"strategy_{this.GetHashCode()}_{DateTime.UtcNow.Ticks}";
                    _isRegistered = ServiceLocator.TryRegisterStrategy(_strategyKey);
                }
            }
        }

        private void TryRegisterStrategy(string accountId, string securityId)
        {
            if (_isRegistered) return;

            _currentAccountId = accountId;
            _currentSecurityId = securityId;
            _strategyKey = $"{accountId}:{securityId}";
            
            _isRegistered = ServiceLocator.TryRegisterStrategy(_strategyKey);
            
            if (!_isRegistered)
            {
                System.Diagnostics.Debug.WriteLine($"Another strategy is already active for Account {accountId} and Security {securityId}. Only one strategy per account/security combination is allowed.");
            }
        }

        private string GetPositionKey(Position position)
        {
            try
            {
                return GetPositionProperty(position, "SecurityId", "ContractId", "Symbol")?.ToString() 
                       ?? position.GetHashCode().ToString();
            }
            catch
            {
                return position.GetHashCode().ToString();
            }
        }

        private decimal GetPositionVolume(Position position)
        {
            try
            {
                var volumeProp = GetPositionProperty(position, "Volume", "Size", "Quantity");
                if (volumeProp != null && (volumeProp is decimal || volumeProp is int || volumeProp is double || volumeProp is float))
                {
                    return Convert.ToDecimal(volumeProp);
                }
            }
            catch
            {
                // Ignore errors and return 0
            }
            return 0;
        }

        private object? GetOrderProperty(Order order, params string[] propertyNames)
        {
            var orderType = order.GetType();
            foreach (var propName in propertyNames)
            {
                var prop = orderType.GetProperty(propName);
                if (prop != null)
                {
                    return prop.GetValue(order);
                }
            }
            return null;
        }

        private object? GetPositionProperty(Position position, params string[] propertyNames)
        {
            var positionType = position.GetType();
            foreach (var propName in propertyNames)
            {
                var prop = positionType.GetProperty(propName);
                if (prop != null)
                {
                    return prop.GetValue(position);
                }
            }
            return null;
        }

        private void ReconfigureConnection()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(_serverIpAddress), _serverPort);
            _orderEventHubDispatchService = ServiceLocator.GetDispatchService(endpoint);
        }
    }
}