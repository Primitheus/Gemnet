using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{

  public class RewardsReq : HeaderPacket
    {
        public int unknown1 {get; set;}
        public int unknown2 {get; set;}
        public int unknown3 {get; set;}
        public int unknown4 {get; set;}
        public int unknown5 {get; set;}
        public int unknown6 {get; set;}
        public int unknown7 {get; set;}
        public int unknown8 {get; set;}
        public int unknown9 {get; set;}
        public int unknown10 {get; set;}
        public int EXP {get; set;}
        public int unknown11 {get; set;}
        public string UserIGN { get; set; }
        public int unknown12 {get; set;} // might be previous best?
        public int Kills {get; set;}
        public string NNNNNNNNNN {get; set;} // idk what to say it's just N's
        // exp again
        public int Carat {get; set;}

        //exp again

        public new static RewardsReq Deserialize(byte[] data)
        {
            RewardsReq packet = new RewardsReq();

            int offset = 6;
            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            offset += 4;
            packet.unknown1 = BitConverter.ToInt16(data, offset);
            offset += 1;
            packet.unknown2 = BitConverter.ToInt16(data, offset);
            offset += 1;
            packet.unknown3 = BitConverter.ToInt16(data, offset);
            offset += 1;
            packet.unknown4 = BitConverter.ToInt16(data, offset);
            offset += 1;
            packet.unknown5 = BitConverter.ToInt16(data, offset);
            offset += 1;
            packet.unknown6 = BitConverter.ToInt16(data, offset);
            offset += 2;
            packet.unknown7 = BitConverter.ToInt16(data, offset); // 0x4d
            offset += 2;
            packet.unknown8 = BitConverter.ToInt16(data, offset);
            offset += 3;
            packet.unknown9 = BitConverter.ToInt16(data, offset);
            offset += 2;
            packet.unknown10 = BitConverter.ToInt16(data, offset);
            offset += 2;
            packet.EXP = BitConverter.ToInt32(data, offset);
            offset += 24;
            packet.unknown11 = BitConverter.ToInt16(data, offset);
            offset += 3;
            packet.UserIGN = Encoding.ASCII.GetString(data, offset, 22);
            nullTerminator = packet.UserIGN.IndexOf('\x00');
            packet.UserIGN = packet.UserIGN.Remove(nullTerminator);
            offset += 23;
            packet.unknown12 = BitConverter.ToInt16(data, offset); // 0x0a
            offset += 3;
            packet.Kills = BitConverter.ToInt32(data, offset);
            offset += 4;
            packet.NNNNNNNNNN = Encoding.ASCII.GetString(data, offset, 10);
            offset += 17;
            packet.Carat = BitConverter.ToInt32(data, offset);

            return packet;
        }
    }

    public class RewardsRes : HeaderPacket
    {
        public int NewCarats {get; set;}
        public int NewExp {get; set;}
        public int unknownvalue1 {get; set;} // unknown7 from req.
        public int unknownvalue2 {get; set;} // 0x01
        public string UserIGN {get; set;}
        public int unknownvalue3 {get; set;} // unknown12 from req.
        public int Kills {get; set;}
        public int EXP {get; set;}
        public string NNNNNNNNNN {get; set;}
        public int CARATS {get; set;}
        //exp again


        public override byte[] Serialize()
        {
            byte[] newcarats = BitConverter.GetBytes(NewCarats);
            byte[] newexp = BitConverter.GetBytes(NewExp);
            byte[] unknown1 = BitConverter.GetBytes(unknownvalue1);
            byte[] unknown2 = BitConverter.GetBytes(unknownvalue2);
            byte[] ign = Encoding.ASCII.GetBytes(UserIGN);
            byte[] unknown3 = BitConverter.GetBytes(unknownvalue3);
            byte[] kills = BitConverter.GetBytes(Kills);
            byte[] exp = BitConverter.GetBytes(EXP);
            byte[] n = Encoding.ASCII.GetBytes(NNNNNNNNNN);
            byte[] carats = BitConverter.GetBytes(CARATS);

            byte[] buffer = new byte[1047];
            Size = (ushort)(buffer.Length);
            
            int offset = 0;
            base.Serialize().CopyTo(buffer, offset);

            offset += 6;

            newcarats.CopyTo(buffer, offset);
            offset += 8;
            newexp.CopyTo(buffer, offset);
            offset += 16;
            unknown1.CopyTo(buffer, offset);
            offset += 24;
            unknown2.CopyTo(buffer, offset);
            offset += 3;
            ign.CopyTo(buffer, offset);
            offset += 21;
            unknown3.CopyTo(buffer, offset);
            offset += 3;
            kills.CopyTo(buffer, offset);
            offset += 4;
            exp.CopyTo(buffer, offset);
            offset += 7;
            n.CopyTo(buffer, offset);
            offset += 10;
            carats.CopyTo(buffer, offset);
            offset += 8;
            exp.CopyTo(buffer, offset);


            return buffer;
        }
    }
    

}