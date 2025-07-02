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
        public int unknown { get; set; } = 3925; // Placeholder for any additional fields

        private struct PropertyOffsets
        {
            public const int ServerID = 6;
            public const int ItemID = 10;
            public const int ItemEnd = 14;
            public const int Unknown = 18;
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
            BitConverter.GetBytes(unknown).CopyTo(buffer, PropertyOffsets.Unknown); // Assuming unknown is at offset 18

            return buffer;
        }
    }

}
