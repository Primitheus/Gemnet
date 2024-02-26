using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class MatchReq : HeaderPacket
    {
        public string UserIGN { get; set; }

        public new static MatchReq Deserialize(byte[] data)
        {
            MatchReq packet = new MatchReq();

            int offset = 6;
            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.UserIGN = Encoding.ASCII.GetString(data, offset, 32);
            nullTerminator = packet.UserIGN.IndexOf('\x00');
            packet.UserIGN = packet.UserIGN.Remove(nullTerminator);


            return packet;
        }
    }

    public class MatchRes : HeaderPacket
    {
        public int UserID { get; set; }
        public string UserIGN { get; set; }
        public int[] ItemID { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }


        private struct PropertyOffsets
        {   

            public static readonly int UserID = 6;
            public static readonly int UserIGN = 10;
            public static readonly int ItemID = 38;
            public static readonly int Country = 1527;
            public static readonly int Region = 1535;

        }

        public override byte[] Serialize()
        {
            Size = (ushort)(1567);
            byte[] buffer = new byte[Size];
            int offset = 0;
            var i = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            BitConverter.GetBytes(UserID).CopyTo(buffer, PropertyOffsets.UserID);
            Encoding.ASCII.GetBytes(UserIGN).CopyTo(buffer, PropertyOffsets.UserIGN);

            foreach (var item in ItemID)
            {
                BitConverter.GetBytes(item).CopyTo(buffer, PropertyOffsets.ItemID+i);
                i += 4;
            }

            Encoding.ASCII.GetBytes(Country).CopyTo(buffer, PropertyOffsets.Country);
            Encoding.ASCII.GetBytes(Region).CopyTo(buffer, PropertyOffsets.Region);


            return buffer;
        }
    }



}

