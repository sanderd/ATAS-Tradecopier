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
        
        // Track active strategies by account:instrument pair -> strategy instance
        private static readonly ConcurrentDictionary<string, WeakReference<BroadcastOrderEventsStrategy>> _activeStrategies = new();
        
        // Track waiting strategies for each account:instrument pair
        private static readonly ConcurrentDictionary<string, Queue<WeakReference<BroadcastOrderEventsStrategy>>> _waitingStrategies = new();

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

        public static bool TryRegisterStrategy(string strategyKey, BroadcastOrderEventsStrategy strategy)
        {
            lock (_lock)
            {
                // Clean up any dead references first
                CleanupDeadReferences(strategyKey);
                
                // Check if there's already an active strategy for this account:instrument
                if (_activeStrategies.TryGetValue(strategyKey, out var existingRef) && 
                    existingRef.TryGetTarget(out var existingStrategy))
                {
                    // Add to waiting queue if different strategy instance
                    if (!ReferenceEquals(existingStrategy, strategy))
                    {
                        var waitingQueue = _waitingStrategies.GetOrAdd(strategyKey, _ => new Queue<WeakReference<BroadcastOrderEventsStrategy>>());
                        waitingQueue.Enqueue(new WeakReference<BroadcastOrderEventsStrategy>(strategy));
                        System.Diagnostics.Debug.WriteLine($"Strategy queued for {strategyKey} - another strategy is active");
                    }
                    return false;
                }
                
                // Register this strategy as active
                _activeStrategies[strategyKey] = new WeakReference<BroadcastOrderEventsStrategy>(strategy);
                System.Diagnostics.Debug.WriteLine($"Strategy registered as active for {strategyKey}");
                return true;
            }
        }

        public static void UnregisterStrategy(string strategyKey, BroadcastOrderEventsStrategy strategy)
        {
            lock (_lock)
            {
                // Only unregister if this is the active strategy
                if (_activeStrategies.TryGetValue(strategyKey, out var activeRef) && 
                    activeRef.TryGetTarget(out var activeStrategy) && 
                    ReferenceEquals(activeStrategy, strategy))
                {
                    _activeStrategies.TryRemove(strategyKey, out _);
                    System.Diagnostics.Debug.WriteLine($"Strategy unregistered for {strategyKey}");
                    
                    // Try to activate the next waiting strategy
                    ActivateNextWaitingStrategy(strategyKey);
                }
            }
        }

        public static bool IsActiveStrategy(string strategyKey, BroadcastOrderEventsStrategy strategy)
        {
            if (_activeStrategies.TryGetValue(strategyKey, out var activeRef) && 
                activeRef.TryGetTarget(out var activeStrategy))
            {
                return ReferenceEquals(activeStrategy, strategy);
            }
            return false;
        }

        private static void CleanupDeadReferences(string strategyKey)
        {
            // Clean up active strategy if dead
            if (_activeStrategies.TryGetValue(strategyKey, out var activeRef) && 
                !activeRef.TryGetTarget(out _))
            {
                _activeStrategies.TryRemove(strategyKey, out _);
                ActivateNextWaitingStrategy(strategyKey);
            }
            
            // Clean up waiting queue
            if (_waitingStrategies.TryGetValue(strategyKey, out var waitingQueue))
            {
                var tempQueue = new Queue<WeakReference<BroadcastOrderEventsStrategy>>();
                while (waitingQueue.Count > 0)
                {
                    var waitingRef = waitingQueue.Dequeue();
                    if (waitingRef.TryGetTarget(out _))
                    {
                        tempQueue.Enqueue(waitingRef);
                    }
                }
                
                if (tempQueue.Count > 0)
                {
                    _waitingStrategies[strategyKey] = tempQueue;
                }
                else
                {
                    _waitingStrategies.TryRemove(strategyKey, out _);
                }
            }
        }

        private static void ActivateNextWaitingStrategy(string strategyKey)
        {
            if (_waitingStrategies.TryGetValue(strategyKey, out var waitingQueue))
            {
                while (waitingQueue.Count > 0)
                {
                    var waitingRef = waitingQueue.Dequeue();
                    if (waitingRef.TryGetTarget(out var waitingStrategy))
                    {
                        // Activate this waiting strategy
                        _activeStrategies[strategyKey] = waitingRef;
                        System.Diagnostics.Debug.WriteLine($"Activated waiting strategy for {strategyKey}");
                        
                        // Notify the strategy that it's now active
                        waitingStrategy.OnActivated();
                        break;
                    }
                }
                
                // Clean up empty queue
                if (waitingQueue.Count == 0)
                {
                    _waitingStrategies.TryRemove(strategyKey, out _);
                }
            }
        }

        // Legacy method for backward compatibility
        public static bool TryRegisterStrategy(string strategyKey)
        {
            // This method is no longer used but kept for compatibility
            return false;
        }

        // Legacy method for backward compatibility  
        public static void UnregisterStrategy(string strategyKey)
        {
            // This method is no longer used but kept for compatibility
        }
    }
}