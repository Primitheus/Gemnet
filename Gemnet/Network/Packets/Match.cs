using Gemnet.Network.Header;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network.Packets
{
    public class MatchReq : HeaderPacket
    {
        public string UserIGN { get; set; }

        public new static MatchReq Deserialize(byte[] data)
        {
            MatchReq packet = new MatchReq();

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

    public class MatchRes : HeaderPacket
    {
        public int UserID { get; set; }
        public string UserIGN { get; set; }
        public int[] ItemID { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }


        private struct PropertyOffsets
        {

            public static readonly int UserID = 6;
            public static readonly int UserIGN = 10;
            public static readonly int ItemID = 38;
            public static readonly int Country = 1527;
            public static readonly int Region = 1535;

        }

        public override byte[] Serialize()
        {
            Size = (ushort)(1567);
            byte[] buffer = new byte[Size];
            int offset = 0;
            var i = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            BitConverter.GetBytes(UserID).CopyTo(buffer, PropertyOffsets.UserID);
            Encoding.ASCII.GetBytes(UserIGN).CopyTo(buffer, PropertyOffsets.UserIGN);

            foreach (var item in ItemID)
            {
                BitConverter.GetBytes(item).CopyTo(buffer, PropertyOffsets.ItemID + i);
                i += 4;
            }

            Encoding.ASCII.GetBytes(Country).CopyTo(buffer, PropertyOffsets.Country);
            Encoding.ASCII.GetBytes(Region).CopyTo(buffer, PropertyOffsets.Region);


            return buffer;
        }
    }

    public class MatchRewardPlayer
    {
        public ushort SlotID { get; set; }
        public string PlayerIGN { get; set; }
        public ushort Kills { get; set; }
        public ushort Deaths { get; set; }
        public string NNNNNNNNNNN { get; } = "NNNNNNNNNN"; // 10 N's
        public int EXP { get; set; }
        public int Carats { get; set; }

        public string AdditionalStats { get; set; }

        // Add other properties as needed
    }

    public class MatchRewardReq : HeaderPacket
    {
        public int MatchID { get; set; }
        public int Unknown1 { get; set; }
        public byte UknownB1 { get; set; }
        public byte UknownB2 { get; set; }
        public byte UknownB3 { get; set; }
        public byte NumberOfPlayers { get; set; } // Guess 
        public List<MatchRewardPlayer> Players { get; set; } = new List<MatchRewardPlayer>();
        public string AdditionalStats { get; set; } // Assuming this is a string of fixed length

        public class PropertyOffsets
        {
            public static readonly int MatchID = 6;
            public static readonly int Unknown1 = 11;
            public static readonly int UknownB1 = 32;
            public static readonly int UknownB2 = 34;
            public static readonly int UknownB3 = 36;
            public static readonly int NumberOfPlayers = 50;
            public static readonly int PlayersStart = 51; // Start of players data
            public static readonly int AdditionalStats = 896;
        }

        public class PropertyPlayerOffsets
        {
            public static readonly int SlotID = 0;
            public static readonly int PlayerIGN = 2;
            public static readonly int Kills = 27;
            public static readonly int Deaths = 29;
            public static readonly int NNNNNNNNNNN = 31;
            public static readonly int EXP = 41;
            public static readonly int Carats = 48;

        }


        public new static MatchRewardReq Deserialize(byte[] data)
        {
            MatchRewardReq packet = new MatchRewardReq();

            int offset = 6;

            packet.Type = ToUInt16BigEndian(data, 0);
            packet.Size = ToUInt16BigEndian(data, 2);
            packet.Action = BitConverter.ToUInt16(data, 4);

            packet.MatchID = BitConverter.ToInt32(data, PropertyOffsets.MatchID);
            packet.Unknown1 = BitConverter.ToInt32(data, PropertyOffsets.Unknown1);

            packet.UknownB1 = data[PropertyOffsets.UknownB1];
            packet.UknownB2 = data[PropertyOffsets.UknownB2];
            packet.UknownB3 = data[PropertyOffsets.UknownB3];

            packet.NumberOfPlayers = data[PropertyOffsets.NumberOfPlayers];

            var index = 0;

            for (int i = 0; i < packet.NumberOfPlayers; i++)
            {
                MatchRewardPlayer player = new MatchRewardPlayer
                {
                    SlotID = BitConverter.ToUInt16(data, PropertyPlayerOffsets.SlotID + index),
                    PlayerIGN = Encoding.ASCII.GetString(data, PropertyPlayerOffsets.PlayerIGN + index, 25).TrimEnd('\0'),
                    Kills = BitConverter.ToUInt16(data, PropertyPlayerOffsets.Kills + index),
                    Deaths = BitConverter.ToUInt16(data, PropertyPlayerOffsets.Deaths + index),
                    EXP = BitConverter.ToInt32(data, PropertyPlayerOffsets.EXP + index),
                    Carats = BitConverter.ToInt32(data, PropertyPlayerOffsets.Carats + index),
                };
                index += 105; // Each player data is 60 bytes long
                packet.Players.Add(player);
            }

            packet.AdditionalStats = Encoding.ASCII.GetString(data, PropertyOffsets.AdditionalStats + index + 635, 20).TrimEnd('\0');


            return packet;
        }


    }

    public class MatchRewardRes : HeaderPacket
    {

        
        public byte NumberOfPlayers { get; set; } // Guess 
        public List<MatchRewardPlayer> Players { get; set; } = new List<MatchRewardPlayer>();
        public int AdditionalStats { get; set; } // Assuming this is a string of fixed length

        // Add other properties as needed
        public class PropertyOffsets
        {

            public static readonly int NumberOfPlayers = 54;
            public static readonly int PlayersStart = 55; // Start of players data
            public static readonly int AdditionalStats = 853;
        }
        

        public override byte[] Serialize()
        {
            Size = (ushort)(1047);
            byte[] buffer = new byte[Size];
            int offset = 0;
            var i = 0;

            base.Serialize().CopyTo(buffer, offset);
            offset += 6;

            buffer[PropertyOffsets.NumberOfPlayers] = Players.Count > 0 ? (byte)Players.Count : (byte)0;

            foreach (var player in Players)
            {
                BitConverter.GetBytes(player.SlotID).CopyTo(buffer, PropertyOffsets.PlayersStart + i);
                Encoding.ASCII.GetBytes(player.PlayerIGN.PadRight(25, '\0')).CopyTo(buffer, PropertyOffsets.PlayersStart + i + 2);
                BitConverter.GetBytes(player.Kills).CopyTo(buffer, PropertyOffsets.PlayersStart + i + 27);
                BitConverter.GetBytes(player.Deaths).CopyTo(buffer, PropertyOffsets.PlayersStart + i + 29);
                Encoding.ASCII.GetBytes(player.NNNNNNNNNNN.PadRight(10, '\0')).CopyTo(buffer, PropertyOffsets.PlayersStart + i + 31);
                BitConverter.GetBytes(player.EXP).CopyTo(buffer, PropertyOffsets.PlayersStart + i + 41);
                BitConverter.GetBytes(player.Carats).CopyTo(buffer, PropertyOffsets.PlayersStart + i + 48);

                i += 124; // Each player data is 124 bytes long
            }

            BitConverter.GetBytes(AdditionalStats).CopyTo(buffer, PropertyOffsets.AdditionalStats + i + 550);




            return buffer;
        }
    }



}

