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
        public int MatchId {get; set;} // Guess.
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
            packet.MatchId = BitConverter.ToInt16(data, offset);
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

        private struct PropertyOffsets
        {
            public static readonly int NewCarats = 6; 
            public static readonly int NewExp = 14;
            public static readonly int unknownvalue1 = 30;
            public static readonly int unknownvalue2 = 54;
            public static readonly int UserIGN = 57;
            public static readonly int unknownvalue3 = 78;
            public static readonly int Kills = 81;
            public static readonly int EXP = 85;
            public static readonly int NNNNNNNNNN = 92;
            public static readonly int CARATS = 102;
            public static readonly int EXP2 = 110;

        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[1047];
            Size = (ushort)(buffer.Length);
            
            int offset = 0;
            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(NewCarats).CopyTo(buffer, PropertyOffsets.NewCarats);
            BitConverter.GetBytes(NewExp).CopyTo(buffer, PropertyOffsets.NewExp);
            BitConverter.GetBytes(unknownvalue1).CopyTo(buffer, PropertyOffsets.unknownvalue1);
            BitConverter.GetBytes(unknownvalue2).CopyTo(buffer, PropertyOffsets.unknownvalue2);
            Encoding.ASCII.GetBytes(UserIGN).CopyTo(buffer, PropertyOffsets.UserIGN);
            BitConverter.GetBytes(unknownvalue3).CopyTo(buffer, PropertyOffsets.unknownvalue3);
            BitConverter.GetBytes(Kills).CopyTo(buffer, PropertyOffsets.Kills);
            BitConverter.GetBytes(EXP).CopyTo(buffer, PropertyOffsets.EXP);
            Encoding.ASCII.GetBytes(NNNNNNNNNN).CopyTo(buffer, PropertyOffsets.NNNNNNNNNN);
            BitConverter.GetBytes(CARATS).CopyTo(buffer, PropertyOffsets.CARATS);
            BitConverter.GetBytes(EXP).CopyTo(buffer, PropertyOffsets.EXP2);


            return buffer;
        }
    }
    

}