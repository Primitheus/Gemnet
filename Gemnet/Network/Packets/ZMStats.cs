using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{

  public class GetZMStatsReq : HeaderPacket
    {
        public int ID {get; set;}


        public new static GetZMStatsReq Deserialize(byte[] data)
        {
            GetZMStatsReq packet = new GetZMStatsReq();

            int offset = 6;
            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.ID = BitConverter.ToInt32(data, offset);

            return packet;
        }
    }

    public class GetZMStatsRes : HeaderPacket
    {
        public string UserIGN {get; set;}
        public int Kills {get; set;}
        public int EXP {get; set;}

        //exp again


        public override byte[] Serialize()
        {
            byte[] ign = Encoding.ASCII.GetBytes(UserIGN);
            byte[] kills = BitConverter.GetBytes(Kills);
            byte[] exp = BitConverter.GetBytes(EXP);


            byte[] buffer = new byte[1047];
            Size = (ushort)buffer.Length;
            
            int offset = 0;
            base.Serialize().CopyTo(buffer, offset);

            offset += 6;

            return buffer;
        }
    }

    public class GetZMStats2Req : HeaderPacket
    {
        public int ID {get; set;}


        public new static GetZMStats2Req Deserialize(byte[] data)
        {
            GetZMStats2Req packet = new GetZMStats2Req();

            int offset = 6;
            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.ID = BitConverter.ToInt32(data, offset);
                
            return packet;
        }
    }
    

}