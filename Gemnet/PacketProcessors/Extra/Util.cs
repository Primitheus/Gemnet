using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Gemnet.Program;
using static Gemnet.Server;
using Gemnet.Network.Packets;


namespace Gemnet.PacketProcessors.Extra
{
    public class Util
    {
        private static PlayerManager _playerManager = ServerHolder._playerManager;
        private static GameManager _gameManager = ServerHolder._gameManager;

        public static void Announce(string Message)
        {

            UseMegaphoneRes response = new UseMegaphoneRes();
            response.Type = 528;
            response.Action = 0x1A;

            response.UserIGN = "[Announcement]";
            response.Message = Message;

            Console.WriteLine($":Sending Announcement: {Message}");


            // Note: SendNotificationPacket requires a stream parameter, but this is a broadcast
            // We'll need to send to all connected clients
            var connections = ServerHolder.ServerInstance.GetAllConnections();
            foreach (var connection in connections)
            {
                _ = ServerHolder.ServerInstance.SendNotificationPacket(response.Serialize(), connection.Stream);
            }

        }

        public static void UserUpdateRoom(string UserIGN, NetworkStream stream)
        {

            var player = _playerManager.GetPlayerByIGN(UserIGN);


            UpdateRoomMasterRes response = new UpdateRoomMasterRes();
            response.Type = 576;
            response.Action = 0x17;
            response.NewRoomMaster = player.UserIGN;
            response.Unknown1 = 1;

            var player2 = _playerManager.GetPlayerByStream(stream);


            UpdateRoomMasterRes response2 = new UpdateRoomMasterRes();
            response2.Type = 576;
            response2.Action = 0x17;
            response2.NewRoomMaster = player2.UserIGN;
            response2.Unknown1 = 0;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
            _ = ServerHolder.ServerInstance.SendPacket(response2.Serialize(), stream);


        }

        public static ushort MarkAsFailed(ushort value)
        {
            // Preserve the low byte and set the high byte to 0x02
            return (ushort)((value & 0x00FF) | 0x0200);
        }

        public static ushort MarkActionAsFailed(ushort value)
        {
            return (ushort)((value & 0xFF00) | 0x0001);
        }


        public static void GenericFail(ushort type, ushort action, NetworkStream stream)
        {

            
            action++;

            Console.WriteLine("Generic Fail Sending");
            GenericFailRes response = new GenericFailRes();
            response.Type = MarkAsFailed(type);
            response.Action = action;
            
            response.Message = "FAIL(UNKNOWN)";

            byte[] data = response.Serialize();
            data[5] = 0x01;

            string hexOutput = string.Join(", ", data.Select(b => $"0x{b:X2}"));
            Console.WriteLine($"Generic Fail!: {hexOutput}");

            _ = ServerHolder.ServerInstance.SendPacketAsync(data, stream);
            Console.WriteLine("Generic Fail Sent");

        
        }


    }


}
