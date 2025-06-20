using Gemnet.Network.Header;
using Gemnet.Packets.Login;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class GetBuddyListReq : HeaderPacket
    {
        public int Unk1 { get; set; }

        public new static GetBuddyListReq Deserialize(byte[] data)
        {
            GetBuddyListReq packet = new GetBuddyListReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.Unk1 = BitConverter.ToInt32(data, offset);

            return packet;
        }

    }

    public class Buddy
    {
        public int UserID { get; set; }
        public string UserIGN { get; set; }

        // UNKNOWN FIELD(S) HERE 

    }


    public class GetBuddyListRes : HeaderPacket
    {
        public ushort BuddyCount { get; set; }
        public List<Buddy> Buddies { get; set; }

        private struct PropertyOffsets
        {

            public static readonly int NumberOfBuddies = 6;
            public static readonly int UserID = 8;
            public static readonly int UserIGN = 12;


        }


        public override byte[] Serialize()
        {
            BuddyCount = (ushort)(Buddies?.Count ?? 0);

            // Use a MemoryStream for dynamic sizing
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Reserve space for header (will patch Size later)
                byte[] header = base.Serialize();
                writer.Write(header);

                // Write BuddyCount (Big Endian)
                ushort buddyCountBE = BitConverter.IsLittleEndian ? (ushort)((BuddyCount >> 8) | (BuddyCount << 8)) : BuddyCount;
                writer.Write(BuddyCount);

                // Write each buddy
                var i = 0;
                foreach (var buddy in Buddies)
                {

                    // UserID
                    // Write UserID (Little Endian)
                    if (i == 0)
                    {
                        byte[] useridBytes = BitConverter.GetBytes(buddy.UserID);
                        writer.Write(useridBytes);
                    }
                    else
                    {
                        byte[] useridBytes = BitConverter.GetBytes(buddy.UserID);
                        writer.Write(useridBytes);
                        writer.Write((byte)0x00);
                    }




                    // UserIGN length (byte), then UserIGN bytes (ASCII), then null terminator
                    byte[] ignBytes = Encoding.ASCII.GetBytes(buddy.UserIGN);

                    // pad ignBytes to 24 bytes long.
                    if (ignBytes.Length < 24)
                    {
                        ignBytes = ignBytes.Concat(new byte[24 - ignBytes.Length]).ToArray();
                    }
                    else if (ignBytes.Length > 24)
                    {
                        ignBytes = ignBytes.Take(24).ToArray();
                    }

                    writer.Write(ignBytes);
                    writer.Write((byte)0x4E); // terminator ??

                    i++;

                }

                // Patch the Size field in the header if needed
                byte[] result = ms.ToArray();
                ushort size = (ushort)result.Length;
                // Patch Size at offset 2 (assuming header is [Type(2), Size(2), Action(2)])
                byte[] sizeBytes = BitConverter.IsLittleEndian ? new byte[] { (byte)(size >> 8), (byte)(size & 0xFF) } : BitConverter.GetBytes(size);
                result[2] = sizeBytes[0];
                result[3] = sizeBytes[1];

                return result;
            }
        }

    }

    public class AddBuddyReq : HeaderPacket
    {
        public string UserIGN { get; set; }

        public new static AddBuddyReq Deserialize(byte[] data)
        {
            AddBuddyReq packet = new AddBuddyReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);


            // Read UserIGN (ASCII)
            packet.UserIGN = Encoding.ASCII.GetString(data, offset, 24).TrimEnd('\0');


            return packet;
        }
    }

    public class AddBuddyRes : HeaderPacket
    {
        public int UserID { get; set; }
        public string UserIGN { get; set; }

        public override byte[] Serialize()
        {
            // Use a MemoryStream for dynamic sizing
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Reserve space for header (will patch Size later)
                byte[] header = base.Serialize();
                writer.Write(header);

                // Write UserID (Little Endian)
                byte[] useridBytes = BitConverter.GetBytes(UserID);
                writer.Write(useridBytes);

                // Write UserIGN (ASCII, padded to 24 bytes)
                byte[] ignBytes = Encoding.ASCII.GetBytes(UserIGN);
                if (ignBytes.Length < 24)
                {
                    ignBytes = ignBytes.Concat(new byte[24 - ignBytes.Length]).ToArray();
                }
                else if (ignBytes.Length > 24)
                {
                    ignBytes = ignBytes.Take(24).ToArray();
                }
                writer.Write(ignBytes);
                writer.Write((byte)0x4E); // terminator ??

                // Patch the Size field in the header if needed
                byte[] result = ms.ToArray();
                ushort size = (ushort)result.Length;
                byte[] sizeBytes = BitConverter.IsLittleEndian ? new byte[] { (byte)(size >> 8), (byte)(size & 0xFF) } : BitConverter.GetBytes(size);
                result[2] = sizeBytes[0];
                result[3] = sizeBytes[1];

                return result;
            }
        }
    }


    public class RemoveBuddyReq : HeaderPacket
    {
        public int UserID { get; set; }

        public new static RemoveBuddyReq Deserialize(byte[] data)
        {
            RemoveBuddyReq packet = new RemoveBuddyReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.UserID = BitConverter.ToInt32(data, offset);

            return packet;
        }

    }

    public class RemoveBuddyRes : HeaderPacket
    {
        public int UserID { get; set; }

        public override byte[] Serialize()
        {
            // Use a MemoryStream for dynamic sizing
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Reserve space for header (will patch Size later)
                byte[] header = base.Serialize();
                writer.Write(header);

                // Write UserID (Little Endian)
                byte[] useridBytes = BitConverter.GetBytes(UserID);
                writer.Write(useridBytes);

                // Patch the Size field in the header if needed
                byte[] result = ms.ToArray();
                ushort size = (ushort)result.Length;
                byte[] sizeBytes = BitConverter.IsLittleEndian ? new byte[] { (byte)(size >> 8), (byte)(size & 0xFF) } : BitConverter.GetBytes(size);
                result[2] = sizeBytes[0];
                result[3] = sizeBytes[1];

                return result;
            }
        }
    }

}
