using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class CreateAccountReq : HeaderPacket
    {
        public string UserIGN { get; set; }
        public int ClassId {get; set;}

        private struct PropertyOffsets
        {
            public static readonly int UserIGN = 39;
            public static readonly int ClassId = 59;

        }
       
        public new static CreateAccountReq Deserialize(byte[] data)
        {
            CreateAccountReq packet = new CreateAccountReq();

            int offset = 6;
            int nullTerminator = 0;


            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);


            packet.UserIGN = Encoding.ASCII.GetString(data, PropertyOffsets.UserIGN, 20);
            nullTerminator = packet.UserIGN.IndexOf('\x00');
            packet.UserIGN = packet.UserIGN.Remove(nullTerminator);

            packet.ClassId = BitConverter.ToInt32(data, PropertyOffsets.ClassId);


            return packet;
        }
    }
    

    public class CreateAccountRes : HeaderPacket
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
