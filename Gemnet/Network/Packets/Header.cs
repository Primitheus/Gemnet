
namespace Gemnet.Network.Header

{
    public class HeaderPacket
    {
        public ushort Type { get; set; }
        public ushort Size { get; set; }
        public ushort Action { get; set; }


        public virtual byte[] Serialize()
        {
            byte[] buffer = new byte[6];
            int offset = 0;
            BitConverter.GetBytes(ToUInt16BigEndian(BitConverter.GetBytes(Type), 0)).CopyTo(buffer, offset);
            offset += 2;
            BitConverter.GetBytes(ToUInt16BigEndian(BitConverter.GetBytes(Size), 0)).CopyTo(buffer, offset);
            offset += 2;

            BitConverter.GetBytes(Action).CopyTo(buffer, offset);

            return buffer;
        }

        public static HeaderPacket Deserialize(byte[] data)
        {
            HeaderPacket packet = new HeaderPacket();
            int offset = 0;

            packet.Type = ToUInt16BigEndian(data, offset);
            offset += sizeof(ushort);

            packet.Size = ToUInt16BigEndian(data, offset);
            offset += sizeof(ushort);

            packet.Action = BitConverter.ToUInt16(data, offset);

            return packet;
        }
        public static ushort ToUInt16BigEndian(byte[] bytes, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes, startIndex, 2);

            return BitConverter.ToUInt16(bytes, startIndex);
        }

    }
}