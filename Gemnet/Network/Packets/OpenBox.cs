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

            packet.ServerID = BitConverter.ToInt16(data, offset);

            return packet;
        }
    }
    public class OpenBoxRes : HeaderPacket
    {
        public int ServerID { get; set; }
        public int ItemID { get; set; }
        public int ItemEnd { get; set; }

        public override byte[] Serialize()
        {
            byte[] serverid = BitConverter.GetBytes(ServerID);
            byte[] itemid = BitConverter.GetBytes(ItemID);
            byte[] itemend = BitConverter.GetBytes(ItemEnd);


            Size = (ushort)(serverid.Length + itemid.Length + itemend.Length + 6 + 2);

            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 8;

            serverid.CopyTo(buffer, offset);
            offset += 4;
            itemid.CopyTo(buffer, offset);
            offset += 4;
            itemend.CopyTo(buffer, offset);


            return buffer;
        }
    }

}
