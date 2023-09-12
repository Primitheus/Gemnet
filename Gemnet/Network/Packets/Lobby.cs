using Gemnet.Network.Header;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class ToLobbyReq : HeaderPacket
    {

        public new static ToLobbyReq Deserialize(byte[] data)
        {
            ToLobbyReq packet = new ToLobbyReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            return packet;
        }
    }

    public class ToLobbyRes : HeaderPacket
    {
        public int Result { get; set; }
        public override byte[] Serialize()
        {

            byte[] result = BitConverter.GetBytes(Result);

            Size = (ushort)(result.Length + 6);
            byte[] buffer = new byte[Size];
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            result.CopyTo(buffer, offset);
            
            return buffer;
        }
    }

    public class CreateRoomReq : HeaderPacket 
    {
        public int unkownvalue1 {get; set;}
        public int unkownvalue2 {get; set;}
        public string SomeID {get; set;}
        public string SomeID2 {get; set;}
        public string RoomName {get; set;}

        public new static CreateRoomReq Deserialize(byte[] data)
        {
            CreateRoomReq packet = new CreateRoomReq();

            int offset = 6;
            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            offset += 2;
            packet.unkownvalue1 = BitConverter.ToInt32(data, offset); // seems to always be 0x8c not sure.
            offset += 4;
            packet.SomeID = Encoding.ASCII.GetString(data, offset, 4); // again, probably the p2p room id?
            nullTerminator = packet.SomeID.IndexOf('\x00');
            packet.SomeID = packet.SomeID.Remove(nullTerminator);
            offset += 38;
            packet.unkownvalue2 = BitConverter.ToInt32(data, offset); // might actually be the password. starting at offset 32, if no password then offset 38 is set to 0x09.
            offset += 4;
            packet.SomeID2 = Encoding.ASCII.GetString(data, offset, 4);
            nullTerminator = packet.SomeID2.IndexOf('\x00');
            packet.SomeID2 = packet.SomeID2.Remove(nullTerminator);
            offset += 6;
            packet.RoomName = Encoding.ASCII.GetString(data, offset, 32);
            nullTerminator = packet.RoomName.IndexOf('\x00');
            packet.RoomName = packet.RoomName.Remove(nullTerminator);

            return packet;
        }


    }

    public class CreateRoomRes : HeaderPacket
    {

        public int Result {get; set;} //Currently guessing it's a result packet.

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[15];

            Size = (ushort)(buffer.Length);

            byte[] result = BitConverter.GetBytes(Result);

            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            offset += 4;
            result.CopyTo(buffer, offset);

            return buffer;
        }

    }

    public class LeaveRoomReq : HeaderPacket
    {
        public int Result {get; set;}
        public new static LeaveRoomReq Deserialize(byte[] data)
        {
            LeaveRoomReq packet = new LeaveRoomReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            offset += 4;
            packet.Result = BitConverter.ToInt32(data, offset);

            return packet;
        }
    }

    public class LeaveRoomRes : HeaderPacket
    {
        public int Result {get; set;}

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[12];

            Size = (ushort)(buffer.Length);

            byte[] result = BitConverter.GetBytes(Result);

            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            offset += 4;
            result.CopyTo(buffer, offset);

            return buffer;
        }

    }

    public class GetRoomListReq : HeaderPacket 
    {
        public int ChannelID { get; set; }

        public new static GetRoomListReq Deserialize(byte[] data)
        {
            GetRoomListReq packet = new GetRoomListReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.ChannelID = BitConverter.ToInt32(data, offset);


            return packet;
        }

    }

    public class GetRoomListRes : HeaderPacket 

    {
        public int unknownValue1 {get; set; }
        public int unknownValue2 {get; set; }
        public int unknownValue3 {get; set; }
        public int unknownValue4 {get; set; } 
        public int unknownValue5 {get; set; }
        public int unknownValue6 {get; set; }
        public int unknownValue7 {get; set; }
        public string RoomMasterIGN {get; set;}
        public int unknownValue8 {get; set; }
        public string SomeID {get; set;}
        public string RoomName {get; set;}
        public int unknownValue9 {get; set; }
        public int PlayerNumber {get; set; }
        public int MaxPlayers {get; set; }
        public int unknownValue10 {get; set; }
        public int MatchType {get; set; } //Single or Team
        public int GameMode {get; set; }
        public int unknownValue12 {get; set; }
        public int unknownValue13 {get; set; }
        public byte[] Time { get; set; }
        public string Country {get; set;}
        public string Region {get; set;}
            

        public override byte[] Serialize()
        {
            byte[] buffer = new byte[239];

            Size = (ushort)(buffer.Length);
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            byte[] unknownval1 = BitConverter.GetBytes(unknownValue1);
            byte[] unknownval2 = BitConverter.GetBytes(unknownValue2);
            byte[] unknownval3 = BitConverter.GetBytes(unknownValue3);
            byte[] unknownval4 = BitConverter.GetBytes(unknownValue4);
            byte[] unknownval5 = BitConverter.GetBytes(unknownValue5);
            byte[] unknownval6 = BitConverter.GetBytes(unknownValue6);
            byte[] unknownval7 = BitConverter.GetBytes(unknownValue7);
            byte[] master = Encoding.ASCII.GetBytes(RoomMasterIGN);
            byte[] unknownval8 = BitConverter.GetBytes(unknownValue8);
            byte[] someid = Encoding.ASCII.GetBytes(SomeID);
            byte[] roomname = Encoding.ASCII.GetBytes(RoomName);
            byte[] unknownval9 = BitConverter.GetBytes(unknownValue9);
            byte[] maxplayers = BitConverter.GetBytes(MaxPlayers);
            byte[] playernumber = BitConverter.GetBytes(PlayerNumber);
            byte[] unknownval10 = BitConverter.GetBytes(unknownValue10);
            byte[] matchtype = BitConverter.GetBytes(MatchType);

            byte[] gamemode = BitConverter.GetBytes(GameMode);
            byte[] unknownval12 = BitConverter.GetBytes(unknownValue12);
            byte[] unknownval13 = BitConverter.GetBytes(unknownValue13);

            byte[] country = Encoding.ASCII.GetBytes(Country);
            byte[] region = Encoding.ASCII.GetBytes(Region);

            offset += 2;
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
            offset += 4;
            unknownval7.CopyTo(buffer, offset);
            offset += 4;
            master.CopyTo(buffer, offset);
            offset += 20;
            unknownval8.CopyTo(buffer, offset);
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