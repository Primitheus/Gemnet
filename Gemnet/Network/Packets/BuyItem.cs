using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class BuyItemReq : HeaderPacket
    {
        public int ItemID { get; set; }

        public new static BuyItemReq Deserialize(byte[] data)
        {
            BuyItemReq packet = new BuyItemReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.ItemID = BitConverter.ToInt32(data, offset+1);


            return packet;
        }
    }
    public class BuyItemRes : HeaderPacket
    {
        public int ServerID { get; set; }
        public int Carats { get; set; }

        public override byte[] Serialize()
        {

            byte[] serverid = BitConverter.GetBytes(ServerID);
            byte[] carats = BitConverter.GetBytes(Carats);

            //Size = (ushort)(Time.Length + 6);
            Size = (ushort)(serverid.Length + carats.Length + 6);
            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            serverid.CopyTo(buffer, offset);
            offset += 4;
            carats.CopyTo(buffer, offset);

            return buffer;
        }
    }
}
