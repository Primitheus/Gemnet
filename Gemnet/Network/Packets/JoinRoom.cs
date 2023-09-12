using Gemnet.Network.Header;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{

    public class JoinRoomReq : HeaderPacket
    {
        public int UnknownValue1 {get; set;}
        public int UnknownValue2 {get; set;}
        public int UnknownValue3 {get; set;}
        public int UnknownValue4 {get; set;}
        public int UnknownValue5 {get; set;}
        public int UnknownValue6 {get; set;}
        public int UnknownValue7{get; set;}
        public string SomeID {get; set;}


        public new static JoinRoomReq Deserialize(byte[] data)
        {
            JoinRoomReq packet = new JoinRoomReq();

            int offset = 6;
            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);


            packet.UnknownValue1 = BitConverter.ToInt16(data, offset);
            offset += 4;
            packet.UnknownValue2 = BitConverter.ToInt16(data, offset);
            offset += 4;
            packet.UnknownValue3 = BitConverter.ToInt16(data, offset);
            offset += 2;
            packet.UnknownValue4 = BitConverter.ToInt16(data, offset);
            offset += 4;
            packet.UnknownValue5 = BitConverter.ToInt16(data, offset);
            offset += 2;
            packet.UnknownValue6 = BitConverter.ToInt16(data, offset); 
            offset += 6;
            packet.UnknownValue7 = BitConverter.ToInt16(data, offset); 
            offset += 4;
            packet.SomeID = Encoding.ASCII.GetString(data, offset, 4); // again, probably the p2p room id?
            nullTerminator = packet.SomeID.IndexOf('\x00');
            packet.SomeID = packet.SomeID.Remove(nullTerminator);


            return packet;
        }
    }

    public class JoinRoomRes : HeaderPacket
    {
        public int UnknownValue1 {get; set;}
        public int UnknownValue2 {get; set;}
        public int UnknownValue3 {get; set;}
        public int UnknownValue4 {get; set;}
        public int UnknownValue5 {get; set;}
        public int UnknownValue6 {get; set;}
        public string RoomMaster {get; set;}
        public int UnknownValue7 {get; set;}
        public string SomeID {get; set;}
        public string RoomName {get; set;}
        public int UnknownValue9 {get; set; }
        public int PlayerNumber {get; set; }
        public int MaxPlayers {get; set; }
        public int UnknownValue10 {get; set; }
        public int MatchType {get; set; } //Single or Team
        public int GameMode {get; set; }
        public int UnknownValue11 {get; set; }

        public int UnknownValue12 {get; set; }
        public int UnknownValue13 {get; set; }
        public byte[] Time { get; set; }
        public string Country {get; set;}
        public string Region {get; set;}


        public override byte[] Serialize()
        {
            byte[] unknownval1 = BitConverter.GetBytes(UnknownValue1);
            byte[] unknownval2 = BitConverter.GetBytes(UnknownValue2);
            byte[] unknownval3 = BitConverter.GetBytes(UnknownValue3);
            byte[] unknownval4 = BitConverter.GetBytes(UnknownValue4);
            byte[] unknownval5 = BitConverter.GetBytes(UnknownValue5);
            byte[] unknownval6 = BitConverter.GetBytes(UnknownValue6);
            byte[] unknownval7 = BitConverter.GetBytes(UnknownValue7);
            byte[] master = Encoding.ASCII.GetBytes(RoomMaster);
            byte[] unknownval9 = BitConverter.GetBytes(UnknownValue9);
            byte[] someid = Encoding.ASCII.GetBytes(SomeID);
            byte[] roomname = Encoding.ASCII.GetBytes(RoomName);
            byte[] unknownval10 = BitConverter.GetBytes(UnknownValue10);
            byte[] maxplayers = BitConverter.GetBytes(MaxPlayers);
            byte[] playernumber = BitConverter.GetBytes(PlayerNumber);
            byte[] matchtype = BitConverter.GetBytes(MatchType);

            byte[] gamemode = BitConverter.GetBytes(GameMode);
            byte[] unknownval12 = BitConverter.GetBytes(UnknownValue12);
            byte[] unknownval13 = BitConverter.GetBytes(UnknownValue13);

            byte[] country = Encoding.ASCII.GetBytes(Country);
            byte[] region = Encoding.ASCII.GetBytes(Region);

            byte[] buffer = new byte[238];
            Size = (ushort)buffer.Length;
            
            int offset = 0;
            base.Serialize().CopyTo(buffer, offset);

            offset += 6;
            unknownval1.CopyTo(buffer, offset);
            offset += 4;
            unknownval2.CopyTo(buffer, offset);
            offset += 2;
            unknownval3.CopyTo(buffer, offset);
            offset += 2;
            unknownval4.CopyTo(buffer, offset);
            offset += 4;
            unknownval5.CopyTo(buffer, offset);
            offset += 2;
            unknownval6.CopyTo(buffer, offset);
            offset += 8;
            master.CopyTo(buffer, offset);
            offset += 20;
            unknownval7.CopyTo(buffer, offset);
            offset += 4;
            someid.CopyTo(buffer, offset);
            offset += 32;
            roomname.CopyTo(buffer, offset);
            offset += 40;
            unknownval9.CopyTo(buffer, offset);
            offset += 1;
            maxplayers.CopyTo(buffer, offset);
            offset += 1;
            playernumber.CopyTo(buffer, offset);
            offset += 1;
            unknownval10.CopyTo(buffer, offset);
            offset += 3;
            matchtype.CopyTo(buffer, offset); // possible need to change offsets
            offset += 4;
            gamemode.CopyTo(buffer, offset);
            offset += 5;
            unknownval12.CopyTo(buffer, offset);
            offset += 4;
            unknownval13.CopyTo(buffer, offset);
            offset += 46;
            Time.CopyTo(buffer, offset);
            offset += 8;
            country.CopyTo(buffer, offset);
            offset += 8;
            region.CopyTo(buffer, offset);


            return buffer;
        }
    }
}

