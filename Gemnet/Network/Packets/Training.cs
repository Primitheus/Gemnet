using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class TrainingReq : HeaderPacket
    {
        public string UserIGN { get; set; }

        public new static TrainingReq Deserialize(byte[] data)
        {
            TrainingReq packet = new TrainingReq();

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

    public class TrainingRes : HeaderPacket
    {
        public int UserID { get; set; }
        public string UserIGN { get; set; }

        public override byte[] Serialize()
        {
            byte[] userid = BitConverter.GetBytes(UserID);
            byte[] userign = Encoding.ASCII.GetBytes(UserIGN);

            Size = (ushort)(userid.Length + userign.Length + 6);

            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            userid.CopyTo(buffer, offset);
            offset += 4;
            userign.CopyTo(buffer, offset);


            return buffer;
        }
    }
}

