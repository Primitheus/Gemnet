using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class Unknown2Req : HeaderPacket
    {
        public int Iteration { get; set; }

        public new static Unknown2Req Deserialize(byte[] data)
        {
            Unknown2Req packet = new Unknown2Req();
            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.Iteration = BitConverter.ToInt32(data, offset);


            return packet;
        }

    }

}
