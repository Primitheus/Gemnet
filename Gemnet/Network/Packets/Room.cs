using Gemnet.Network.Header;
using Microsoft.VisualBasic;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Crypto.Prng;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class LeaveRoomReq : HeaderPacket
    {
        public int Unknown { get; set; }
        public ushort RoomID { get; set; } // This is the room ID, not the P2P ID

        private struct PropertyOffsets
        {
            public static readonly int Unknown = 6; // goes into Unknown
            public static readonly int ID = 10;



        }

        public new static LeaveRoomReq Deserialize(byte[] data)
        {
            LeaveRoomReq packet = new LeaveRoomReq();

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.Unknown = Convert.ToInt32(data[PropertyOffsets.Unknown]);
            packet.RoomID = Convert.ToUInt16(data[PropertyOffsets.ID]);

            return packet;
        }
    }

    public class LeaveRoomRes : HeaderPacket
    {
        public int Unknown { get; set; }
        public ushort RoomID { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int Unknown = 6; // goes into Unknown
            public static readonly int RoomID = 10;

        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[12];
            Size = (ushort)(buffer.Length);

            int offset = 0;
            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(Unknown).CopyTo(buffer, PropertyOffsets.Unknown);
            BitConverter.GetBytes(RoomID).CopyTo(buffer, PropertyOffsets.RoomID);

            return buffer;
        }

    }

    public class JoinRoomReq : HeaderPacket
    {
        public int UnknownValue1 { get; set; } // goes into UnknownValue1 6
        public ushort RoomID { get; set; } // goes into UnknownValue2 10
        public int UnknownValue3 { get; set; } // goes into UnknownValue3
        public int UnknownValue4 { get; set; } // goes into UnknownValue4
        public ushort SlotID { get; set; } // goes into UnknownValue5 // 18 SlotID? Guess
        public int UnknownValue6 { get; set; } // goes into UnknownValue6 // 20
        public int P2PID { get; set; } // goes into UnknownValue7 THIS IS THE P2P ID!
        public string SomeID { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int UnknownValue1 = 6;
            public static readonly int RoomID = 10;
            public static readonly int UnknownValue3 = 14;
            public static readonly int UnknownValue4 = 16;
            public static readonly int SlotID = 20;
            public static readonly int UnknownValue6 = 22;
            public static readonly int P2PID = 28;
            public static readonly int SomeID = 32;

        }

        public new static JoinRoomReq Deserialize(byte[] data)
        {
            JoinRoomReq packet = new JoinRoomReq();
            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.UnknownValue1 = BitConverter.ToInt32(data, PropertyOffsets.UnknownValue1);
            packet.RoomID = BitConverter.ToUInt16(data, PropertyOffsets.RoomID);
            packet.UnknownValue3 = BitConverter.ToInt32(data, PropertyOffsets.UnknownValue3);
            packet.UnknownValue4 = BitConverter.ToInt32(data, PropertyOffsets.UnknownValue4);
            packet.SlotID = BitConverter.ToUInt16(data, PropertyOffsets.SlotID);
            packet.UnknownValue6 = BitConverter.ToInt32(data, PropertyOffsets.UnknownValue6);
            packet.P2PID = BitConverter.ToInt32(data, PropertyOffsets.P2PID);
            packet.SomeID = Encoding.ASCII.GetString(data, PropertyOffsets.SomeID, 4); // again, probably the p2p room id?
            nullTerminator = packet.SomeID.IndexOf('\x00');
            packet.SomeID = packet.SomeID.Remove(nullTerminator);

            return packet;
        }
    }

    public class JoinRoomRes : HeaderPacket
    {
        public int UnknownValue1 { get; set; }
        public ushort RoomID { get; set; }
        public int SlotID { get; set; } // Guess
        public int UnknownValue4 { get; set; }
        public int UnknownValue5 { get; set; }
        public int UnknownValue6 { get; set; }
        public string RoomMaster { get; set; }
        public int UnknownValue7 { get; set; }
        public string SomeID { get; set; }
        public string RoomName { get; set; }
        public byte isPassword { get; set; }
        public int PlayerNumber { get; set; }
        public int MaxPlayers { get; set; }
        public byte GameState { get; set; } // Game State
        public ushort unknownValue7 { get; set; }
        public ushort unknownValue8 { get; set; }

        public byte MatchType { get; set; } //Single or Team
        public byte BattleType { get; set; }
        public byte RoundNumber { get; set; }
        public ushort GameMode1 { get; set; }
        public ushort GameMode2 { get; set; }
        public ushort GameMode3 { get; set; }
        public ushort Map1 { get; set; }
        public ushort Map2 { get; set; }
        public ushort Map3 { get; set; }
        

        public int UnknownValue13 { get; set; }
        public byte[] Time { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int UnknownValue1 = 6;
            public static readonly int RoomID = 10;
            public static readonly int SlotID = 12; // SlotID Guess
            public static readonly int UnknownValue4 = 14;
            public static readonly int UnknownValue5 = 16;
            public static readonly int UnknownValue6 = 18;
            public static readonly int RoomMaster = 28;
            public static readonly int UnknownValue7 = 48;
            public static readonly int SomeID = 52;
            public static readonly int RoomName = 84;
            public static readonly int isPassword = 124;
            public static readonly int MaxPlayers = 125;
            public static readonly int PlayerNumber = 126;
            public static readonly int GameState = 127; //GameState
            public static readonly int unknownValue7 = 128;
            public static readonly int MatchType = 131;
            public static readonly int unknownValue8 = 132;
            public static readonly int BattleType = 133;
            public static readonly int RoundNumber = 134;
            public static readonly int GameMode1 = 135;
            public static readonly int GameMode2 = 139;
            public static readonly int GameMode3 = 143;

            public static readonly int Map1 = 137;
            public static readonly int Map2 = 141;
            public static readonly int Map3 = 145;


            public static readonly int UnknownValue13 = 189;
            public static readonly int UnknownValue14 = 191;
            public static readonly int UnknownValue15 = 193;
            public static readonly int UnknownValue16 = 195;
            public static readonly int Country = 197;
            public static readonly int Region = 205;

        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[238];
            Size = (ushort)buffer.Length;

            int offset = 0;
            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(UnknownValue1).CopyTo(buffer, PropertyOffsets.UnknownValue1);
            BitConverter.GetBytes(RoomID).CopyTo(buffer, PropertyOffsets.RoomID);
            BitConverter.GetBytes(SlotID).CopyTo(buffer, PropertyOffsets.SlotID);
            BitConverter.GetBytes(UnknownValue4).CopyTo(buffer, PropertyOffsets.UnknownValue4);
            BitConverter.GetBytes(UnknownValue5).CopyTo(buffer, PropertyOffsets.UnknownValue5);
            BitConverter.GetBytes(UnknownValue6).CopyTo(buffer, PropertyOffsets.UnknownValue6);
            Encoding.ASCII.GetBytes(RoomMaster).CopyTo(buffer, PropertyOffsets.RoomMaster);
            BitConverter.GetBytes(UnknownValue7).CopyTo(buffer, PropertyOffsets.UnknownValue7);
            Encoding.ASCII.GetBytes(SomeID).CopyTo(buffer, PropertyOffsets.SomeID);
            Encoding.ASCII.GetBytes(RoomName).CopyTo(buffer, PropertyOffsets.RoomName);
            buffer[PropertyOffsets.isPassword] = isPassword;
            BitConverter.GetBytes(MaxPlayers).CopyTo(buffer, PropertyOffsets.MaxPlayers);
            BitConverter.GetBytes(PlayerNumber).CopyTo(buffer, PropertyOffsets.PlayerNumber);
            buffer[PropertyOffsets.GameState] = GameState;
            BitConverter.GetBytes(unknownValue7).CopyTo(buffer, PropertyOffsets.unknownValue7);
            buffer[PropertyOffsets.MatchType] = MatchType;
            BitConverter.GetBytes(unknownValue8).CopyTo(buffer, PropertyOffsets.unknownValue8);
            buffer[PropertyOffsets.BattleType] = BattleType;
            buffer[PropertyOffsets.RoundNumber] = RoundNumber;

            BitConverter.GetBytes(GameMode1).CopyTo(buffer, PropertyOffsets.GameMode1);
            BitConverter.GetBytes(GameMode2).CopyTo(buffer, PropertyOffsets.GameMode2);
            BitConverter.GetBytes(GameMode3).CopyTo(buffer, PropertyOffsets.GameMode3);

            BitConverter.GetBytes(Map1).CopyTo(buffer, PropertyOffsets.Map1);
            BitConverter.GetBytes(Map2).CopyTo(buffer, PropertyOffsets.Map2);
            BitConverter.GetBytes(Map3).CopyTo(buffer, PropertyOffsets.Map3);

            BitConverter.GetBytes(5).CopyTo(buffer, PropertyOffsets.UnknownValue13);
            BitConverter.GetBytes(5).CopyTo(buffer, PropertyOffsets.UnknownValue14);
            BitConverter.GetBytes(5).CopyTo(buffer, PropertyOffsets.UnknownValue15);
            BitConverter.GetBytes(5).CopyTo(buffer, PropertyOffsets.UnknownValue16); // to here
            Encoding.ASCII.GetBytes(Country).CopyTo(buffer, PropertyOffsets.Country);
            Encoding.ASCII.GetBytes(Region).CopyTo(buffer, PropertyOffsets.Region);

            return buffer;
        }
    }

    public class Player
    {
        public int unknownValue1 { get; set; }
        public int PlayerLevel { get; set; }
        public int EXP { get; set; }
        public string IGN { get; set; }
        public byte Team { get; set; }
        public int P2PID { get; set; }
        public string SomeID { get; set; }
        public int[] ItemID { get; set; }
        public byte unknownValue4 { get; set; }
        public byte unknownValue5 { get; set; }
        public int unknownValue6 { get; set; }
        public int unknownValue7 { get; set; }
        public int unknownValue8 { get; set; }
        public int unknownValue9 { get; set; }
        public int unknownValue10 { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }

    }

    public class GetPlayersReq : HeaderPacket

    {
        public int unknownValue1 { get; set; }
        public ushort RoomID { get; set; }

        public struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            public static readonly int RoomID = 10;
        }
        public new static GetPlayersReq Deserialize(byte[] data)
        {
            GetPlayersReq packet = new GetPlayersReq();

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.unknownValue1 = Convert.ToInt32(data[PropertyOffsets.unknownValue1]);
            packet.RoomID = Convert.ToUInt16(data[PropertyOffsets.RoomID]);


            return packet;
        }


    }

    public class GetPlayersRes : HeaderPacket
    {
        public int unknownValue1 { get; set; }
        public int RoomID { get; set; }
        public int unknownValue3 { get; set; }
        public int PlayerNumber { get; set; }
        public List<Player> Players { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            public static readonly int RoomID = 8;
            public static readonly int unknownValue3 = 12;
            public static readonly int PlayerNumber = 14;
            public static readonly int Player = 16;

        }

        private struct PlayerPropertyOffsets
        {
            public static readonly int unknownValue1 = 0;
            public static readonly int PlayerLevel = 2;
            public static readonly int EXP = 6;
            public static readonly int IGN = 10;
            public static readonly int Team = 31;
            public static readonly int P2PID = 32;
            public static readonly int SomeID = 36;
            public static readonly int ItemID = 68;
            public static readonly int unknownValue4 = 1515;
            public static readonly int unknownValue5 = 1516;
            public static readonly int unknownValue6 = 1538;

            public static readonly int Country = 1554;
            public static readonly int Region = 1562;
        }



        public override byte[] Serialize()
        {
            //Console.WriteLine($"Serialize Player Data Begin");

            byte[] buffer = new byte[PlayerNumber * 1562 + 16 + 2000];
            Size = (ushort)buffer.Length;

            int offset = 0;
            var i = 16;
            var j = 0;

            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(unknownValue1).CopyTo(buffer, PropertyOffsets.unknownValue1);
            BitConverter.GetBytes(RoomID).CopyTo(buffer, PropertyOffsets.RoomID);
            BitConverter.GetBytes(unknownValue3).CopyTo(buffer, PropertyOffsets.unknownValue3);
            BitConverter.GetBytes(PlayerNumber).CopyTo(buffer, PropertyOffsets.PlayerNumber);

            //Console.WriteLine($"Serialize Player Data");

            foreach (var player in Players)
            {
                //Console.WriteLine($"Adding Player: {player.IGN}.");
                BitConverter.GetBytes(player.unknownValue1).CopyTo(buffer, PlayerPropertyOffsets.unknownValue1 + i);
                BitConverter.GetBytes(player.PlayerLevel).CopyTo(buffer, PlayerPropertyOffsets.PlayerLevel + i);
                BitConverter.GetBytes(player.EXP).CopyTo(buffer, PlayerPropertyOffsets.EXP + i);
                Encoding.ASCII.GetBytes(player.IGN).CopyTo(buffer, PlayerPropertyOffsets.IGN + i);
                buffer[PlayerPropertyOffsets.Team + i] = player.Team;

                BitConverter.GetBytes(player.P2PID).CopyTo(buffer, PlayerPropertyOffsets.P2PID + i);
                Encoding.ASCII.GetBytes(player.SomeID).CopyTo(buffer, PlayerPropertyOffsets.SomeID + i);

                j = 0;
                foreach (var item in player.ItemID)
                {
                    //Console.WriteLine($"Adding Item: {item}");
                    BitConverter.GetBytes(item).CopyTo(buffer, PlayerPropertyOffsets.ItemID + i + j);
                    j += 4;
                }

                buffer[PlayerPropertyOffsets.unknownValue4 + i] = 0x4e;
                buffer[PlayerPropertyOffsets.unknownValue5 + i] = 0x4e;
                BitConverter.GetBytes(player.unknownValue6).CopyTo(buffer, PlayerPropertyOffsets.unknownValue6 + i);
                Encoding.ASCII.GetBytes(player.Country).CopyTo(buffer, PlayerPropertyOffsets.Country + i);
                Encoding.ASCII.GetBytes(player.Region).CopyTo(buffer, PlayerPropertyOffsets.Region + i);
                i += 1594;

            }

            //Console.WriteLine("Finished Adding Players");


            return buffer;
        }
    }

    public class PlayerJoin
    {
        public int UserID { get; set; }
        public string IGN { get; set; }
        public int PlayerLevel { get; set; }
        public int EXP { get; set; }
        public byte Team { get; set; }
        public int P2PID { get; set; }
        public string SomeID { get; set; }
        public int[] ItemID { get; set; }
        public byte unknownValue2 { get; set; }
        public byte unknownValue3 { get; set; }

        public int unknownValue4 { get; set; }

        public int unknownValue5 { get; set; }

        public string Country { get; set; }
        public string Region { get; set; }

    }
    public class UserJoinedRes : HeaderPacket
    {
        public ushort SlotID { get; set; } // guess or might be number of players that follow which would be "1".
        public List<PlayerJoin> Players { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int SlotID = 6;
            public static readonly int Players = 8;

        }
        private struct PropertyPlayerOffsets
        {

            public static readonly int UserID = 0;
            public static readonly int IGN = 4;
            public static readonly int PlayerLevel = 24;
            public static readonly int EXP = 28;
            public static readonly int Team = 31;
            public static readonly int P2PID = 32;
            public static readonly int SomeID = 36;
            public static readonly int ItemID = 68;

            public static readonly int unknownValue2 = 1515;
            public static readonly int unknownValue3 = 1516;

            public static readonly int unknownValue4 = 1538;

            public static readonly int Country = 1555;
            public static readonly int Region = 1563;

            public static readonly int End = 1594;

        }
        public UserJoinedRes()
        {

            Players = new List<PlayerJoin>();

        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[1 * 1594 + 8 + 500];
            Size = (ushort)buffer.Length;

            int offset = 0;
            var i = 8;
            var j = 0;
            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(SlotID).CopyTo(buffer, PropertyOffsets.SlotID);

            foreach (var player in Players)
            {

                BitConverter.GetBytes(player.UserID).CopyTo(buffer, PropertyPlayerOffsets.UserID + i);
                Encoding.ASCII.GetBytes(player.IGN).CopyTo(buffer, PropertyPlayerOffsets.IGN + i);
                BitConverter.GetBytes(player.PlayerLevel).CopyTo(buffer, PropertyPlayerOffsets.PlayerLevel + i);
                BitConverter.GetBytes(player.EXP).CopyTo(buffer, PropertyPlayerOffsets.EXP + i);
                buffer[PropertyPlayerOffsets.Team + i] = player.Team;
                BitConverter.GetBytes(player.P2PID).CopyTo(buffer, PropertyPlayerOffsets.P2PID + i);
                Encoding.ASCII.GetBytes(player.SomeID).CopyTo(buffer, PropertyPlayerOffsets.SomeID + i);
                j = 0;
                foreach (var item in player.ItemID)
                {
                    BitConverter.GetBytes(item).CopyTo(buffer, PropertyPlayerOffsets.ItemID + j + i);
                    j += 4;
                }

                buffer[PropertyPlayerOffsets.unknownValue2 + i] = 0x4e;
                buffer[PropertyPlayerOffsets.unknownValue3 + i] = 0x4e;

                BitConverter.GetBytes(player.unknownValue4).CopyTo(buffer, PropertyPlayerOffsets.unknownValue4 + i);

                Encoding.ASCII.GetBytes(player.Country).CopyTo(buffer, PropertyPlayerOffsets.Country + i);
                Encoding.ASCII.GetBytes(player.Region).CopyTo(buffer, PropertyPlayerOffsets.Region + i);
                BitConverter.GetBytes(0).CopyTo(buffer, PropertyPlayerOffsets.End + i);
                i += 1595;

            }

            return buffer;
        }

    }

    public class UserReadyReq : HeaderPacket
    {
        public int unknownValue1 { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;

        }

        public new static UserReadyReq Deserialize(byte[] data)
        {

            UserReadyReq packet = new UserReadyReq();

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.unknownValue1 = Convert.ToInt32(data[PropertyOffsets.unknownValue1]);

            return packet;
        }

    }


    public class UserReadyRes : HeaderPacket
    {
        public string IGN { get; set; }
        public int unknownValue1 { get; set; }
        public int unknownValue2 { get; set; }

        public struct PropertyOffsets
        {
            public static readonly int IGN = 6;
            public static readonly int unknownValue1 = 26;
            public static readonly int unknownValue2 = 27;

        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[63];
            Size = (ushort)buffer.Length;

            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);

            Encoding.ASCII.GetBytes(IGN).CopyTo(buffer, PropertyOffsets.IGN);
            BitConverter.GetBytes(unknownValue1).CopyTo(buffer, PropertyOffsets.unknownValue1);
            BitConverter.GetBytes(unknownValue2).CopyTo(buffer, PropertyOffsets.unknownValue2);


            return buffer;

        }
    }

    public class StartMatchReq : HeaderPacket
    {
        public int unknownValue1 { get; set; }
        public int unknownValue2 { get; set; }
        public int unknownValue3 { get; set; }
        public int unknownValue4 { get; set; }


        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            //public static readonly int unknownValue2 = 8;
            public static readonly int unknownValue3 = 10;
            //public static readonly int unknownValue4 = 12;


        }

        public new static StartMatchReq Deserialize(byte[] data)
        {

            StartMatchReq packet = new StartMatchReq();

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.unknownValue1 = BitConverter.ToInt32(data, PropertyOffsets.unknownValue1);
            packet.unknownValue3 = BitConverter.ToInt32(data, PropertyOffsets.unknownValue3);

            return packet;
        }
    }

    public class StartMatchRes : HeaderPacket
    {
        public int unknownValue1 { get; set; }
        public int unknownValue3 { get; set; }
        public int unknownValue4 { get; set; }
        public int unknownValue5 { get; set; }
        public int unknownValue6 { get; set; }


        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            public static readonly int unknownValue3 = 10;
            public static readonly int unknownValue5 = 14;


        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[18];
            Size = (ushort)buffer.Length;

            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(unknownValue1).CopyTo(buffer, PropertyOffsets.unknownValue1);

            BitConverter.GetBytes(unknownValue3).CopyTo(buffer, PropertyOffsets.unknownValue3);
            BitConverter.GetBytes(unknownValue5).CopyTo(buffer, PropertyOffsets.unknownValue5);

            return buffer;

        }


    }

    public class LoadGameReq : HeaderPacket
    {
        public ushort SizeOfData { get; set; }
        public byte[] Data { get; set; }


        private struct PropertyOffsets
        {
            public static readonly int SizeOfData = 6;


        }

        public new static LoadGameReq Deserialize(byte[] data)
        {

            LoadGameReq packet = new LoadGameReq();

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            Console.WriteLine($"PACKET LENGTH: {packet.Size}");
            packet.SizeOfData = BitConverter.ToUInt16(data, PropertyOffsets.SizeOfData);

            int dataLength = packet.SizeOfData + 2;
            packet.Data = new byte[dataLength];
            Array.Copy(data, PropertyOffsets.SizeOfData, packet.Data, 0, dataLength);

            return packet;
        }

    }

    public class LoadGameRes : HeaderPacket
    {

        public string IGN { get; set; }
        public byte[] Data { get; set; }

        public struct PropertyOffsets
        {
            public static readonly int IGN = 6;
            public static readonly int Data = 26;


        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[26 + Data.Length];
            Size = (ushort)buffer.Length;

            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);

            Encoding.ASCII.GetBytes(IGN).CopyTo(buffer, PropertyOffsets.IGN);
            Data.CopyTo(buffer, PropertyOffsets.Data);

            return buffer;

        }

    }

    public class ChangeMapReq : HeaderPacket
    {
        public ushort unknownValue1 { get; set; }
        public byte ItemToggle { get; set; }

        public byte BattleType { get; set; }

        public byte RoundNumber { get; set; }
        public ushort GameMode1 { get; set; }
        public ushort GameMode2 { get; set; }
        public ushort GameMode3 { get; set; }

        public ushort Map1 { get; set; }
        public ushort Map2 { get; set; }
        public ushort Map3 { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            public static readonly int ItemToggle = 8;

            public static readonly int BattleType = 11;

            public static readonly int RoundNumber = 12;
            public static readonly int GameMode1 = 13;
            public static readonly int GameMode2 = 17;
            public static readonly int GameMode3 = 21;

            public static readonly int Map1 = 15;
            public static readonly int Map2 = 19;
            public static readonly int Map3 = 23;

        }

        public new static ChangeMapReq Deserialize(byte[] data)
        {
            ChangeMapReq packet = new ChangeMapReq();

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.unknownValue1 = Convert.ToUInt16(data[PropertyOffsets.unknownValue1]);
            packet.ItemToggle = data[PropertyOffsets.ItemToggle];

            packet.BattleType = data[PropertyOffsets.BattleType];

            packet.RoundNumber = data[PropertyOffsets.RoundNumber];
            packet.GameMode1 = Convert.ToUInt16(data[PropertyOffsets.GameMode1]);
            packet.GameMode2 = Convert.ToUInt16(data[PropertyOffsets.GameMode2]);
            packet.GameMode3 = Convert.ToUInt16(data[PropertyOffsets.GameMode3]);
            packet.Map1 = BitConverter.ToUInt16(data, PropertyOffsets.Map1);
            packet.Map2 = Convert.ToUInt16(data[PropertyOffsets.Map2]);
            packet.Map3 = Convert.ToUInt16(data[PropertyOffsets.Map3]);

            return packet;
        }

    }

    public class ChangeMapRes : HeaderPacket
    {
        public ushort unknownValue1 { get; set; }
        public byte ItemToggle { get; set; }

        public byte BattleType { get; set; }

        public byte RoundNumber { get; set; }
        public ushort GameMode1 { get; set; }
        public ushort GameMode2 { get; set; }
        public ushort GameMode3 { get; set; }

        public ushort Map1 { get; set; }
        public ushort Map2 { get; set; }
        public ushort Map3 { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            public static readonly int ItemToggle = 8;

            public static readonly int BattleType = 11;

            public static readonly int RoundNumber = 12;
            public static readonly int GameMode1 = 13;
            public static readonly int GameMode2 = 17;
            public static readonly int GameMode3 = 21;

            public static readonly int Map1 = 15;
            public static readonly int Map2 = 19;
            public static readonly int Map3 = 23;
        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[25];
            Size = (ushort)(buffer.Length);

            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(unknownValue1).CopyTo(buffer, PropertyOffsets.unknownValue1);

            buffer[PropertyOffsets.ItemToggle] = ItemToggle;

            buffer[PropertyOffsets.BattleType] = BattleType;
            buffer[PropertyOffsets.RoundNumber] = RoundNumber;

            BitConverter.GetBytes(GameMode1).CopyTo(buffer, PropertyOffsets.GameMode1);
            BitConverter.GetBytes(GameMode2).CopyTo(buffer, PropertyOffsets.GameMode2);
            BitConverter.GetBytes(GameMode3).CopyTo(buffer, PropertyOffsets.GameMode3);

            BitConverter.GetBytes(Map1).CopyTo(buffer, PropertyOffsets.Map1);
            BitConverter.GetBytes(Map2).CopyTo(buffer, PropertyOffsets.Map2);
            BitConverter.GetBytes(Map3).CopyTo(buffer, PropertyOffsets.Map3);

            return buffer;
        }


    }

    public class RoomUpdatePlayersRes : HeaderPacket
    {
        public string PlayerWhoLeft { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int PlayerWhoLeft = 6;

        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[26];
            Size = (ushort)(buffer.Length);

            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);

            Encoding.ASCII.GetBytes(PlayerWhoLeft).CopyTo(buffer, PropertyOffsets.PlayerWhoLeft);


            return buffer;
        }


    }

    public class UpdateRoomMasterRes : HeaderPacket
    {
        public string NewRoomMaster { get; set; }
        public int Unknown1 { get; set; }

        private struct PropertyOffsets
        {
            public static readonly int NewRoomMaster = 6;
            public static readonly int Unknown1 = 26;


        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[30];
            Size = (ushort)(buffer.Length);

            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);

            Encoding.ASCII.GetBytes(NewRoomMaster).CopyTo(buffer, PropertyOffsets.NewRoomMaster);
            BitConverter.GetBytes(Unknown1).CopyTo(buffer, PropertyOffsets.Unknown1);


            return buffer;
        }



    }

    public class SelectTeamReq : HeaderPacket
    {

        public byte Team { get; set; } // This is the room ID, not the P2P ID

        private struct PropertyOffsets
        {
            public static readonly int Team = 6;



        }

        public new static SelectTeamReq Deserialize(byte[] data)
        {
            SelectTeamReq packet = new SelectTeamReq();

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.Team = data[PropertyOffsets.Team];


            return packet;
        }


    }


    public class SelectTeamRes : HeaderPacket
    {

        public string UserIGN { get; set; }
        public byte Team { get; set; } // This is the room ID, not the P2P ID

        private struct PropertyOffsets
        {
            public static readonly int UserIGN = 6; // goes into Unknown
            public static readonly int Team = 26;


        }

        public override byte[] Serialize()
        {
            byte[] buffer = new byte[27];
            Size = (ushort)(buffer.Length);

            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);

            Encoding.ASCII.GetBytes(UserIGN).CopyTo(buffer, PropertyOffsets.UserIGN);
            buffer[PropertyOffsets.Team] = Team;

            return buffer;
        
        }


    }


}