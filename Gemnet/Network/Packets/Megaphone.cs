using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class UseMegaphoneReq : HeaderPacket
    {
        public byte Unknown1 { get; set; }
        public int Unknown2 { get; set; } 
        public string Message { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int Unknown1 = 6;
            public static readonly int Unknown2 = 7;
            public static readonly int Message = 11;
        }
       
        public new static UseMegaphoneReq Deserialize(byte[] data)
        {
            UseMegaphoneReq packet = new UseMegaphoneReq();

            int offset = 6;
            int nullTerminator = 0;


            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.Unknown1 = data[PropertyOffsets.Unknown1];
            packet.Unknown2 = Convert.ToInt32(data[PropertyOffsets.Unknown2]);

            packet.Message = Encoding.ASCII.GetString(data, PropertyOffsets.Message, 44);
            nullTerminator = packet.Message.IndexOf('\x00');
            packet.Message = packet.Message.Remove(nullTerminator);
            

            return packet;
        }
    }
    

    public class UseMegaphoneRes : HeaderPacket
    {
        public byte Unknown1 { get; set; } = 10;
        public string UserIGN { get; set; }
        public string Message { get; set; }

        private struct PropertyOffsets
        {
            public readonly static int Unknown1 = 6;
            public readonly static int UserIGN = 7;
            public readonly static int Message = 27;

        }

        public override byte[] Serialize()
        {


            Size = (ushort)(283);
            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            buffer[PropertyOffsets.Unknown1] = Unknown1;
            Encoding.ASCII.GetBytes(UserIGN).CopyTo(buffer, PropertyOffsets.UserIGN);
            Encoding.ASCII.GetBytes(Message).CopyTo(buffer, PropertyOffsets.Message);
            


            return buffer;
        }
    }
}
