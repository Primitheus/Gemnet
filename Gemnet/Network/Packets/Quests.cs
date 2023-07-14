using Gemnet.Network.Header;
using Gemnet.Packets.Login;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class GetQuestsReq : HeaderPacket
    {
        public int QuestType { get; set; }

        public new static GetQuestsReq Deserialize(byte[] data)
        {
            GetQuestsReq packet = new GetQuestsReq();
            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.QuestType = BitConverter.ToInt32(data, offset);


            return packet;
        }

    }
}
