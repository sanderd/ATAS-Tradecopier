using System.Collections.Concurrent;
using System.Net;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using sadnerd.io.ATAS.BroadcastOrderEvents.Mappers;

namespace sadnerd.io.ATAS.BroadcastOrderEvents
{
    public static class ServiceLocator
    {
        private static readonly object _lock = new object();
        private static IOrderToNewOrderEventV1MessageMapper? _orderToNewOrderEventMapper;
        private static IOrderToOrderChangedV1MessageMapper? _orderToOrderChangedEventMapper;
        private static IPositionToPositionChangedV1MessageMapper? _positionToPositionChangedEventMapper;
        private static readonly ConcurrentDictionary<string, ResilientOrderEventHubDispatchService> _dispatchServices = new();
        private static readonly ConcurrentDictionary<string, bool> _activeStrategies = new();

        public static IOrderToNewOrderEventV1MessageMapper OrderToNewOrderEventMapper
        {
            get
            {
                if (_orderToNewOrderEventMapper == null)
                {
                    lock (_lock)
                    {
                        _orderToNewOrderEventMapper ??= new OrderToNewOrderEventV1MessageMapper();
                    }
                }
                return _orderToNewOrderEventMapper;
            }
        }

        public static IOrderToOrderChangedV1MessageMapper OrderToOrderChangedEventMapper
        {
            get
            {
                if (_orderToOrderChangedEventMapper == null)
                {
                    lock (_lock)
                    {
                        _orderToOrderChangedEventMapper ??= new OrderToOrderChangedV1MessageMapper();
                    }
                }
                return _orderToOrderChangedEventMapper;
            }
        }

        public static IPositionToPositionChangedV1MessageMapper PositionToPositionChangedEventMapper
        {
            get
            {
                if (_positionToPositionChangedEventMapper == null)
                {
                    lock (_lock)
                    {
                        _positionToPositionChangedEventMapper ??= new PositionToPositionChangedV1MessageMapper();
                    }
                }
                return _positionToPositionChangedEventMapper;
            }
        }

        public static IOrderEventHubDispatchService GetDispatchService(IPEndPoint endpoint)
        {
            var key = $"{endpoint.Address}:{endpoint.Port}";
            return _dispatchServices.GetOrAdd(key, _ => new ResilientOrderEventHubDispatchService(endpoint));
        }

        public static bool TryRegisterStrategy(string strategyKey)
        {
            return _activeStrategies.TryAdd(strategyKey, true);
        }

        public static void UnregisterStrategy(string strategyKey)
        {
            _activeStrategies.TryRemove(strategyKey, out _);
        }
    }
}