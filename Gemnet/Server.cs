using SendPacket;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using Gemnet.Packets;
using System.Collections.Concurrent;
using static Gemnet.Packets.Enums.Packets;
using Gemnet.Network.Packets;
using Gemnet.Settings;
using Gemnet.Security;
using static Program;


public class Server
{

    private static PlayerManager _playerManager = ServerHolder._playerManager;
    private static GameManager _gameManager = ServerHolder._gameManager;


    private const int MaxBufferSize = 1440;
    private const int PacketHeaderSize = 6; // 2 bytes for type + 2 bytes for length + 2 bytes for action
    private const int TypeOffset = 0;
    private const int LengthOffset = 2;
    private const int ActionOffset = 4;

    private Settings.SData _settings;

    private TcpListener tcpListener;
    private ConcurrentBag<TcpClient> clients = new ConcurrentBag<TcpClient>();
    private ConcurrentDictionary<NetworkStream, ClientCipherState> clientCiphers = new();

    Parser parser = new Parser();

    public Server(IPAddress ipAddress, int port)
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        _settings = Settings.ImportSettings("./settings.json");

    }

    public async Task Start()
    {
        tcpListener.Start();
        Console.WriteLine($"<Gemnet - Rumble Fighter Server Emulator> Port: {tcpListener.LocalEndpoint}");
        Console.WriteLine($"Settings: {_settings.Port}, {_settings.UseEncryption}, {_settings.RC4Key}");


        while (true)
        {
            TcpClient client = await tcpListener.AcceptTcpClientAsync();
            clients.Add(client);
            _ = ProcessClient(client);

        }
    }

    private async Task ProcessClient(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            Console.WriteLine($"Client connected: {((IPEndPoint)client.Client.RemoteEndPoint).ToString()}");

            if (_settings.UseEncryption)
            {
                var cipherState = new ClientCipherState(_settings.RC4Key);
                clientCiphers.TryAdd(stream, cipherState);

            }

            byte[] buffer = new byte[MaxBufferSize];
            int bytesRead = 0;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await ParsePackets(buffer, bytesRead, stream);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("Client connection lost.");
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("Stream already disposed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }

        finally
        {
            clients.TryTake(out client);
            client.Close();
            Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}");
        }
    }

    private async Task ParsePackets(byte[] buffer, int bytesRead, NetworkStream stream)
    {

        int offset = 0;
        byte[] packet = new byte[bytesRead];

        while (offset < bytesRead)
        {
            if (offset + PacketHeaderSize > bytesRead)
            {
                Console.WriteLine("Not Enough bytes for a complete packet header.");
                break;
            }

            // if encryption enabled then decrypt the packet.
            if (_settings.UseEncryption)
            {
                if (clientCiphers.TryGetValue(stream, out var cipherState))
                {
                    // To decrypt incoming data:
                    packet = cipherState.Decryptor.Decrypt(buffer, bytesRead);

                }
            }
            else
            {
                packet = buffer;
            }


            ushort type = (ushort)((packet[offset + TypeOffset] << 8) | packet[offset + TypeOffset + 1]);
            ushort length = (ushort)((packet[offset + LengthOffset] << 8) | packet[offset + LengthOffset + 1]);
            ushort action = BitConverter.ToUInt16(packet, offset + ActionOffset);

            if (length > bytesRead)
            {
                Console.WriteLine($"Length: {length}, Bytes Read: {bytesRead}.");
                break;
            }

            byte[] packetBody = new byte[length];
            Buffer.BlockCopy(buffer, offset + PacketHeaderSize, packetBody, 0, length);

            // Process the packet (type, action, packetBody)

            parser.ProcessPacketAsync(type, action, packet, stream);

            offset += PacketHeaderSize + length;
        }

        if (offset < bytesRead)
        {
            // Move the remaining bytes to the start of the buffer
            Buffer.BlockCopy(buffer, offset, buffer, 0, bytesRead - offset);
        }

    }

    public async Task SendPacket(ushort type, ushort length, ushort action, NetworkStream stream)
    {
        String TypeName = Enum.GetName((HeaderType)type);

        byte[] packet = new byte[length];
        packet[0] = (byte)(type >> 8);
        packet[1] = (byte)type;
        packet[2] = (byte)(length >> 8);
        packet[3] = (byte)length;
        packet[4] = (byte)action;
        packet[5] = (byte)(action >> 8);


        if (_settings.UseEncryption)
        {
            if (clientCiphers.TryGetValue(stream, out var cipherState))
            {
                packet = cipherState.Encryptor.Encrypt(packet);
            }
        }

        await stream.WriteAsync(packet, 0, packet.Length);

        Console.WriteLine($"Sent Packet: Type={TypeName}, Action={action:X2}, Length={length}");
    }

    public async Task SendPacket(ushort type, ushort length, ushort action, byte[] data, NetworkStream stream)
    {
        String TypeName = Enum.GetName((HeaderType)type);

        byte[] packet = new byte[length];
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

        if (_settings.UseEncryption)
        {
            if (clientCiphers.TryGetValue(stream, out var cipherState))
            {
                packet = cipherState.Encryptor.Encrypt(packet);
            }
        }

        await stream.WriteAsync(packet, 0, packet.Length);

        Console.WriteLine("PACKET TEST!");
        Console.WriteLine($"Sent Packet: Type={TypeName}, Action={action:X2}, Length={length}");
    }

    public async Task SendPacket(byte[] data, NetworkStream stream)
    {

        if (_settings.UseEncryption)
        {
            if (clientCiphers.TryGetValue(stream, out var cipherState))
            {
                data = cipherState.Encryptor.Encrypt(data, data.Length);
            }
        }

        await stream.WriteAsync(data, 0, data.Length);

        //Console.WriteLine($"Sent Packet");
    }

    public async Task SendPacket(byte[] data, int maxBufferSize, NetworkStream stream)
    {
        int totalLength = data.Length;
        int bytesSent = 0;

        while (bytesSent < totalLength)
        {
            int remainingLength = totalLength - bytesSent;
            int bufferSize = Math.Min(maxBufferSize, remainingLength);

            if (_settings.UseEncryption)
            {
                if (clientCiphers.TryGetValue(stream, out var cipherState))
                {
                    data = cipherState.Encryptor.Encrypt(data);
                }
            }
                
            await stream.WriteAsync(data, bytesSent, bufferSize);

            bytesSent += bufferSize;
            //Console.WriteLine($"Sent Packet ({bytesSent}/{totalLength} bytes)");

        }
    }

    public async Task SendToRoom(byte[] data, ushort roomId)
    {
        //var room = _gameManager.GetRoom(roomId);
        var playersInRoom = _gameManager.GetPlayersInRoom(roomId);
        Console.WriteLine("Sending To All Players in Room: {roomId}");

            
        foreach (var player in playersInRoom)
        {
            Console.WriteLine($"Sending To Player: {player.UserIGN}");
            await SendPacket(data, player.Stream);

        }

    }


    public async Task SendToRoomExcludeSender(byte[] data, ushort roomId, NetworkStream sendingStream)
    {
        //var room = _gameManager.GetRoom(roomId);
        var playersInRoom = _gameManager.GetPlayersInRoom(roomId);
        Console.WriteLine("Sending To All Players in Room but Excluding Sender: {roomId}");

            
        foreach (var player in playersInRoom)
        {
            if (player.Stream == sendingStream)
            {
                Console.WriteLine("Skipping Sender");
            }
            else
            {
                await SendPacket(data, player.Stream);

            }

        }

    }

    public async Task SendPacket(byte[] data, NetworkStream senderStream, bool Exclude)
    {
        foreach (var client in clients)
        {
            // Skip the sender client if == true;

            var clientStream = client.GetStream();
            if (clientStream == senderStream && Exclude == true)
            {
                Console.WriteLine("Skipping Sender");
                
            }
            else
            {
                if (_settings.UseEncryption)
                {
                    if (clientCiphers.TryGetValue(clientStream, out var cipherState))
                    {
                        data = cipherState.Encryptor.Encrypt(data);
                    }
                }

                await clientStream.WriteAsync(data, 0, data.Length);

            }

            Console.WriteLine($"Sent Packet Whilst Excluding Player Who Sent Req");
        }
    }


    // Notify Packet To ALL Connected Clients.
    public async Task SendNotificationPacket(byte[] data)
    {
        foreach (var client in clients)
        {

            var clientStream = client.GetStream();

            if (_settings.UseEncryption)
            {
                if (clientCiphers.TryGetValue(clientStream, out var cipherState))
                {
                    data = cipherState.Encryptor.Encrypt(data);
                }
            }

            await clientStream.WriteAsync(data, 0, data.Length);

            Console.WriteLine($"Sending Notify Packet");
        }
    }


}