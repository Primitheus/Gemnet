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
    //public int Astros { get; set; }
    public string GUID { get; set; }
    public string Token { get; set; }
    public string ForumName { get; set; }


    public override byte[] Serialize()
    {
        byte[] buffer = new byte[326];


        Size = (ushort)(buffer.Length);

        int offset = 0;

        base.Serialize().CopyTo(buffer, offset);
        offset += 6;

        byte[] uuid = BitConverter.GetBytes(UserID);
        byte[] ign = Encoding.ASCII.GetBytes(IGN);
        byte[] exp = BitConverter.GetBytes(Exp);
        byte[] carats = BitConverter.GetBytes(Carats);
        byte[] guid = Encoding.ASCII.GetBytes(GUID.ToString());
        byte[] token = Encoding.ASCII.GetBytes(Token.ToString());
        byte[] forum_name = Encoding.ASCII.GetBytes(ForumName);

        uuid.CopyTo(buffer, offset);
        offset += 4;
        ign.CopyTo(buffer, offset);
        offset += 24;
        exp.CopyTo(buffer, offset);
        offset += 4;
        carats.CopyTo(buffer, offset);
        offset += 58;
        guid.CopyTo(buffer, offset);
        offset += 101;
        token.CopyTo(buffer, offset);
        offset += 65;
        forum_name.CopyTo(buffer, offset);

        return buffer;
    }

  


}

