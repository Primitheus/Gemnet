using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class EquipReq : HeaderPacket
    {
        // Additional fields specific to the LoginPacket type

        public int ItemID { get; set; }
        public int ItemEnd { get; set; }

        public new static EquipReq Deserialize(byte[] data)
        {
            EquipReq packet = new EquipReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.ItemID = BitConverter.ToInt32(data, offset);
            packet.ItemEnd = BitConverter.ToInt32(data, offset + 4);

            return packet;
        }
    }
    public class EquipRes : HeaderPacket
    {
        public int Result { get; set; }
        public override byte[] Serialize()
        {

            byte[] result = BitConverter.GetBytes(Result);

            Size = (ushort)(result.Length + 6);
            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            result.CopyTo(buffer, offset);
            
            return buffer;
        }
    }
}
