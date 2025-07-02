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

    }
}
