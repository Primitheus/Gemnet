using SendPacket;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using Gemnet.Packets;
using System.Collections.Concurrent;
using static Gemnet.Packets.Enums.Packets;
public class Server
{
    public static Dictionary<NetworkStream, string> clientUsernames = new Dictionary<NetworkStream, string>();

    private const int MaxBufferSize = 1440;
    private const int PacketHeaderSize = 6; // 2 bytes for type + 2 bytes for length + 2 bytes for action
    private const int TypeOffset = 0;
    private const int LengthOffset = 2;
    private const int ActionOffset = 4;

    private TcpListener tcpListener;
    private ConcurrentBag<TcpClient> clients = new ConcurrentBag<TcpClient>();

    Parser parser = new Parser();

    public Server(IPAddress ipAddress, int port)
    {
        tcpListener = new TcpListener(ipAddress, port);

    }

    public async Task Start()
    {
        tcpListener.Start();
        Console.WriteLine($"<Gemnet - Rumble Fighter Server Emulator> Port: {tcpListener.LocalEndpoint}");


        while (true)
        {
            TcpClient client = await tcpListener.AcceptTcpClientAsync();
            clients.Add(client);
            _ = ProcessClient(client); // Start processing client asynchronously
        }
    }

    private async Task ProcessClient(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            Console.WriteLine($"Client connected: {((IPEndPoint)client.Client.RemoteEndPoint).ToString()}");


            byte[] buffer = new byte[MaxBufferSize];
            int bytesRead = 0;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await ParsePackets(buffer, bytesRead, stream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing client: {ex.Message}");
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

        while (offset < bytesRead)
        {
            if (offset + PacketHeaderSize > bytesRead)
            {
                // Not enough bytes for a complete packet header, wait for more data
                Console.WriteLine("Not Enough bytes for a complete packet header.");
                break;
            }

            ushort type = (ushort)((buffer[offset + TypeOffset] << 8) | buffer[offset + TypeOffset + 1]);
            ushort length = (ushort)((buffer[offset + LengthOffset] << 8) | buffer[offset + LengthOffset + 1]);
            ushort action = BitConverter.ToUInt16(buffer, offset + ActionOffset);

            if (length > bytesRead)
            {
                // Not enough bytes for the complete packet, wait for more data
                Console.WriteLine($"Length: {length}, Bytes Read: {bytesRead}.");
                break;
            }

            byte[] packetBody = new byte[length];
            Buffer.BlockCopy(buffer, offset + PacketHeaderSize, packetBody, 0, length);

            // Process the packet (type, action, packetBody)

            parser.ProcessPacketAsync(type, action, buffer, stream);

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

        await stream.WriteAsync(packet, 0, packet.Length);

        Console.WriteLine($"Sent Packet: Type={TypeName}, Action={action:X2}, Length={length}");
    }

    public async Task SendPacket(byte[] data, NetworkStream stream)
    {

        await stream.WriteAsync(data, 0, data.Length);

        Console.WriteLine($"Sent Packet");
    }

    public async Task SendPacket(byte[] data, int maxBufferSize, NetworkStream stream)
    {
        int totalLength = data.Length;
        int bytesSent = 0;

        while (bytesSent < totalLength)
        {
            int remainingLength = totalLength - bytesSent;
            int bufferSize = Math.Min(maxBufferSize, remainingLength);

            await stream.WriteAsync(data, bytesSent, bufferSize);

            bytesSent += bufferSize;
            Console.WriteLine($"Sent Packet ({bytesSent}/{totalLength} bytes)");

            // You can introduce a delay here if needed before sending the next chunk
            // await Task.Delay(delayInMilliseconds);
        }
    }

    public async Task SendPacket(byte[] data, NetworkStream senderStream, bool Exclude)
{
    foreach (var client in clients)
    {
        // Skip the sender client (excludeClientId)

        var clientStream = client.GetStream();
        if (clientStream == senderStream && Exclude == true) {
        } else {
            await clientStream.WriteAsync(data, 0, data.Length);

        }

        Console.WriteLine($"Sent Packet");
    }
}

}