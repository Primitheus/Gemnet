using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class EnchantReq : HeaderPacket
    {

        //Both are Server IDs
        public int EnchantGemID { get; set; } 
        public int EnchantCardID { get; set; }

        public struct PropertyOffsets
        {
            public static readonly int EnchantGemID = 6;
            public static readonly int EnchantCardID = 10;

        }

        public new static EnchantReq Deserialize(byte[] data)
        {
            EnchantReq packet = new EnchantReq();


            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.EnchantGemID = BitConverter.ToInt32(data, PropertyOffsets.EnchantGemID);
            packet.EnchantCardID = BitConverter.ToInt32(data, PropertyOffsets.EnchantCardID);

            return packet;
        }
    }

    public class EnchantRes : HeaderPacket
    {

        public int EnchantGemID { get; set; }
        public int StatMod { get; set; }

        public struct PropertyOffsets
        {
            public static readonly int EnchantGemID = 6;
            public static readonly int StatMod = 10;
        }

        public override byte[] Serialize()
        {

            Size = (ushort)(12);
            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            BitConverter.GetBytes(EnchantGemID).CopyTo(buffer, PropertyOffsets.EnchantGemID);
            BitConverter.GetBytes(StatMod).CopyTo(buffer, PropertyOffsets.StatMod);

            return buffer;
        }
    }



}

