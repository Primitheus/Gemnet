using Gemnet.Network.Header;
using MySqlX.XDevAPI.Common;
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

    public class JoinRoomReq : HeaderPacket
    {
        public int UnknownValue1 {get; set;} // goes into UnknownValue1
        public int UnknownValue2 {get; set;} // goes into UnknownValue2
        public int UnknownValue3 {get; set;} // goes into UnknownValue3
        public int UnknownValue4 {get; set;} // goes into UnknownValue4
        public int UnknownValue5 {get; set;} // goes into UnknownValue5
        public int UnknownValue6 {get; set;} // goes into UnknownValue6
        public int UnknownValue7{get; set;} // goes into UnknownValue7
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
            public static readonly int UnknownValue3 = 12;
            public static readonly int UnknownValue4 = 14;
            public static readonly int UnknownValue5 = 18;
            public static readonly int UnknownValue6 = 20;
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

    public class GetPlayersRes : HeaderPacket
    {
        public int unknownValue1 {get; set;}
        public int unknownValue2 {get; set;}
        public int unknownValue3 {get; set;}
        public int unknownValue4 {get; set;}
        public int unknownValue5 {get; set;}
        public int unknownValue6 {get; set;}
        public string UserIGN {get; set;}
        public int unknownValue7 {get; set;}
        public string SomeID {get; set;}
        public int[] ItemID { get; set; }
 
        
        private struct PropertyOffsets
        {
            public static readonly int unknownValue1 = 6;
            public static readonly int unknownValue2 = 8;
            public static readonly int unknownValue3 = 12;
            public static readonly int unknownValue4 = 14;
            public static readonly int unknownValue5 = 16;
            public static readonly int unknownValue6 = 18;
            public static readonly int UserIGN = 26;
            public static readonly int unknownValue7 = 48;
            public static readonly int SomeID = 52;
            public static readonly int ItemID = 84;

        }

        public override byte[] Serialize()
        {

            byte[] buffer = new byte[2880];
            Size = (ushort)buffer.Length;

            int offset = 0;
            base.Serialize().CopyTo(buffer, offset);

            BitConverter.GetBytes(unknownValue1).CopyTo(buffer, PropertyOffsets.unknownValue1);
            BitConverter.GetBytes(unknownValue2).CopyTo(buffer, PropertyOffsets.unknownValue2);
            BitConverter.GetBytes(unknownValue3).CopyTo(buffer, PropertyOffsets.unknownValue3);
            BitConverter.GetBytes(unknownValue4).CopyTo(buffer, PropertyOffsets.unknownValue4);
            BitConverter.GetBytes(unknownValue5).CopyTo(buffer, PropertyOffsets.unknownValue5);
            BitConverter.GetBytes(unknownValue6).CopyTo(buffer, PropertyOffsets.unknownValue6);
            Encoding.ASCII.GetBytes(UserIGN).CopyTo(buffer, PropertyOffsets.UserIGN);
            BitConverter.GetBytes(unknownValue7).CopyTo(buffer, PropertyOffsets.unknownValue7);
            Encoding.ASCII.GetBytes(SomeID).CopyTo(buffer, PropertyOffsets.SomeID);

            var i = 0;
            foreach (var item in ItemID) 
            {

                BitConverter.GetBytes(item).CopyTo(buffer, PropertyOffsets.ItemID+i);
                i += 4;

            }

            

            return buffer;
        }
    }

}
