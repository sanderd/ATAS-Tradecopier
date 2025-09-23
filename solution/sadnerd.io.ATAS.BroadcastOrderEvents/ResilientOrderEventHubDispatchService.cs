using System.Collections.Concurrent;
using System.Net;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using ServiceWire.TcpIp;

namespace sadnerd.io.ATAS.BroadcastOrderEvents
{
    public class ResilientOrderEventHubDispatchService : IOrderEventHubDispatchService, IDisposable
    {
        private readonly IPEndPoint _endpoint;
        private readonly object _connectionLock = new object();
        private readonly ConcurrentQueue<QueuedMessage> _messageQueue = new();
        private readonly Timer _retryTimer;
        private TcpClient<IOrderEventHubDispatchService>? _client;
        private bool _isConnected;
        private bool _disposed;
        private DateTime _lastConnectionAttempt = DateTime.MinValue;
        private readonly TimeSpan _connectionRetryDelay = TimeSpan.FromSeconds(5);

        public ResilientOrderEventHubDispatchService(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
            _retryTimer = new Timer(ProcessMessageQueue, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public void NewOrder(NewOrderEventV1Message message)
        {
            ExecuteWithResilience(() => GetConnectedClient().Proxy.NewOrder(message), 
                new QueuedMessage(MessageType.NewOrder, message));
        }

        public void OrderChanged(OrderChangedV1Message message)
        {
            ExecuteWithResilience(() => GetConnectedClient().Proxy.OrderChanged(message),
                new QueuedMessage(MessageType.OrderChanged, message));
        }

        public void PositionChanged(PositionChangedV1Message message)
        {
            ExecuteWithResilience(() => GetConnectedClient().Proxy.PositionChanged(message),
                new QueuedMessage(MessageType.PositionChanged, message));
        }

        private void ExecuteWithResilience(Action action, QueuedMessage fallbackMessage)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(900));
                var task = Task.Run(action, cts.Token);
                task.Wait(cts.Token);
            }
            catch
            {
                _messageQueue.Enqueue(fallbackMessage);
                MarkAsDisconnected();
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
                _isConnected = false;
            }
        }

        private void ProcessMessageQueue(object? state)
        {
            if (_disposed) return;

            var processedCount = 0;
            const int maxProcessPerCycle = 10; // Limit processing to avoid blocking

            while (_messageQueue.TryDequeue(out var message) && processedCount < maxProcessPerCycle)
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(900));
                    var task = Task.Run(() => ExecuteQueuedMessage(message), cts.Token);
                    task.Wait(cts.Token);
                    processedCount++;
                }
                catch
                {
                    // Re-queue the message if it fails
                    _messageQueue.Enqueue(message);
                    MarkAsDisconnected();
                    break; // Stop processing to avoid rapid retry loops
                }
            }
        }

        private void ExecuteQueuedMessage(QueuedMessage message)
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
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _retryTimer?.Dispose();
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