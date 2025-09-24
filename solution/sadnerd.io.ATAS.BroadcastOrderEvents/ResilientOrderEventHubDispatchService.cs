using System.Collections.Concurrent;
using System.Net;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using ServiceWire.TcpIp;

namespace sadnerd.io.ATAS.BroadcastOrderEvents
{
    // NOTE: An implementation with TPL ActionBlock would be cleaner, but we don't want to inject System.*.dlls into ATAS.
    public class ResilientOrderEventHubDispatchService : IOrderEventHubDispatchService, IDisposable
    {
        private readonly IPEndPoint _endpoint;
        private readonly object _connectionLock = new object();
        private readonly ConcurrentQueue<QueuedMessage> _messageQueue = new();
        private readonly Thread _processingThread;
        private readonly AutoResetEvent _messageAvailable = new(false);
        private TcpClient<IOrderEventHubDispatchService>? _client;
        private bool _isConnected;
        private bool _disposed;
        private DateTime _lastConnectionAttempt = DateTime.MinValue;
        private readonly TimeSpan _connectionRetryDelay = TimeSpan.FromSeconds(5);
        private const int MaxQueueSize = 1000;

        public ResilientOrderEventHubDispatchService(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
            
            _processingThread = new Thread(ProcessMessageQueue)
            {
                IsBackground = true,
                Name = $"OrderEventDispatcher-{_endpoint}"
            };
            _processingThread.Start();
        }

        // Fire-and-forget pattern - never blocks the caller
        public void NewOrder(NewOrderEventV1Message message)
        {
            EnqueueMessage(new QueuedMessage(MessageType.NewOrder, message));
        }

        public void OrderChanged(OrderChangedV1Message message)
        {
            EnqueueMessage(new QueuedMessage(MessageType.OrderChanged, message));
        }

        public void PositionChanged(PositionChangedV1Message message)
        {
            EnqueueMessage(new QueuedMessage(MessageType.PositionChanged, message));
        }

        private void EnqueueMessage(QueuedMessage message)
        {
            if (_disposed)
                return;

            // Check queue size for backpressure handling
            if (_messageQueue.Count >= MaxQueueSize)
            {
                return; // Drop message if queue is full
            }

            _messageQueue.Enqueue(message);
            _messageAvailable.Set(); // Wake up processing thread
        }

        private void ProcessMessageQueue()
        {
            while (!_disposed)
            {
                try
                {
                    // Wait for messages or timeout after 1 second
                    if (!_messageAvailable.WaitOne(1000))
                    {
                        continue; // Timeout, check if disposed and continue
                    }

                    // Process all available messages
                    while (_messageQueue.TryDequeue(out var message) && !_disposed)
                    {
                        ProcessSingleMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    // Wait a bit before continuing to avoid tight error loops
                    Thread.Sleep(1000);
                }
            }
        }

        private void ProcessSingleMessage(QueuedMessage message)
        {
            const int maxRetries = 3;
            int attemptCount = 0;

            while (attemptCount < maxRetries && !_disposed)
            {
                attemptCount++;
                
                try
                {
                    // Execute the message directly - let ServiceWire handle timeouts
                    ExecuteMessage(message);
                    return; // Success!
                }
                catch (Exception ex)
                {
                    // Mark as disconnected on any failure
                    MarkAsDisconnected();
                    
                    if (attemptCount < maxRetries)
                    {
                        // Exponential backoff with jitter
                        var delayMs = (int)(1000 * Math.Pow(2, attemptCount - 1) + Random.Shared.Next(0, 1000));
                        Thread.Sleep(delayMs);
                    }
                }
            }
        }

        private void ExecuteMessage(QueuedMessage message)
        {
            var client = GetConnectedClient();
            
            switch (message.Type)
            {
                case MessageType.NewOrder:
                    client.Proxy.NewOrder((NewOrderEventV1Message)message.Data);
                    break;
                case MessageType.OrderChanged:
                    client.Proxy.OrderChanged((OrderChangedV1Message)message.Data);
                    break;
                case MessageType.PositionChanged:
                    client.Proxy.PositionChanged((PositionChangedV1Message)message.Data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown message type: {message.Type}");
            }
        }

        private TcpClient<IOrderEventHubDispatchService> GetConnectedClient()
        {
            lock (_connectionLock)
            {
                if (_client == null || !_isConnected)
                {
                    // Implement connection retry delay to avoid rapid reconnection attempts
                    if (DateTime.UtcNow - _lastConnectionAttempt < _connectionRetryDelay)
                    {
                        throw new InvalidOperationException("Connection retry delay not elapsed");
                    }

                    _client?.Dispose();
                    _client = new TcpClient<IOrderEventHubDispatchService>(_endpoint);
                    _isConnected = true;
                    _lastConnectionAttempt = DateTime.UtcNow;
                }
                return _client;
            }
        }

        private void MarkAsDisconnected()
        {
            lock (_connectionLock)
            {
                if (_isConnected)
                {
                    _isConnected = false;
                }
            }
        }

        public void ClearQueue()
        {
            if (_disposed)
                return;

            // Clear all messages from the queue
            while (_messageQueue.TryDequeue(out _))
            {
                // Just drain the queue
            }
        }

        public void Dispose()
        {
            if (_disposed) 
                return;
            
            _disposed = true;
            
            try
            {
                // Wake up the processing thread so it can exit
                _messageAvailable.Set();
                
                // Wait for the processing thread to finish (with timeout)
                _processingThread.Join(5000);
                
                _messageAvailable?.Dispose();
            }
            catch (Exception ex)
            {
                // Ignore disposal errors
            }
            
            lock (_connectionLock)
            {
                _client?.Dispose();
                _client = null;
            }
        }

        private record QueuedMessage(MessageType Type, object Data);

        private enum MessageType
        {
            NewOrder,
            OrderChanged,
            PositionChanged
        }
    }
}