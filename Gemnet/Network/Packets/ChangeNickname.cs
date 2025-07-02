using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class ChangeNicknameReq : HeaderPacket
    {

        public string NewIGN { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int NewIGN = 6;
        }

        public new static ChangeNicknameReq Deserialize(byte[] data)
        {
            ChangeNicknameReq packet = new ChangeNicknameReq();

            int offset = 6;
            int nullTerminator = 0;


            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.NewIGN = Encoding.ASCII.GetString(data, PropertyOffsets.NewIGN, 20);
            nullTerminator = packet.NewIGN.IndexOf('\x00');
            packet.NewIGN = packet.NewIGN.Remove(nullTerminator);


            return packet;
        }
    }


    public class ChangeNicknameRes : HeaderPacket
    {
        public byte Unknown1 { get; set; }


        private struct PropertyOffsets
        {
            public readonly static int Unknown1 = 6;


        }

        public override byte[] Serialize()
        {


            Size = (ushort)(283);
            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            buffer[PropertyOffsets.Unknown1] = Unknown1;



            return buffer;
        }
    }

    public class AddBuddyIDReq : HeaderPacket
    {

        public string NewIGN { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int NewIGN = 6;
        }

        public new static AddBuddyIDReq Deserialize(byte[] data)
        {
            AddBuddyIDReq packet = new AddBuddyIDReq();

            int offset = 6;
            int nullTerminator = 0;


            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.NewIGN = Encoding.ASCII.GetString(data, PropertyOffsets.NewIGN, 20);
            nullTerminator = packet.NewIGN.IndexOf('\x00');
            packet.NewIGN = packet.NewIGN.Remove(nullTerminator);


            return packet;
        }
    }


    public class AddBuddyIDRes : HeaderPacket
    {
        public byte Unknown1 { get; set; }


        private struct PropertyOffsets
        {
            public readonly static int Unknown1 = 6;


        }

        public override byte[] Serialize()
        {


            Size = (ushort)(283);
            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            buffer[PropertyOffsets.Unknown1] = Unknown1;



            return buffer;
        }
    }
    
}
