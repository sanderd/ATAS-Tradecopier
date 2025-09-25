using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using OFT.Attributes;
using System.Net;
using ATAS.Strategies.Chart;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

// Import specific types needed from ATAS.DataFeedsCore
using Order = ATAS.DataFeedsCore.Order;
using Position = ATAS.DataFeedsCore.Position;
using Portfolio = ATAS.DataFeedsCore.Portfolio;
using Security = ATAS.DataFeedsCore.Security;

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
        private int _serverPort = 35144;

        [Parameter]
        [Display(Name = "Server IP Address", GroupName = "Connection Settings", Description = "IP address of the ServiceWire backend server")]
        public string ServerIpAddress
        {
            get => _serverIpAddress;
            set
            {
                if (_serverIpAddress != value)
                {
                    _serverIpAddress = value;
                    // Only reconfigure if the value is valid and not empty
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        try
                        {
                            ReconfigureConnection();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error reconfiguring connection with IP {value}: {ex.Message}");
                            // Don't throw - allow the property to be set even if connection fails
                        }
                    }
                }
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
                if (_serverPort != value)
                {
                    _serverPort = value;
                    // Only reconfigure if we have a valid IP address
                    if (!string.IsNullOrWhiteSpace(_serverIpAddress))
                    {
                        try
                        {
                            ReconfigureConnection();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error reconfiguring connection with port {value}: {ex.Message}");
                            // Don't throw - allow the property to be set even if connection fails
                        }
                    }
                }
            }
        }

        public BroadcastOrderEventsStrategy()
        {
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
            if (!ShouldProcessEvents() || _orderEventHubDispatchService == null) return;
            
            try
            {
                var mappedMessage = ServiceLocator.OrderToNewOrderEventMapper.Map(order);
                _orderEventHubDispatchService.NewOrder(mappedMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing NewOrder event: {ex.Message}");
            }
        }

        protected override void OnOrderChanged(Order order)
        {
            if (!ShouldProcessEvents() || _orderEventHubDispatchService == null) return;
            
            try
            {
                var mappedMessage = ServiceLocator.OrderToOrderChangedEventMapper.Map(order);
                _orderEventHubDispatchService.OrderChanged(mappedMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing OrderChanged event: {ex.Message}");
            }
        }

        // NOTE: Potential code smell! Please inspect why positionKey is relevant here.
        protected override void OnPositionChanged(Position position)
        {
            if (!ShouldProcessEvents() || _orderEventHubDispatchService == null) return;
            
            try
            {
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing PositionChanged event: {ex.Message}");
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
            
            // Clear the dispatch service queue to prevent messages from being processed after stopping
            try
            {
                if (_orderEventHubDispatchService != null && 
                    !string.IsNullOrWhiteSpace(_serverIpAddress) && 
                    IPAddress.TryParse(_serverIpAddress, out var ipAddress))
                {
                    var endpoint = new IPEndPoint(ipAddress, _serverPort);
                    ServiceLocator.ClearDispatchServiceQueue(endpoint);
                    System.Diagnostics.Debug.WriteLine($"Cleared dispatch service queue for {endpoint}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing dispatch service queue: {ex.Message}");
            }
            
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
            try
            {
                // Validate IP address before trying to parse it
                if (string.IsNullOrWhiteSpace(_serverIpAddress))
                {
                    System.Diagnostics.Debug.WriteLine("Cannot configure connection: IP address is empty");
                    return;
                }

                if (_serverPort <= 0 || _serverPort > 65535)
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot configure connection: Invalid port {_serverPort}");
                    return;
                }

                // Test IP address parsing before creating endpoint
                if (!IPAddress.TryParse(_serverIpAddress, out var ipAddress))
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot configure connection: Invalid IP address '{_serverIpAddress}'");
                    return;
                }

                var endpoint = new IPEndPoint(ipAddress, _serverPort);
                _orderEventHubDispatchService = ServiceLocator.GetDispatchService(endpoint);
                System.Diagnostics.Debug.WriteLine($"Successfully configured connection to {endpoint}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ReconfigureConnection: {ex.Message}");
                // Don't rethrow - allow strategy to continue working
            }
        }
    }
}