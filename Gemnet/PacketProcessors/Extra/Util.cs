using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Program;
using static Server;
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


            _ = ServerHolder.ServerInstance.SendNotificationPacket(response.Serialize());

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

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream, false);
            _ = ServerHolder.ServerInstance.SendPacket(response2.Serialize(), stream, false);


        }


    }


}
