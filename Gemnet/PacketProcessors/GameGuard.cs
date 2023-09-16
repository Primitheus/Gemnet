﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Program;

namespace Gemnet.PacketProcessors
{
    internal class GameGuard
    {

        public static void SendGameGuard(ushort type, ushort action, NetworkStream stream)
        {
            action++;
            Console.WriteLine($"Sending GameGuard");


            byte[] data = { 0x00, 0x48, 0x00, 0x54, 0x81, 0x00, 0x4c, 0x00, 0x0c, 0x47, 0x29, 0x91, 0x27, 0x8e, 0x52, 0x22, 0x36, 0xd5, 0x78, 0xb3, 0x48, 0x1c, 0xa1, 0x44, 0x5c, 0x22, 0xeb, 0xe2, 0x90, 0x96, 0x3e, 0xf1, 0xfd, 0x8d, 0x95, 0x0d, 0x1b, 0xfd, 0x2a, 0xfa, 0x93, 0x97, 0x50, 0x02, 0xf3, 0xc7, 0xc3, 0x17, 0x23, 0x65, 0xbe, 0x4d, 0x28, 0xdd, 0xc1, 0x43, 0x5b, 0x6f, 0x3d, 0x31, 0x2a, 0xa6, 0x61, 0x31, 0x18, 0x32, 0x52, 0x71, 0x13, 0xc7, 0x70, 0xf6, 0x53, 0xee, 0x62, 0x37, 0x44, 0x5d, 0x6f, 0xe4, 0x87, 0x6e, 0x57, 0x37 };

            _ = ServerHolder.ServerInstance.SendPacket(data, stream);
        }

    }
}
