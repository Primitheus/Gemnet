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
        public int unknownvalue3 {get; set;}
        public string SomeID2 {get; set;}
        public string RoomName {get; set;}
        public string Password {get; set;}
        public int unknownvalue4 {get; set;}
        public int MaxPlayers {get; set;}
        public int PlayerNumber {get; set;}
        public int MatchType {get; set;}
        public int GameMode {get; set;}
        public int unknownvalue5 {get; set;}
        public int unknownvalue6 {get; set;}

        private struct PropertyOffsets
        {
            public static readonly int unkownvalue1 = 6; 
            public static readonly int unkownvalue2 = 8;
            public static readonly int SomeID = 12;
            public static readonly int unknownvalue3 = 50;
            public static readonly int SomeID2 = 54;
            public static readonly int RoomName = 60;
            public static readonly int Password = 86;
            public static readonly int unknownvalue4 = 100;
            public static readonly int MaxPlayers = 101;
            public static readonly int PlayerNumber = 102;
            public static readonly int MatchType = 104;
            public static readonly int GameMode = 108;
            public static readonly int unknownvalue5 = 113;
            public static readonly int unknownvalue6 = 117;

        }
        public new static CreateRoomReq Deserialize(byte[] data)
        {
            CreateRoomReq packet = new CreateRoomReq();

            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.unkownvalue1 = BitConverter.ToInt32(data, PropertyOffsets.unkownvalue1); // seems to always be 0x8c not sure.
            packet.unkownvalue2 = BitConverter.ToInt32(data, PropertyOffsets.unkownvalue2); 
            packet.SomeID = Encoding.ASCII.GetString(data, PropertyOffsets.SomeID, 4); // again, probably the p2p room id?
            nullTerminator = packet.SomeID.IndexOf('\x00');
            packet.SomeID = packet.SomeID.Remove(nullTerminator);
            packet.unknownvalue3 = BitConverter.ToInt32(data, PropertyOffsets.unknownvalue3); 

            packet.SomeID2 = Encoding.ASCII.GetString(data, PropertyOffsets.SomeID2, 4);
            nullTerminator = packet.SomeID2.IndexOf('\x00');
            packet.SomeID2 = packet.SomeID2.Remove(nullTerminator);

            packet.RoomName = Encoding.ASCII.GetString(data, PropertyOffsets.RoomName, 32);
            nullTerminator = packet.RoomName.IndexOf('\x00');
            packet.RoomName = packet.RoomName.Remove(nullTerminator);

            packet.Password = Encoding.ASCII.GetString(data, PropertyOffsets.Password, 14);
            nullTerminator = packet.Password.IndexOf('\x00');
            packet.Password = packet.Password.Remove(nullTerminator);
            
            packet.unknownvalue4 = BitConverter.ToInt32(data, PropertyOffsets.unknownvalue4); 

            packet.MaxPlayers = BitConverter.ToInt32(data, PropertyOffsets.MaxPlayers); 
            packet.PlayerNumber = BitConverter.ToInt32(data, PropertyOffsets.PlayerNumber); 

            packet.MatchType = BitConverter.ToInt32(data, PropertyOffsets.MatchType); 
            packet.GameMode = BitConverter.ToInt32(data, PropertyOffsets.GameMode); 

            packet.unknownvalue5 = BitConverter.ToInt32(data, PropertyOffsets.unknownvalue5); 
            packet.unknownvalue6 = BitConverter.ToInt32(data, PropertyOffsets.unknownvalue6); 



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

    public class RoomInfo
    {
        public int unknownValue1 {get; set; }
        public int unknownValue2 {get; set; }
        public int NumberOfRooms {get; set;}
        public int unknownValue3 {get; set; }
        public int unknownValue4 {get; set; } 
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

    }

    public class GetRoomListRes : HeaderPacket 
    {

        public List<RoomInfo> Rooms { get; set; }

        private struct PropertyOffsets
        {
        public static readonly int unknownValue1 = 8;
        public static readonly int unknownValue2 = 12;
        public static readonly int NumberOfRooms = 14;
        public static readonly int unknownValue3 = 20;
        public static readonly int unknownValue4 = 26;
        public static readonly int RoomMasterIGN = 30;
        public static readonly int unknownValue8 = 50;
        public static readonly int SomeID = 54;
        public static readonly int RoomName = 86;
        public static readonly int unknownValue9 = 126;
        public static readonly int MaxPlayers = 127;
        public static readonly int PlayerNumber = 128;
        public static readonly int unknownValue10 = 129;
        public static readonly int MatchType = 132;
        public static readonly int GameMode = 136;
        public static readonly int unknownValue12 = 141;
        public static readonly int unknownValue13 = 145;
        public static readonly int Time = 191;
        public static readonly int Country = 199;
        public static readonly int Region = 207;

        }
        
        public GetRoomListRes()
        {
            Rooms = new List<RoomInfo>();

        }


        public override byte[] Serialize()
        {
            int roomCount = Rooms.Count;
            int bufferSize = 6 + (roomCount * 233);
            byte[] buffer = new byte[bufferSize];

            Size = (ushort)buffer.Length;
            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
            var i = 0;

            foreach (var room in Rooms) 
            {
                if (i == 0) {
                BitConverter.GetBytes(room.unknownValue1).CopyTo(buffer, PropertyOffsets.unknownValue1+i);
                BitConverter.GetBytes(room.unknownValue2).CopyTo(buffer, PropertyOffsets.unknownValue2+i);
                BitConverter.GetBytes(room.NumberOfRooms).CopyTo(buffer, PropertyOffsets.NumberOfRooms+i);
                }
               
                BitConverter.GetBytes(room.unknownValue3).CopyTo(buffer, PropertyOffsets.unknownValue3+i);
                BitConverter.GetBytes(room.unknownValue4).CopyTo(buffer, PropertyOffsets.unknownValue4+i);
                Encoding.ASCII.GetBytes(room.RoomMasterIGN).CopyTo(buffer, PropertyOffsets.RoomMasterIGN+i);
                BitConverter.GetBytes(room.unknownValue8).CopyTo(buffer, PropertyOffsets.unknownValue8+i);
                Encoding.ASCII.GetBytes(room.SomeID).CopyTo(buffer, PropertyOffsets.SomeID+i);
                Encoding.ASCII.GetBytes(room.RoomName).CopyTo(buffer, PropertyOffsets.RoomName+i);
                BitConverter.GetBytes(room.unknownValue9).CopyTo(buffer, PropertyOffsets.unknownValue9+i);
                BitConverter.GetBytes(room.MaxPlayers).CopyTo(buffer, PropertyOffsets.MaxPlayers+i);
                BitConverter.GetBytes(room.PlayerNumber).CopyTo(buffer, PropertyOffsets.PlayerNumber+i);
                BitConverter.GetBytes(room.MatchType).CopyTo(buffer, PropertyOffsets.MatchType+i);
                BitConverter.GetBytes(room.GameMode).CopyTo(buffer, PropertyOffsets.GameMode+i);
                BitConverter.GetBytes(room.unknownValue12).CopyTo(buffer, PropertyOffsets.unknownValue12+i);
                BitConverter.GetBytes(room.unknownValue13).CopyTo(buffer, PropertyOffsets.unknownValue13+i); 
                room.Time.CopyTo(buffer, PropertyOffsets.Time+i);
                Encoding.ASCII.GetBytes(room.Country).CopyTo(buffer, PropertyOffsets.Country+i);
                Encoding.ASCII.GetBytes(room.Region).CopyTo(buffer, PropertyOffsets.Region+i);

                i = i + 235 - 12;

            }


            

         
            return buffer;
        }

    }
}