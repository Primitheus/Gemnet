using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class AvatarListReq : HeaderPacket
    {
        public int UserID { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int UserID = 6;

        }
        public new static AvatarListReq Deserialize(byte[] data)
        {
            AvatarListReq packet = new AvatarListReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.UserID = Convert.ToInt32(data[PropertyOffsets.UserID]);

            return packet;
        }
    }

    public class AvatarListRes : HeaderPacket
    {   
        public int AvatarCount { get; set; }
        public int[] AvatarID { get; set; }

        private struct PropertyOffsets
        {   
            public static readonly int AvatarCount = 6;
            public static readonly int AvatarID = 7;

        }
        public override byte[] Serialize()
        {

            AvatarCount = AvatarID.Length;

            Size = (ushort)((AvatarCount * 4) + 8);
            byte[] buffer = new byte[Size];


            int offset = 0;
            var i = 0;

            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(AvatarCount).CopyTo(buffer, PropertyOffsets.AvatarCount);

            foreach (var avatar in AvatarID)
            {
                BitConverter.GetBytes(avatar).CopyTo(buffer, PropertyOffsets.AvatarID + i);
                i += 4;
            }


            return buffer;
        }
    }

    public class AvatarListItemsReq : HeaderPacket
    {
        public int AvatarID { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int AvatarID = 6;

        }
        public new static AvatarListItemsReq Deserialize(byte[] data)
        {
            AvatarListItemsReq packet = new AvatarListItemsReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.AvatarID = BitConverter.ToInt32(data, PropertyOffsets.AvatarID);

            return packet;
        }
    }

    // Needs to be fixed (Have idenfier for each item such as Class, Head, Top etc.)
    public class AvatarListItemsRes : HeaderPacket
    {   
        public int[] ServerID { get; set; }

        private struct PropertyOffsets
        {   

            public static readonly int ServerID = 6;

        }
        public override byte[] Serialize()
        {
            Size = (ushort)(( ServerID.Length * 4) + 178 + 4);

            byte[] buffer = new byte[Size];
            int offset = 0;
            var i = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            foreach (var serverid in ServerID)
            {
                BitConverter.GetBytes(serverid).CopyTo(buffer, PropertyOffsets.ServerID+i);
                i += 4;
            }

            return buffer;
        }
    }

    public class EquippedAvatarRes : HeaderPacket
    {   
        public int AvatarID { get; set; }

        private struct PropertyOffsets
        {   

            public static readonly int AvatarID = 6;

        }
        public override byte[] Serialize()
        {
            Size = 10;

            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            BitConverter.GetBytes(AvatarID).CopyTo(buffer, PropertyOffsets.AvatarID);
        

            return buffer;
        }
    }

}

