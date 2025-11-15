using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GemnetCS.Network.Packets
{
    public class OpenBoxReq : HeaderPacket
    {
        // Additional fields specific to the LoginPacket type

        public int ServerID { get; set; }

        public new static OpenBoxReq Deserialize(byte[] data)
        {
            OpenBoxReq packet = new OpenBoxReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.ServerID = BitConverter.ToInt32(data, offset);

            return packet;
        }
    }
    public class OpenBoxRes : HeaderPacket
    {
        public int ServerID { get; set; }
        public int ItemID { get; set; }
        public int ItemEnd { get; set; }
        public byte Unknown1 { get; set; } // Placeholder for any additional fields
        public byte Quantity { get; set; }
        private struct PropertyOffsets
        {
            public const int ServerID = 6;
            public const int ItemID = 10;
            public const int ItemEnd = 14;
            public const int Unknown1 = 18;
            public const int Quantity = 19;

        }

        public override byte[] Serialize()
        {

            Size = (ushort)(43);

            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(ServerID).CopyTo(buffer, PropertyOffsets.ServerID);
            BitConverter.GetBytes(ItemID).CopyTo(buffer, PropertyOffsets.ItemID);
            BitConverter.GetBytes(ItemEnd).CopyTo(buffer, PropertyOffsets.ItemEnd);
            buffer[PropertyOffsets.Unknown1] = Unknown1;
            buffer[PropertyOffsets.Quantity] = Quantity;

            return buffer;
        }
    }

}
