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

    public class JoinRoomReq : HeaderPacket
    {
        public int UnknownValue1 {get; set;} // goes into UnknownValue1 6
        public int UnknownValue2 {get; set;} // goes into UnknownValue2 10
        public int UnknownValue3 {get; set;} // goes into UnknownValue3
        public int UnknownValue4 {get; set;} // goes into UnknownValue4
        public int UnknownValue5 {get; set;} // goes into UnknownValue5 // 18
        public int UnknownValue6 {get; set;} // goes into UnknownValue6 // 20
        public int UnknownValue7{get; set;} // goes into UnknownValue7 THIS IS THE P2P ID!
        public string SomeID {get; set;}

        private struct PropertyOffsets
        {
            public static readonly int UnknownValue1 = 6; 
            public static readonly int UnknownValue2 = 10;
            public static readonly int UnknownValue3 = 14;
            public static readonly int UnknownValue4 = 16;
            public static readonly int UnknownValue5 = 20;
            public static readonly int UnknownValue6 = 22;
            public static readonly int UnknownValue7 = 28;
            public static readonly int SomeID = 32;

        }

        public new static JoinRoomReq Deserialize(byte[] data)
        {
            JoinRoomReq packet = new JoinRoomReq();
            int nullTerminator = 0;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.UnknownValue1 = Convert.ToInt32(data[PropertyOffsets.UnknownValue1]);
            packet.UnknownValue2 = Convert.ToInt32(data[PropertyOffsets.UnknownValue2]);
            packet.UnknownValue3 = Convert.ToInt32(data[PropertyOffsets.UnknownValue3]);
            packet.UnknownValue4 = Convert.ToInt32(data[PropertyOffsets.UnknownValue4]);
            packet.UnknownValue5 = Convert.ToInt32(data[PropertyOffsets.UnknownValue5]);
            packet.UnknownValue6 = Convert.ToInt32(data[PropertyOffsets.UnknownValue6]);
            packet.UnknownValue7 = Convert.ToInt32(data[PropertyOffsets.UnknownValue7]);
            packet.SomeID = Encoding.ASCII.GetString(data, PropertyOffsets.SomeID, 4); // again, probably the p2p room id?
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
        public int GameState {get; set; } // Game State
        public int unknownValue7 {get; set;}
        public int MatchType {get; set; } //Single or Team
        public int unknownValue8 {get; set;}
        public int unknownValue9 {get; set;}
        public int RoundNumber {get; set;}
        public int GameMode {get; set; }
        public int UnknownValue11 {get; set; }
        public int UnknownValue12 {get; set; }
        public int UnknownValue13 {get; set; }
        public byte[] Time { get; set; }
        public string Country {get; set;}
        public string Region {get; set;}

        private struct PropertyOffsets
        {
            public static readonly int UnknownValue1 = 6;
            public static readonly int UnknownValue2 = 10;
            public static readonly int UnknownValue3 = 12; // SlotID
            public static readonly int UnknownValue4 = 14; 
            public static readonly int UnknownValue5 = 16;
            public static readonly int UnknownValue6 = 18;
            public static readonly int RoomMaster = 28;
            public static readonly int UnknownValue7 = 48;
            public static readonly int SomeID = 52;
            public static readonly int RoomName = 84;
            public static readonly int UnknownValue9 = 124;
            public static readonly int MaxPlayers = 125;
            public static readonly int PlayerNumber = 126;
            public static readonly int GameState = 127; //GameState
            public static readonly int unknownValue7 = 128;
            public static readonly int MatchType = 131;
            public static readonly int unknownValue8 = 132;
            public static readonly int unknownValue9 = 133;
            public static readonly int RoundNumber = 134;
            public static readonly int GameMode = 135;
            public static readonly int UnknownValue11 = 139;
            public static readonly int UnknownValue12 = 143;
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
            BitConverter.GetBytes(UnknownValue2).CopyTo(buffer, PropertyOffsets.UnknownValue2);
            BitConverter.GetBytes(UnknownValue3).CopyTo(buffer, PropertyOffsets.UnknownValue3);
            BitConverter.GetBytes(UnknownValue4).CopyTo(buffer, PropertyOffsets.UnknownValue4);
            BitConverter.GetBytes(UnknownValue5).CopyTo(buffer, PropertyOffsets.UnknownValue5);
            BitConverter.GetBytes(UnknownValue6).CopyTo(buffer, PropertyOffsets.UnknownValue6);
            Encoding.ASCII.GetBytes(RoomMaster).CopyTo(buffer, PropertyOffsets.RoomMaster);
            BitConverter.GetBytes(UnknownValue7).CopyTo(buffer, PropertyOffsets.UnknownValue7);
            Encoding.ASCII.GetBytes(SomeID).CopyTo(buffer, PropertyOffsets.SomeID);
            Encoding.ASCII.GetBytes(RoomName).CopyTo(buffer, PropertyOffsets.RoomName);
            BitConverter.GetBytes(UnknownValue9).CopyTo(buffer, PropertyOffsets.UnknownValue9);
            BitConverter.GetBytes(MaxPlayers).CopyTo(buffer, PropertyOffsets.MaxPlayers);
            BitConverter.GetBytes(PlayerNumber).CopyTo(buffer, PropertyOffsets.PlayerNumber);
            BitConverter.GetBytes(GameState).CopyTo(buffer, PropertyOffsets.GameState);
            BitConverter.GetBytes(unknownValue7).CopyTo(buffer, PropertyOffsets.unknownValue7);
            BitConverter.GetBytes(MatchType).CopyTo(buffer, PropertyOffsets.MatchType);
            BitConverter.GetBytes(unknownValue8).CopyTo(buffer, PropertyOffsets.unknownValue8);
            BitConverter.GetBytes(unknownValue9).CopyTo(buffer, PropertyOffsets.unknownValue9);
            BitConverter.GetBytes(RoundNumber).CopyTo(buffer, PropertyOffsets.RoundNumber);
            BitConverter.GetBytes(GameMode).CopyTo(buffer, PropertyOffsets.GameMode);
            BitConverter.GetBytes(UnknownValue11).CopyTo(buffer, PropertyOffsets.UnknownValue11); // All 0x01 from here
            BitConverter.GetBytes(UnknownValue12).CopyTo(buffer, PropertyOffsets.UnknownValue12);
            BitConverter.GetBytes(UnknownValue13).CopyTo(buffer, PropertyOffsets.UnknownValue13);
            BitConverter.GetBytes(UnknownValue13).CopyTo(buffer, PropertyOffsets.UnknownValue14);
            BitConverter.GetBytes(UnknownValue13).CopyTo(buffer, PropertyOffsets.UnknownValue15);
            BitConverter.GetBytes(UnknownValue13).CopyTo(buffer, PropertyOffsets.UnknownValue16); // to here
            Encoding.ASCII.GetBytes(Country).CopyTo(buffer, PropertyOffsets.Country);
            Encoding.ASCII.GetBytes(Region).CopyTo(buffer, PropertyOffsets.Region);

            return buffer;
        }
    }

    public class Player
    {
        public int unknownValue1 {get; set;}
        public int unknownValue2 {get; set;}
        public int EXP {get; set;}
        public string IGN { get; set; }
        public int P2PID {get; set;}
        public string SomeID {get; set;}
        public int[] ItemID { get; set; }
        public int unknownValue4 {get; set;}
        public int unknownValue5 {get; set;}
        public int unknownValue6 {get; set;}
        public int unknownValue7 {get; set;}
        public int unknownValue8 {get; set;}
        public int unknownValue9 {get; set;}
        public int unknownValue10 {get; set;}
        public string Country {get; set;}
        public string Region {get; set;}

    }

    public class GetPlayersReq : HeaderPacket

    {
        public int unknownValue1 {get; set;}
        public int unknownValue2 {get; set;}

        public struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            public static readonly int unknownValue2 = 10;
        }
        public new static GetPlayersReq Deserialize(byte[] data)
        {
            GetPlayersReq packet = new GetPlayersReq();

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.unknownValue1 = Convert.ToInt32(data[PropertyOffsets.unknownValue1]);
            packet.unknownValue2 = Convert.ToInt32(data[PropertyOffsets.unknownValue2]);
           

            return packet;
        }


    }

    public class GetPlayersRes : HeaderPacket
    {
        public int unknownValue1 {get; set;}
        public int unknownValue2 {get; set;}
        public int unknownValue3 {get; set;}
        public int PlayerNumber {get; set;}
        public List<Player> Players {get; set;}
        
        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            public static readonly int unknownValue2 = 8;
            public static readonly int unknownValue3 = 12;
            public static readonly int PlayerNumber = 14;
            public static readonly int Player = 16;

        }

        private struct PlayerPropertyOffsets 
        {
            public static readonly int unknownValue1 = 0;
            public static readonly int unknownValue2 = 2;
            public static readonly int EXP = 6;
            public static readonly int IGN = 10; 
            public static readonly int P2PID = 32;
            public static readonly int SomeID = 36;
            public static readonly int ItemID = 68;
            public static readonly int unknownValue4 = 1515;
            public static readonly int unknownValue5 = 1516;
            public static readonly int unknownValue6 = 1538;
            public static readonly int unknownValue7 = 1539;
            public static readonly int unknownValue8 = 1543;
            public static readonly int unknownValue9 = 1547;
            public static readonly int unknownValue10 = 1551;

            public static readonly int Country = 1554;
            public static readonly int Region = 1562;
        }



        public override byte[] Serialize()
        {
            Console.WriteLine($"Serialize Player Data Begin");

            byte[] buffer = new byte[PlayerNumber*1562 + 16 + 2000];
            Size = (ushort)buffer.Length;

            int offset = 0;
            var i = 16;
            var j = 0;

            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(unknownValue1).CopyTo(buffer, PropertyOffsets.unknownValue1);
            BitConverter.GetBytes(unknownValue2).CopyTo(buffer, PropertyOffsets.unknownValue2);
            BitConverter.GetBytes(unknownValue3).CopyTo(buffer, PropertyOffsets.unknownValue3);
            BitConverter.GetBytes(PlayerNumber).CopyTo(buffer, PropertyOffsets.PlayerNumber);
            
            Console.WriteLine($"Serialize Player Data");
            
            foreach (var player in Players) 
            {
                Console.WriteLine($"Adding Player: {player.IGN}.");
                BitConverter.GetBytes(player.unknownValue1).CopyTo(buffer, PlayerPropertyOffsets.unknownValue1+i);
                BitConverter.GetBytes(player.unknownValue2).CopyTo(buffer, PlayerPropertyOffsets.unknownValue2+i);
                BitConverter.GetBytes(player.EXP).CopyTo(buffer, PlayerPropertyOffsets.EXP+i);
                Encoding.ASCII.GetBytes(player.IGN).CopyTo(buffer, PlayerPropertyOffsets.IGN+i);
                BitConverter.GetBytes(player.P2PID).CopyTo(buffer, PlayerPropertyOffsets.P2PID+i);
                Encoding.ASCII.GetBytes(player.SomeID).CopyTo(buffer, PlayerPropertyOffsets.SomeID+i);

                j = 0;
                foreach (var item in player.ItemID) 
                {
                    Console.WriteLine($"Adding Item: {item}");
                    BitConverter.GetBytes(item).CopyTo(buffer, PlayerPropertyOffsets.ItemID+i+j);
                    j += 4;
                }

                BitConverter.GetBytes(player.unknownValue4).CopyTo(buffer, PlayerPropertyOffsets.unknownValue4+i);
                BitConverter.GetBytes(player.unknownValue5).CopyTo(buffer, PlayerPropertyOffsets.unknownValue5+i);
                BitConverter.GetBytes(player.unknownValue6).CopyTo(buffer, PlayerPropertyOffsets.unknownValue6+i);
                BitConverter.GetBytes(player.unknownValue7).CopyTo(buffer, PlayerPropertyOffsets.unknownValue7+i);
                BitConverter.GetBytes(player.unknownValue8).CopyTo(buffer, PlayerPropertyOffsets.unknownValue8+i);
                BitConverter.GetBytes(player.unknownValue9).CopyTo(buffer, PlayerPropertyOffsets.unknownValue9+i);
                BitConverter.GetBytes(player.unknownValue10).CopyTo(buffer, PlayerPropertyOffsets.unknownValue10+i);
                Encoding.ASCII.GetBytes(player.Country).CopyTo(buffer, PlayerPropertyOffsets.Country+i);
                Encoding.ASCII.GetBytes(player.Region).CopyTo(buffer, PlayerPropertyOffsets.Region+i);
                i += 1594;

            } 

            Console.WriteLine("Finished Adding Players");
          

            return buffer;
        }
    }

    public class PlayerJoin
    {
        public int UserID {get; set;}
        public string IGN {get; set;}
        public int unknownValue1 {get; set;}
        public int EXP {get; set;}
        public int P2PID {get; set;}
        public string SomeID {get; set;}
        public int[] ItemID { get; set; }
        public int unknownValue2 {get; set;}
        public int unknownValue3 {get; set;}
        public int unknownValue4 {get; set;}
        public int unknownValue5 {get; set;}
        public int unknownValue6 {get; set;}
        public int unknownValue7 {get; set;}
        public int unknownValue8 {get; set;}
        public string Country {get; set;}
        public string Region {get; set;}

    }
    public class UserJoinedRes : HeaderPacket 
    {
        public int unknownValue1 {get; set;}
        public List<PlayerJoin> Players { get; set; }

        private struct PropertyOffsets {
            public static readonly int unknownValue1 = 6;
            public static readonly int Players = 8;
            
        }
        private struct PropertyPlayerOffsets
        {
            
            public static readonly int UserID = 0;
            public static readonly int IGN = 4;
            public static readonly int unknownValue1 = 24;
            public static readonly int EXP = 28;
            public static readonly int P2PID = 32;
            public static readonly int SomeID = 36;
            public static readonly int ItemID = 68;
            public static readonly int unknownValue2 = 1515;
            public static readonly int unknownValue3 = 1516;
            public static readonly int unknownValue4 = 1538;
            public static readonly int unknownValue5 = 1539;
            public static readonly int unknownValue6 = 1543;
            public static readonly int unknownValue7 = 1547;
            public static readonly int unknownValue8 = 1551;
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

            byte[] buffer = new byte[unknownValue1 * 1594 + 8 + 500];
            Size = (ushort)buffer.Length;

            int offset = 0;
            var i = 8;
            var j = 0;
            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(unknownValue1).CopyTo(buffer, PropertyOffsets.unknownValue1);

            foreach (var player in Players) {

                BitConverter.GetBytes(player.UserID).CopyTo(buffer, PropertyPlayerOffsets.UserID+i);
                Encoding.ASCII.GetBytes(player.IGN).CopyTo(buffer, PropertyPlayerOffsets.IGN+i);
                BitConverter.GetBytes(player.unknownValue1).CopyTo(buffer, PropertyPlayerOffsets.unknownValue1+i);
                BitConverter.GetBytes(player.EXP).CopyTo(buffer, PropertyPlayerOffsets.EXP+i);
                BitConverter.GetBytes(player.P2PID).CopyTo(buffer, PropertyPlayerOffsets.P2PID+i);
                Encoding.ASCII.GetBytes(player.SomeID).CopyTo(buffer, PropertyPlayerOffsets.SomeID+i);
                j = 0;
                foreach(var item in player.ItemID) {
                    BitConverter.GetBytes(item).CopyTo(buffer, PropertyPlayerOffsets.ItemID+j+i);
                    j += 4;
                }

                BitConverter.GetBytes(player.unknownValue2).CopyTo(buffer, PropertyPlayerOffsets.unknownValue2+i);
                BitConverter.GetBytes(player.unknownValue3).CopyTo(buffer, PropertyPlayerOffsets.unknownValue3+i);
                BitConverter.GetBytes(player.unknownValue4).CopyTo(buffer, PropertyPlayerOffsets.unknownValue4+i);
                BitConverter.GetBytes(player.unknownValue5).CopyTo(buffer, PropertyPlayerOffsets.unknownValue5+i);
                BitConverter.GetBytes(player.unknownValue6).CopyTo(buffer, PropertyPlayerOffsets.unknownValue6+i);
                BitConverter.GetBytes(player.unknownValue7).CopyTo(buffer, PropertyPlayerOffsets.unknownValue7+i);
                BitConverter.GetBytes(player.unknownValue8).CopyTo(buffer, PropertyPlayerOffsets.unknownValue8+i);
                Encoding.ASCII.GetBytes(player.Country).CopyTo(buffer, PropertyPlayerOffsets.Country+i);
                Encoding.ASCII.GetBytes(player.Region).CopyTo(buffer, PropertyPlayerOffsets.Region+i);
                BitConverter.GetBytes(0).CopyTo(buffer, PropertyPlayerOffsets.End+i);
                i += 1595;

            }
            
            return buffer;
        }
        
    }

    public class UserReadyReq : HeaderPacket
    {
        public int unknownValue1 {get; set;}

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
        public string IGN {get; set;}
        public int unknownValue1 {get; set;}
        public int unknownValue2 {get; set;}

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
        public int unknownValue1 {get; set;}
        public int unknownValue2 {get; set;}
        public int unknownValue3 {get; set;}
        public int unknownValue4 {get; set;}


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
        public int unknownValue1 {get; set;}
        public int unknownValue2 {get; set;}
        public int unknownValue3 {get; set;}
        public int unknownValue4 {get; set;}
        public int unknownValue5 {get; set;}
        public int unknownValue6 {get; set;}


        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            public static readonly int unknownValue2 = 7;
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
            BitConverter.GetBytes(unknownValue2).CopyTo(buffer, PropertyOffsets.unknownValue2);

            BitConverter.GetBytes(unknownValue3).CopyTo(buffer, PropertyOffsets.unknownValue3);
            BitConverter.GetBytes(unknownValue5).CopyTo(buffer, PropertyOffsets.unknownValue5);

            return buffer;

        }


    }

    public class LoadGameReq : HeaderPacket
    {
        public byte[] unknownValue1 {get; set;}


        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;

        }

        public new static LoadGameReq Deserialize(byte[] data)
        {

            LoadGameReq packet = new LoadGameReq();

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            Console.WriteLine($"PACKET LENGTH: {packet.Size}");
            packet.unknownValue1 = new byte[packet.Size - PropertyOffsets.unknownValue1];
            Array.Copy(data, PropertyOffsets.unknownValue1, packet.unknownValue1, 0, packet.Size - PropertyOffsets.unknownValue1);


            return packet;
        }

    }

    public class LoadGameRes : HeaderPacket
    {

        public string IGN {get; set;}
        public byte[] unknownValue1 {get; set;}

        public struct PropertyOffsets
        {
            public static readonly int IGN = 6;
            public static readonly int unknownValue1 = 26;


        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[unknownValue1.Length + PropertyOffsets.unknownValue1];
            Size = (ushort)buffer.Length;

            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);

            Encoding.ASCII.GetBytes(IGN).CopyTo(buffer, PropertyOffsets.IGN);
            unknownValue1.CopyTo(buffer, PropertyOffsets.unknownValue1);

            return buffer;

        }

    }

    public class ChangeMapReq : HeaderPacket
    {
        public int unknownValue1 {get; set;}
        public int unknownValue2 {get; set;}
        public int unknownValue3 {get; set;}
        public int unknownValue4 {get; set;}
        public int unknownValue5 {get; set;}
        public int Map1 {get; set;}
        public int unknownValue7 {get; set;}
        public int Map2 {get; set;} 
        public int unknownValue8 {get; set;}
        public int Map3 {get; set;} 

        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            public static readonly int unknownValue2 = 8;
            public static readonly int unknownValue3 = 11;
            public static readonly int unknownValue4 = 12;
            public static readonly int unknownValue5 = 13;
            public static readonly int Map1 = 15;
            public static readonly int unknownValue7 = 17;
            public static readonly int Map2 = 19;
            public static readonly int unknownValue8 = 21;
            public static readonly int Map3 = 23;

        }

        public new static ChangeMapReq Deserialize(byte[] data)
        {
            ChangeMapReq packet = new ChangeMapReq();

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.unknownValue1 = Convert.ToInt32(data[PropertyOffsets.unknownValue1]);
            packet.unknownValue2 = Convert.ToInt32(data[PropertyOffsets.unknownValue2]);
            packet.unknownValue3 = Convert.ToInt32(data[PropertyOffsets.unknownValue3]);
            packet.unknownValue4 = Convert.ToInt32(data[PropertyOffsets.unknownValue4]);
            packet.unknownValue5 = Convert.ToInt32(data[PropertyOffsets.unknownValue5]);
            packet.Map1 = Convert.ToInt32(data[PropertyOffsets.Map1]);
            packet.unknownValue7 = Convert.ToInt32(data[PropertyOffsets.unknownValue7]);
            packet.Map2 = Convert.ToInt32(data[PropertyOffsets.Map2]);
            packet.unknownValue8 = Convert.ToInt32(data[PropertyOffsets.unknownValue8]);
            packet.Map3 = Convert.ToInt32(data[PropertyOffsets.Map3]);

            return packet;
        }

    }

    public class ChangeMapRes : HeaderPacket
    {
        public int unknownValue1 {get; set;}
        public int unknownValue2 {get; set;}
        public int unknownValue3 {get; set;}
        public int unknownValue4 {get; set;}
        public int unknownValue5 {get; set;}
        public int Map1 {get; set;} 
        public int unknownValue7 {get; set;}
        public int Map2 {get; set;} 
        public int unknownValue8 {get; set;}
        public int Map3 {get; set;} 

        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            public static readonly int unknownValue2 = 8;
            public static readonly int unknownValue3 = 11;
            public static readonly int unknownValue4 = 12;
            public static readonly int unknownValue5 = 13;
            public static readonly int Map1 = 15;
            public static readonly int unknownValue7 = 17;
            public static readonly int Map2 = 19;
            public static readonly int unknownValue8 = 21;
            public static readonly int Map3 = 23;
        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[25];
            Size = (ushort)(buffer.Length);

            int offset = 0;

            base.Serialize().CopyTo(buffer, offset);
          
            BitConverter.GetBytes(unknownValue1).CopyTo(buffer, PropertyOffsets.unknownValue1);
            BitConverter.GetBytes(unknownValue2).CopyTo(buffer, PropertyOffsets.unknownValue2);
            BitConverter.GetBytes(unknownValue3).CopyTo(buffer, PropertyOffsets.unknownValue3);
            BitConverter.GetBytes(unknownValue4).CopyTo(buffer, PropertyOffsets.unknownValue4);
            BitConverter.GetBytes(unknownValue5).CopyTo(buffer, PropertyOffsets.unknownValue5);
            BitConverter.GetBytes(Map1).CopyTo(buffer, PropertyOffsets.Map1);
            BitConverter.GetBytes(unknownValue7).CopyTo(buffer, PropertyOffsets.unknownValue7);
            BitConverter.GetBytes(Map2).CopyTo(buffer, PropertyOffsets.Map2);
            BitConverter.GetBytes(unknownValue8).CopyTo(buffer, PropertyOffsets.unknownValue8);
            BitConverter.GetBytes(Map3).CopyTo(buffer, PropertyOffsets.Map3);

            return buffer;
        }


    }

}