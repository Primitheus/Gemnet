using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Gemnet.Network;
using Gemnet.Settings;
using Gemnet.Security;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Gemnet.Packets.Enums;
using Gemnet.Network.Packets;
using Gemnet.PacketProcessors;
using Gemnet.Shop.Boxes;

namespace Gemnet
{
    public class Server : IDisposable
    {
        private readonly ILogger<Server> _logger;
        private readonly Settings.Settings.SData _settings;
        private readonly ConnectionManager _connectionManager;
        private readonly PacketProcessor _packetProcessor;
        private readonly PlayerManager _playerManager;
        private readonly GameManager _gameManager;

        private TcpListener _tcpListener;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _disposed = false;

        public Server(
            ILogger<Server> logger,
            Settings.Settings.SData settings,
            PlayerManager playerManager,
            GameManager gameManager)
        {
            _logger = logger;
            _settings = settings;
            _playerManager = playerManager;
            _gameManager = gameManager;
            
            _connectionManager = new ConnectionManager(settings.MaxConnections ?? 1000);
            var packetProcessorLogger = logger;
            _packetProcessor = new PacketProcessor(packetProcessorLogger, _connectionManager, _playerManager, _gameManager);
            
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, _settings.Port);
                _tcpListener.Start();

                BoxLoader.LoadBoxes("Data/Boxes"); // Initialize Box data.

                _logger.LogInformation("Gemnet - Rumble Fighter Server Emulator started on port {Port}", _settings.Port);
                _logger.LogInformation("Settings: Port={Port}, Encryption={UseEncryption}, RC4Key={RC4Key}", 
                    _settings.Port, _settings.UseEncryption, _settings.RC4Key);

                // Start connection monitoring task
                _ = Task.Run(MonitorConnectionsAsync, _cancellationTokenSource.Token);

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var client = await _tcpListener.AcceptTcpClientAsync();
                        
