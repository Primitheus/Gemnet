using System;
using System.Text;
using Gemnet.Network.Header;
using Newtonsoft.Json.Linq;
using static Gemnet.Packets.Enums.Packets;
using static Program;

namespace Gemnet.Packets.Login;

public class LoginReq : HeaderPacket
{
    public string Email { get; set; }
    public string Password { get; set; }


    public new static LoginReq Deserialize(byte[] data)
    {
        LoginReq packet = new LoginReq();
        int offset = 6;
        int maxEmailLength = 63;
        int maxPasswordLength = 21;

        int nullTerminator = 0;

        packet.Type = ToUInt16BigEndian(data, 0);
        packet.Size = ToUInt16BigEndian(data, 2);
        packet.Action = BitConverter.ToUInt16(data, 4);


        // Read Username
        packet.Email = Encoding.ASCII.GetString(data, offset, maxEmailLength);
        nullTerminator = packet.Email.IndexOf('\x00');
        packet.Email = packet.Email.Remove(nullTerminator);

        offset = 70;

        // Read Password
        packet.Password = Encoding.ASCII.GetString(data, offset, maxPasswordLength);
        nullTerminator = packet.Password.IndexOf('\x00');
        packet.Password = packet.Password.Remove(nullTerminator);

        return packet;
    }


}

public class LoginFailRes : HeaderPacket
{
    public string Error { get; set; }
    public int Code { get; set; }

    public override byte[] Serialize()
    {
        byte[] buffer = new byte[520];

        Size = (ushort)(buffer.Length);

        int offset = 0;

        base.Serialize().CopyTo(buffer, offset);
        offset += 6;

        byte[] err = Encoding.ASCII.GetBytes(Error);
        byte[] code = BitConverter.GetBytes(Code);

        code.CopyTo(buffer, offset);
        offset += 2;
        err.CopyTo(buffer, offset);

        return buffer;
    }

}

public class LoginRes : HeaderPacket
{
    public int UserID { get; set; }
    public string IGN { get; set; }
    public int Exp { get; set; }
    public int Carats { get; set; }
    public string GUID { get; set; }
    public string Token { get; set; }
    public string ForumName { get; set; }

    private struct PropertyOffsets
    {
        public static readonly int UserID = 6; 
        public static readonly int IGN = 10;
        public static readonly int Exp = 34;
        public static readonly int Carats = 38;
        public static readonly int GUID = 96;
        public static readonly int Token = 197;
        public static readonly int ForumName = 262;
    }

    public override byte[] Serialize()
    {
        byte[] buffer = new byte[326];

        Size = (ushort)buffer.Length;

        base.Serialize().CopyTo(buffer, 0);

        BitConverter.GetBytes(UserID).CopyTo(buffer, PropertyOffsets.UserID);
        Encoding.ASCII.GetBytes(IGN).CopyTo(buffer, PropertyOffsets.IGN);
        BitConverter.GetBytes(Exp).CopyTo(buffer, PropertyOffsets.Exp);
        BitConverter.GetBytes(Carats).CopyTo(buffer, PropertyOffsets.Carats);
        Encoding.ASCII.GetBytes(GUID).CopyTo(buffer, PropertyOffsets.GUID);
        Encoding.ASCII.GetBytes(Token).CopyTo(buffer, PropertyOffsets.Token);
        Encoding.ASCII.GetBytes(ForumName).CopyTo(buffer, PropertyOffsets.ForumName);

        return buffer;
    }
}


