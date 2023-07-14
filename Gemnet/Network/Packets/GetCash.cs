using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class GetCashReq : HeaderPacket
    {
       

        public new static GetCashReq Deserialize(byte[] data)
        {
            GetCashReq packet = new GetCashReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);


            return packet;
        }
    }
    public class GetCashRes : HeaderPacket
    {
        public int UserID { get; set; }
        public int Astros { get; set; }
        public int Medals { get; set; }

        public override byte[] Serialize()
        {

            byte[] uuid = BitConverter.GetBytes(UserID);
            byte[] astros = BitConverter.GetBytes(Astros);
            byte[] medals = BitConverter.GetBytes(Medals);

            Size = (ushort)(uuid.Length + astros.Length + medals.Length + 6);
            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            uuid.CopyTo(buffer, offset);
            offset += 4;
            astros.CopyTo(buffer, offset);
            offset += 4;
            medals.CopyTo(buffer, offset); 

            return buffer;
        }
    }
}
