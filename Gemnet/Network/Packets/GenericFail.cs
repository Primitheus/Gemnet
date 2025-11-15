using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    
    public class GenericFailRes : HeaderPacket
    {
        public string Message { get; set; }

        private struct PropertyOffsets {

            public static readonly int Message = 6;


        }


        public override byte[] Serialize()
        {

            Size = (ushort)(520);
            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            Encoding.ASCII.GetBytes(Message).CopyTo(buffer, PropertyOffsets.Message);


            return buffer;
        }
    }
}
