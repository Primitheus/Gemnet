using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class ChatReq : HeaderPacket
    {
        public string Message { get; set; }

        public struct PropertyOffsets
        {
            public static readonly int Message = 6;

        }

        public new static ChatReq Deserialize(byte[] data)
        {
            ChatReq packet = new ChatReq();

            int offset = 6;
            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.Message = Encoding.ASCII.GetString(data, PropertyOffsets.Message, packet.Size - PropertyOffsets.Message);


            return packet;
        }
    }

    public class ChatRes : HeaderPacket
    {
        public string UserIGN { get; set; }
        public string Message { get; set; }

        public struct PropertyOffsets
        {
            public static readonly int UserIGN = 6;
            public static readonly int Message = 26;


        }

        public override byte[] Serialize()
        {

            Size = (ushort)(Message.Length + 1 + 26);
            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            Encoding.ASCII.GetBytes(UserIGN).CopyTo(buffer, PropertyOffsets.UserIGN);
            Encoding.ASCII.GetBytes(Message).CopyTo(buffer, PropertyOffsets.Message);


            return buffer;
        }
    }
}

