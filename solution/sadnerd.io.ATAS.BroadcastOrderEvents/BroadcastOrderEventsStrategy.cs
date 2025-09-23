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
        private bool _isActiveForCurrentPair = false;
        private IOrderEventHubDispatchService _orderEventHubDispatchService;
        private readonly IDictionary<string, decimal> _lastReportedPosition = new Dictionary<string, decimal>();
        private string _currentRegisteredPair = string.Empty;

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
            
            // Subscribe to TradingManager events for portfolio and security changes
            this.TradingManager.PortfolioSelected += OnPortfolioSelected;
            this.TradingManager.SecuritySelected += OnSecuritySelected;
            
            // Register based on current portfolio and security if available
            RegisterForCurrentAccountInstrument();
        }

        private void OnPortfolioSelected(Portfolio portfolio)
        {
            System.Diagnostics.Debug.WriteLine($"Portfolio selected: {portfolio.AccountID}");
            RegisterForCurrentAccountInstrument();
        }

        private void OnSecuritySelected(Security security)
        {
            System.Diagnostics.Debug.WriteLine($"Security selected: {security.SecurityId}");
            RegisterForCurrentAccountInstrument();
        }

        private void RegisterForCurrentAccountInstrument()
        {
            try
            {
                var accountId = this.TradingManager.Portfolio?.AccountID ?? "";
                var instrumentId = this.TradingManager.Security?.SecurityId ?? "";
                
                if (!string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(instrumentId))
                {
                    UpdateRegistration(accountId, instrumentId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering for current account/instrument: {ex.Message}");
            }
        }

        public void OnActivated()
        {
            // Called by ServiceLocator when this strategy becomes active for the current pair
            _isActiveForCurrentPair = true;
            System.Diagnostics.Debug.WriteLine($"Strategy activated for {_currentRegisteredPair}");
            
            // Clear position history when becoming active
            _lastReportedPosition.Clear();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            ReconfigureConnection();

            // Subscribe to TradingManager events for portfolio and security changes
            this.TradingManager.PortfolioSelected += OnPortfolioSelected;
            this.TradingManager.SecuritySelected += OnSecuritySelected;

            // Register based on current portfolio and security if available
            RegisterForCurrentAccountInstrument();
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

        // NOTE: Potential code smell! Please inspect why positionKey is relevant here.
        protected override void OnPositionChanged(Position position)
        {
            if (!ShouldProcessEvents()) return;
            
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
            RegisterForCurrentAccountInstrument();
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
            _isActiveForCurrentPair = false;
            _lastReportedPosition.Clear();
            
            // Unregister the current account/instrument pair
            if (!string.IsNullOrEmpty(_currentRegisteredPair))
            {
                ServiceLocator.UnregisterStrategy(_currentRegisteredPair, this);
                _currentRegisteredPair = string.Empty;
            }

            base.OnStopping();
        }

        private bool ShouldProcessEvents()
        {
            return _isStarted && _isActiveForCurrentPair;
        }

        private void UpdateRegistration(string accountId, string instrumentId)
        {
            var newPair = $"{accountId}:{instrumentId}";
            
            // Only update if the account/instrument combination has changed
            if (_currentRegisteredPair != newPair)
            {
                // Unregister the old pair if it exists
                if (!string.IsNullOrEmpty(_currentRegisteredPair))
                {
                    ServiceLocator.UnregisterStrategy(_currentRegisteredPair, this);
                    _isActiveForCurrentPair = false;
                }
                
                // Try to register the new pair
                if (ServiceLocator.TryRegisterStrategy(newPair, this))
                {
                    _currentRegisteredPair = newPair;
                    _isActiveForCurrentPair = true;
                    System.Diagnostics.Debug.WriteLine($"Strategy registered and activated for Account {accountId} and Instrument {instrumentId}");
                    
                    // Clear position history when switching account/instrument
                    _lastReportedPosition.Clear();
                }
                else
                {
                    _currentRegisteredPair = newPair; // Still track the pair even if not active
                    _isActiveForCurrentPair = false;
                    System.Diagnostics.Debug.WriteLine($"Strategy registered but not active for Account {accountId} and Instrument {instrumentId} - another strategy is already active");
                }
            }
        }

        private string GetPositionKey(Position position)
        {
            try
            {
                // Prefer security id; fall back to a hashcode
                if (!string.IsNullOrEmpty(position.SecurityId))
                    return position.SecurityId;

                return position.GetHashCode().ToString();
            }
            catch
            {
                return position.GetHashCode().ToString();
            }
        }

        private decimal GetPositionVolume(Position position)
        {
            return position.Volume;
        }

        private void ReconfigureConnection()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(_serverIpAddress), _serverPort);
            _orderEventHubDispatchService = ServiceLocator.GetDispatchService(endpoint);
        }
    }
}