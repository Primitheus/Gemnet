using Gemnet.Network.Header;
using Gemnet.Packets.Login;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class GetPropertyReq : HeaderPacket
    {
        public int UserID { get; set; }

        public new static GetPropertyReq Deserialize(byte[] data)
        {
            GetPropertyReq packet = new GetPropertyReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.UserID = BitConverter.ToInt32(data, offset);

            return packet;
        }
    }

    public class GetPropertyRes : HeaderPacket
    {
        public int[] ServerID { get; set; }
        public int[] ItemID { get; set; }
        public int[] ItemEnd { get; set; }
        public int[] ItemType { get; set; }


        public override byte[] Serialize()
        {
            int pairSize = sizeof(int) * 2; // Size of a pair of ServerID and ItemID
            int paddingSize = 18; // Size of padding between pairs

            int totalPairs = ServerID.Length; // Assuming ServerID and ItemID arrays have the same length

            Size = (ushort)((totalPairs * 49) + 8);

            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            byte[] numberofitems = BitConverter.GetBytes(totalPairs);

            numberofitems.CopyTo(buffer, offset);

            offset += 2;

            for (int i = 0; i < totalPairs; i++)
            {
                byte[] serverID = BitConverter.GetBytes(ServerID[i]);
                byte[] itemID = BitConverter.GetBytes(ItemID[i]);
                byte[] itemend = BitConverter.GetBytes(ItemEnd[i]);


                serverID.CopyTo(buffer, offset);
                offset += 4;
                itemID.CopyTo(buffer, offset);
                offset += 25;
                itemend.CopyTo(buffer, offset);
                offset += 2;

                if (i < totalPairs)
                {
                    byte[] padding = new byte[paddingSize];
                    padding.CopyTo(buffer, offset);
                    offset += paddingSize;
                }
            }

            return buffer;
        }

    }
}
