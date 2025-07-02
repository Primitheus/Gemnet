using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class GetUserInfoRenewalReq : HeaderPacket
    {
        public string UserIGN { get; set; }
       
        public class PropertyOffsets
        {
            public const int UserIGN = 6; // Offset for the UserIGN property in the packet
        }


        public new static GetUserInfoRenewalReq Deserialize(byte[] data)
        {
            GetUserInfoRenewalReq packet = new GetUserInfoRenewalReq();

            int offset = 6;
            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.UserIGN = Encoding.ASCII.GetString(data, PropertyOffsets.UserIGN, 32);
            nullTerminator = packet.UserIGN.IndexOf('\x00');
            packet.UserIGN = packet.UserIGN.Remove(nullTerminator);

            return packet;
        }
    }
    
    public class GetUserInfoRenewalRes : HeaderPacket
    {
        public int UserID { get; set; }
        public string UserIGN { get; set; }
        public int EXP { get; set; }
        public List<int> Items { get; set; } = new List<int>();

        private struct PropertyOffsets
        {
            public static readonly int UserID = 6;
            public static readonly int UserIGN = 10;
            public static readonly int EXP = 34;
            public static readonly int Items = 38;
        }
        

        public override byte[] Serialize()
        {

           
            Size = (ushort)(1630);
            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(UserID).CopyTo(buffer, PropertyOffsets.UserID);
            Encoding.ASCII.GetBytes(UserIGN).CopyTo(buffer, PropertyOffsets.UserIGN);
            BitConverter.GetBytes(EXP).CopyTo(buffer, PropertyOffsets.EXP);

            int i = 0;
            foreach (var item in Items)
            {
                BitConverter.GetBytes(item).CopyTo(buffer, PropertyOffsets.Items + i);
                i += 4;
            }

            return buffer;
        }
    }
}
