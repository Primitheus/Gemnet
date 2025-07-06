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
        public int unknownvalue1 {get; set;} //0x08
        public int P2PID {get; set;} // goes into unknownValue5
        public string GroupP2PID {get; set;}
        public string RoomName {get; set;}
        public byte isPassword {get; set;} //goes into unknownValue6 password protected or not
        public byte MaxPlayers {get; set;}
        public byte PlayerNumber {get; set;}
        public byte GameState { get; set; }
        public int unknownvalue5 { get; set; } //goes into unknownValue7
        public int MatchType {get; set;}
        public int unknownvalue6 {get; set;} //goes into unknownValue9
        public byte BattleType {get; set;} //goes into unknownValue10
        public int RoundNumber {get; set;}
        public int GameMode1 {get; set;}
        public int GameMode2 {get; set;} //goes into unknownValue11
        public int GameMode3 {get; set;} //goes into unknownValue12

        private class PropertyOffsets
        {
            public static readonly int unkownvalue1 = 6; 
            public static readonly int P2PID = 8;
            public static readonly int GroupP2PID = 12;
            public static readonly int RoomName = 60;
            public static readonly int isPassword = 100;
            public static readonly int MaxPlayers = 101;
            public static readonly int PlayerNumber = 102;
            public static readonly int GameState = 103;
            public static readonly int unknownvalue5 = 104;
            public static readonly int MatchType = 105;
            public static readonly int unknownvalue6 = 106;
            public static readonly int BattleType = 107;
            public static readonly int RoundNumber = 108;
            public static readonly int GameMode1 = 109;
            public static readonly int GameMode2 = 113;
            public static readonly int GameMode3 = 117;

        }
        public new static CreateRoomReq Deserialize(byte[] data)
        {
            CreateRoomReq packet = new CreateRoomReq();

            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.unknownvalue1 = BitConverter.ToInt32(data, PropertyOffsets.unkownvalue1);
            packet.P2PID = BitConverter.ToInt32(data, PropertyOffsets.P2PID); 

            packet.GroupP2PID = Encoding.ASCII.GetString(data, PropertyOffsets.GroupP2PID, 4); // again, probably the p2p room id?
            nullTerminator = packet.GroupP2PID.IndexOf('\x00');
            packet.GroupP2PID = packet.GroupP2PID.Remove(nullTerminator);

            packet.RoomName = Encoding.ASCII.GetString(data, PropertyOffsets.RoomName, 32);
            nullTerminator = packet.RoomName.IndexOf('\x00');
            packet.RoomName = packet.RoomName.Remove(nullTerminator);
            
            packet.isPassword = (byte)data[PropertyOffsets.isPassword];
            
            packet.MaxPlayers = (byte)(data[PropertyOffsets.MaxPlayers]);; 
            packet.PlayerNumber =  (byte)(data[PropertyOffsets.PlayerNumber]);

            packet.unknownvalue5 = Convert.ToInt32(data[PropertyOffsets.unknownvalue5]);

            packet.MatchType = Convert.ToInt32(data[PropertyOffsets.MatchType]);

            packet.unknownvalue6 = Convert.ToInt32(data[PropertyOffsets.unknownvalue6]);
            packet.BattleType = data[PropertyOffsets.BattleType];

            packet.RoundNumber = Convert.ToInt32(data[PropertyOffsets.RoundNumber]);

            packet.GameMode1 = Convert.ToInt32(data[PropertyOffsets.GameMode1]);
            packet.GameMode2 = Convert.ToInt32(data[PropertyOffsets.GameMode2]);
            packet.GameMode3 = Convert.ToInt32(data[PropertyOffsets.GameMode3]);

            return packet;
        }


    }

    public class CreateRoomRes : HeaderPacket
    {

        public int Unknown1 {get; set;} //Currently guessing it's a result packet.
        public ushort RoomID { get; set; }
        public ushort Unknown2 { get; set; }

        private class PropertyOffsets
        {
            public static readonly int Unknown1 = 0;
            public static readonly int RoomID = 10;
            public static readonly int Unknown2 = 12;

        }

        public override byte[] Serialize()
        {

            Size = (ushort)(16);
            byte[] buffer = new byte[Size];
            // int offset = 0;

            // base.Serialize().CopyTo(buffer, offset);
            // offset += 6;

            byte[] data = { 0x02, 0x40, 0x00, 0x0f, 0x87, 0x00 };

            data.CopyTo(buffer, 0);

            // BitConverter.GetBytes(Unknown1).CopyTo(buffer, PropertyOffsets.Unknown1);
            // BitConverter.GetBytes(RoomID).CopyTo(buffer, PropertyOffsets.RoomID);
            // BitConverter.GetBytes(Unknown2).CopyTo(buffer, PropertyOffsets.Unknown2);


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

    public class RoomInfoA
    {
        public int unknownValue1 {get; set;}
        public int unknownValue2 {get; set;}
        public int NumberOfRooms {get; set;}
        public ushort RoomID {get; set;}
        public int PlayerLevel { get; set; }
        public int EXP { get; set; }
        public string RoomMasterIGN {get; set;}
        public int P2PID {get; set;}
        public string SomeID {get; set;}
        public string RoomName {get; set;}
        public int isPassword {get; set;}
        public int MaxPlayers {get; set;}
        public int PlayerNumber {get; set;}
        public int GameState {get; set;}
        public int unknownValue7 {get; set;}
        public int MatchType {get; set;}
        public int unknownValue8 {get; set;}
        public int unknownValue9 {get; set;}
        public byte BattleType {get; set;}
        public int RoundNumber {get; set;}
        public int GameMode1 {get; set;}
        public int GameMode2 {get; set;}
        public int GameMode3 {get; set;}
        public byte[] Time {get; set;}
        public string Country {get; set;}
        public string Region {get; set;}

    }

    public class GetRoomListRes : HeaderPacket 
    {

        public List<RoomInfoA> Rooms { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 8;
            public static readonly int unknownValue2 = 12;
            public static readonly int NumberOfRooms = 14;
            public static readonly int RoomID = 20;
            public static readonly int PlayerLevel = 22;
            public static readonly int EXP = 26;
            public static readonly int RoomMasterIGN = 30;
            public static readonly int P2PID = 50;
            public static readonly int SomeID = 54;
            public static readonly int RoomName = 86;
            public static readonly int unknownValue6 = 126;
            public static readonly int MaxPlayers = 127;
            public static readonly int PlayerNumber = 128;
            public static readonly int GameState = 129;
            public static readonly int unknownValue7 = 130;
            public static readonly int MatchType = 131;
            public static readonly int unknownValue8 = 133;
            public static readonly int unknownValue9 = 134;
            public static readonly int BattleType = 135;
            public static readonly int RoundNumber = 136;
            public static readonly int GameMode1 = 137;
            public static readonly int GameMode2 = 141;
            public static readonly int GameMode3 = 145;
            public static readonly int Time = 191;
            public static readonly int Country = 199;
            public static readonly int Region = 207;

        }
        
        public GetRoomListRes()
        {
            Rooms = new List<RoomInfoA>();

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
                    BitConverter.GetBytes(room.RoomID).CopyTo(buffer, PropertyOffsets.RoomID+i);
                    BitConverter.GetBytes(Rooms.Count).CopyTo(buffer, PropertyOffsets.NumberOfRooms+i);
                
                }
               
                BitConverter.GetBytes(room.RoomID).CopyTo(buffer, PropertyOffsets.RoomID+i);
                BitConverter.GetBytes(room.PlayerLevel).CopyTo(buffer, PropertyOffsets.PlayerLevel+i);
                BitConverter.GetBytes(room.EXP).CopyTo(buffer, PropertyOffsets.EXP+i);
                Encoding.ASCII.GetBytes(room.RoomMasterIGN).CopyTo(buffer, PropertyOffsets.RoomMasterIGN+i);
                BitConverter.GetBytes(room.P2PID).CopyTo(buffer, PropertyOffsets.P2PID+i);
                Encoding.ASCII.GetBytes(room.SomeID).CopyTo(buffer, PropertyOffsets.SomeID+i);
                Encoding.ASCII.GetBytes(room.RoomName).CopyTo(buffer, PropertyOffsets.RoomName+i);
                BitConverter.GetBytes(room.isPassword).CopyTo(buffer, PropertyOffsets.unknownValue6+i);
                BitConverter.GetBytes(room.MaxPlayers).CopyTo(buffer, PropertyOffsets.MaxPlayers+i);
                BitConverter.GetBytes(room.PlayerNumber).CopyTo(buffer, PropertyOffsets.PlayerNumber+i);
                BitConverter.GetBytes(room.GameState).CopyTo(buffer, PropertyOffsets.GameState+i);
                BitConverter.GetBytes(room.unknownValue7).CopyTo(buffer, PropertyOffsets.unknownValue7+i);
                BitConverter.GetBytes(room.MatchType).CopyTo(buffer, PropertyOffsets.MatchType+i);
                                     
                BitConverter.GetBytes(room.unknownValue8).CopyTo(buffer, PropertyOffsets.unknownValue8+i);
                BitConverter.GetBytes(room.unknownValue9).CopyTo(buffer, PropertyOffsets.unknownValue9+i);
                buffer[PropertyOffsets.BattleType+i] = room.BattleType;

                BitConverter.GetBytes(room.GameMode1).CopyTo(buffer, PropertyOffsets.GameMode1+i);
                
                BitConverter.GetBytes(room.GameMode2).CopyTo(buffer, PropertyOffsets.GameMode2+i);
                BitConverter.GetBytes(room.GameMode3).CopyTo(buffer, PropertyOffsets.GameMode3+i); 
                room.Time.CopyTo(buffer, PropertyOffsets.Time+i);
                Encoding.ASCII.GetBytes(room.Country).CopyTo(buffer, PropertyOffsets.Country+i);
                Encoding.ASCII.GetBytes(room.Region).CopyTo(buffer, PropertyOffsets.Region+i);

                i = i + 235 - 12;

            }
            return buffer;
        }

    }
}