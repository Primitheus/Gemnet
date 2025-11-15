using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gemnet.Packets;
using Gemnet.Packets.Enums;
using static Gemnet.Packets.Enums.Packets;

namespace Gemnet.Network
{
    /// <summary>
    /// Compatibility layer to maintain backward compatibility with existing packet processors
    /// </summary>
    public static class ServerCompatibility
    {
        public static Server ServerInstance => Program.ServerHolder.ServerInstance;

        public static async Task SendPacket(ushort type, ushort length, ushort action, NetworkStream stream)
        {
            if (ServerInstance != null)
            {
                await ServerInstance.SendPacketAsync(type, length, action, stream);
            }
        }

        public static async Task SendPacket(ushort type, ushort length, ushort action, byte[] data, NetworkStream stream)
        {
            if (ServerInstance != null)
            {
                await ServerInstance.SendPacketAsync(type, length, action, data, stream);
            }
        }

        public static async Task SendPacket(byte[] data, NetworkStream stream)
        {
            if (ServerInstance != null)
            {
                await ServerInstance.SendPacketAsync(data, stream);
            }
        }

        public static async Task SendToRoom(byte[] data, ushort roomId)
        {
            if (ServerInstance != null)
            {
                await ServerInstance.SendToRoomAsync(data, roomId);
            }
        }

        public static async Task SendToRoomExcludeSender(byte[] data, ushort roomId, NetworkStream senderStream)
        {
            if (ServerInstance != null)
            {
                await ServerInstance.SendToRoomExcludeSenderAsync(data, roomId, senderStream);
            }
        }

        public static async Task SendToAll(byte[] data, NetworkStream? senderStream = null, bool excludeSender = false)
        {
            if (ServerInstance != null)
            {
                await ServerInstance.SendToAllAsync(data, senderStream, excludeSender);
            }
        }
    }
} 