                        // Handle client connection asynchronously
                        _ = Task.Run(() => HandleClientConnectionAsync(client), _cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error accepting client connection");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start server");
                throw;
            }
        }

        private async Task HandleClientConnectionAsync(TcpClient client)
        {
            var remoteEndPoint = ((IPEndPoint)client.Client.RemoteEndPoint).ToString();
            
            try
            {
                _logger.LogInformation("Client connecting: {RemoteEndPoint}", remoteEndPoint);

                // Try to add connection to connection manager
                if (!await _connectionManager.TryAddConnectionAsync(client, _settings.UseEncryption ? _settings.RC4Key : null))
                {
                    _logger.LogWarning("Failed to add connection for {RemoteEndPoint} - server may be at capacity", remoteEndPoint);
                    client.Close();
                    return;
                }

                var stream = client.GetStream();
                var buffer = new byte[1440]; // MaxBufferSize
                var connection = _connectionManager.GetConnection(stream);

                _logger.LogInformation("Client connected: {RemoteEndPoint}", remoteEndPoint);

                while (!_cancellationTokenSource.Token.IsCancellationRequested && client.Connected)
                {
                    try
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
                        
                        if (bytesRead == 0)
                        {
                            _logger.LogInformation("Client {RemoteEndPoint} disconnected (graceful)", remoteEndPoint);
                            break;
                        }

                        await _packetProcessor.ProcessPacketAsync(buffer, bytesRead, stream);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (IOException ex)
                    {
                        _logger.LogInformation("Client {RemoteEndPoint} disconnected: {Message}", remoteEndPoint, ex.Message);
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        _logger.LogInformation("Stream disposed for client {RemoteEndPoint}", remoteEndPoint);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error handling client {RemoteEndPoint}", remoteEndPoint);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in client connection handler for {RemoteEndPoint}", remoteEndPoint);
            }
            finally
            {
                try
                {
                    var stream = client.GetStream();
                    var player = _playerManager.GetPlayerByStream(stream);

                    Query.UserUpdateRoom(player, player?.CurrentRoom ?? 0);

                    _gameManager.LeaveRoom(stream, player?.CurrentRoom ?? 0);
                    _playerManager.TryRemovePlayer(stream);

                    _connectionManager.RemoveConnection(stream);
                    client.Close();
                    _logger.LogInformation("Client {RemoteEndPoint} cleanup completed", remoteEndPoint);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during cleanup for client {RemoteEndPoint}", remoteEndPoint);
                }
            }
        }

        private async Task MonitorConnectionsAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var activeConnections = _connectionManager.ActiveConnectionCount;
                    _logger.LogDebug("Active connections: {Count}", activeConnections);

                    // Check for idle connections and disconnect them
                    var connections = _connectionManager.GetAllConnections();
                    var now = DateTime.UtcNow;
                    
                    foreach (var connection in connections)
                    {
                        if (now - connection.LastActivity > TimeSpan.FromMinutes(30)) // 30 minute timeout
                        {
                            _logger.LogInformation("Disconnecting idle connection: {RemoteEndPoint}", connection.RemoteEndPoint);
                            connection.Disconnect();
                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1), _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in connection monitor");
                }
            }
        }

        public async Task StopAsync()
        {
            if (_disposed) return;

            _logger.LogInformation("Stopping server...");
            
            _cancellationTokenSource.Cancel();

            try
            {
                _tcpListener?.Stop();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping TCP listener");
            }

            _logger.LogInformation("Server stopped");
        }

        // Packet sending methods with improved error handling
        public async Task SendPacketAsync(ushort type, ushort length, ushort action, NetworkStream stream)
        {
            try
            {
                var connection = _connectionManager.GetConnection(stream);
                if (connection == null)
                {
                    _logger.LogWarning("Attempted to send packet to unknown connection");
                    return;
                }

                var typeName = $"0x{type:X4}";
                var packet = new byte[length];
                
                packet[0] = (byte)(type >> 8);
                packet[1] = (byte)type;
                packet[2] = (byte)(length >> 8);
                packet[3] = (byte)length;
                packet[4] = (byte)action;
                packet[5] = (byte)(action >> 8);

                if (connection.CipherState != null)
                {
                    packet = connection.CipherState.Encryptor.Encrypt(packet);
                }

                await stream.WriteAsync(packet, 0, packet.Length);
                _logger.LogDebug("Sent packet: Type={TypeName}, Action={Action:X2}, Length={Length}", typeName, action, length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending packet");
                var connection = _connectionManager.GetConnection(stream);
                connection?.Disconnect();
            }
        }

        public async Task SendPacketAsync(ushort type, ushort length, ushort action, byte[] data, NetworkStream stream)
        {
            try
            {
                var connection = _connectionManager.GetConnection(stream);
                if (connection == null)
                {
                    _logger.LogWarning("Attempted to send packet to unknown connection");
                    return;
                }

                var typeName = $"0x{type:X4}";
                var packet = new byte[length];
                
                packet[0] = (byte)(type >> 8);
                packet[1] = (byte)type;
                packet[2] = (byte)(length >> 8);
                packet[3] = (byte)length;
                packet[4] = (byte)action;
                packet[5] = (byte)(action >> 8);

                if (data != null)
                {
                    data.CopyTo(packet, 6);
                }

                if (connection.CipherState != null)
                {
                    packet = connection.CipherState.Encryptor.Encrypt(packet);
                }

                await stream.WriteAsync(packet, 0, packet.Length);
                _logger.LogDebug("Sent packet: Type={TypeName}, Action={Action:X2}, Length={Length}", typeName, action, length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending packet");
                var connection = _connectionManager.GetConnection(stream);
                connection?.Disconnect();
            }
        }

        public async Task SendPacketAsync(byte[] data, NetworkStream stream)
        {
            try
            {
                var connection = _connectionManager.GetConnection(stream);
                if (connection == null)
                {
                    _logger.LogWarning("Attempted to send packet to unknown connection");
                    return;
                }

                if (connection.CipherState != null)
                {
                    data = connection.CipherState.Encryptor.Encrypt(data, data.Length);
                }

                await stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending packet");
                var connection = _connectionManager.GetConnection(stream);
                connection?.Disconnect();
            }
        }

        public async Task SendToRoomAsync(byte[] data, ushort roomId)
        {
            try
            {
                var playersInRoom = _gameManager.GetPlayersInRoom(roomId);
                _logger.LogDebug("Sending to {PlayerCount} players in room {RoomId}", playersInRoom.Count, roomId);

                var tasks = playersInRoom.Select(player => SendPacketAsync(data, player.Stream));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending to room {RoomId}", roomId);
            }
        }

        public async Task SendToRoomExcludeSenderAsync(byte[] data, ushort roomId, NetworkStream senderStream)
        {
            try
            {
                var playersInRoom = _gameManager.GetPlayersInRoom(roomId);
                _logger.LogDebug("Sending to {PlayerCount} players in room {RoomId} (excluding sender)", playersInRoom.Count, roomId);

                var tasks = playersInRoom
                    .Where(player => player.Stream != senderStream)
                    .Select(player => SendPacketAsync(data, player.Stream));
                
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending to room {RoomId} (excluding sender)", roomId);
            }
        }

        public async Task SendToAllAsync(byte[] data, NetworkStream? senderStream = null, bool excludeSender = false)
        {
            try
            {
                var connections = _connectionManager.GetAllConnections();
                var tasks = connections
                    .Where(conn => !excludeSender || conn.Stream != senderStream)
                    .Select(conn => SendPacketAsync(data, conn.Stream));
                
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending to all clients");
            }
        }

        public PacketProcessor GetPacketProcessor()
        {
            return _packetProcessor;
        }

        public IEnumerable<ClientConnection> GetAllConnections()
        {
            return _connectionManager.GetAllConnections();
        }

        // Legacy methods for backward compatibility with existing packet processors
        public async Task SendPacket(byte[] data, NetworkStream stream)
        {
            await SendPacketAsync(data, stream);
        }

        public async Task SendPacket(ushort type, ushort length, ushort action, NetworkStream stream)
        {
            await SendPacketAsync(type, length, action, stream);
        }

        public async Task SendPacket(ushort type, ushort length, ushort action, byte[] data, NetworkStream stream)
        {
            await SendPacketAsync(type, length, action, data, stream);
        }

        public async Task SendToRoom(byte[] data, ushort roomId)
        {
            await SendToRoomAsync(data, roomId);
        }

        public async Task SendToRoomExcludeSender(byte[] data, ushort roomId, NetworkStream senderStream)
        {
            await SendToRoomExcludeSenderAsync(data, roomId, senderStream);
        }

        public async Task SendNotificationPacket(byte[] data, NetworkStream stream)
        {
            await SendPacketAsync(data, stream);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                StopAsync().Wait(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during server disposal");
            }

            _cancellationTokenSource?.Dispose();
            _connectionManager?.Dispose();
        }
    }
}