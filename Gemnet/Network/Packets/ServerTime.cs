using System.Text;
using System;
using Gemnet.Network.Header;
using static Gemnet.Packets.Enums.Packets;

namespace Gemnet.Packets.Login;

public class ServerTime : HeaderPacket
{
    public byte[] Time { get; set; }

    public override byte[] Serialize()
    {

        Size = (ushort)(Time.Length + 6);

        byte[] buffer = new byte[Size];
        int offset = 0;

        base.Serialize().CopyTo(buffer, offset);
        offset += 6;

        Time.CopyTo(buffer, offset);

        return buffer;
    }
}
