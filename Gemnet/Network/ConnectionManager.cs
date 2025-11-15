using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Gemnet.Security;
using System.Collections.Generic;
using System.Linq;

namespace Gemnet.Network
{
    public class ConnectionManager : IDisposable
    {
        private readonly ConcurrentDictionary<NetworkStream, ClientConnection> _connections = new();
        private readonly SemaphoreSlim _connectionSemaphore;
        private readonly int _maxConnections;
        private bool _disposed = false;

        public ConnectionManager(int maxConnections = 1000)
        {
            _maxConnections = maxConnections;
            _connectionSemaphore = new SemaphoreSlim(maxConnections, maxConnections);
        }

        public async Task<bool> TryAddConnectionAsync(TcpClient client, string? rc4Key = null)
        {
            if (_disposed) return false;

            // Wait for available connection slot
            if (!await _connectionSemaphore.WaitAsync(TimeSpan.FromSeconds(5)))
            {
                return false; // Timeout waiting for connection slot
            }

            try
            {
                var stream = client.GetStream();
                var connection = new ClientConnection(client, stream, rc4Key);
                
                if (_connections.TryAdd(stream, connection))
                {
                    connection.OnDisconnected += (s, e) => RemoveConnection(stream);
                    return true;
                }
                
                return false;
            }
            catch
            {
                _connectionSemaphore.Release();
                return false;
            }
        }

        public bool RemoveConnection(NetworkStream stream)
        {
            if (_connections.TryRemove(stream, out var connection))
            {

                connection?.Dispose();
                _connectionSemaphore.Release();
                return true;
            }
            return false;
        }

        public ClientConnection GetConnection(NetworkStream stream)
        {
            _connections.TryGetValue(stream, out var connection);
            return connection;
        }

        public IEnumerable<ClientConnection> GetAllConnections()
        {
            return _connections.Values.ToList();
        }

        public int ActiveConnectionCount => _connections.Count;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var connection in _connections.Values)
            {
                connection?.Dispose();
            }
            _connections.Clear();
            _connectionSemaphore?.Dispose();
        }
    }

    public class ClientConnection : IDisposable
    {
        public TcpClient Client { get; }
        public NetworkStream Stream { get; }
        public ClientCipherState CipherState { get; }
        public DateTime ConnectedAt { get; }
        public DateTime LastActivity { get; private set; }
        public bool IsAuthenticated { get; set; } = false;
        public string RemoteEndPoint { get; }

        public event EventHandler<ClientConnection> OnDisconnected;

        private bool _disposed = false;

        public ClientConnection(TcpClient client, NetworkStream stream, string? rc4Key = null)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            ConnectedAt = DateTime.UtcNow;
            LastActivity = DateTime.UtcNow;
            RemoteEndPoint = ((IPEndPoint)client.Client.RemoteEndPoint).ToString();

            if (!string.IsNullOrEmpty(rc4Key))
            {
                CipherState = new ClientCipherState(rc4Key);
            }
        }

        public void UpdateActivity()
        {
            LastActivity = DateTime.UtcNow;
        }

        public void Disconnect()
        {
            if (!_disposed)
            {
                OnDisconnected?.Invoke(this, this);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                Stream?.Close();
                Client?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing connection {RemoteEndPoint}: {ex.Message}");
            }
        }
    }
} 