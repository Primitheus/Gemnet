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

            packet.UserIGN = Encoding.ASCII.GetString(data, offset, 20);
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



    public class MatchRewardReq : HeaderPacket
    {
        public int MatchID { get; set; }
        public int Unknown1 { get; set; }
        public byte UknownB1 { get; set; }
        public byte UknownB2 { get; set; }
        public byte UknownB3 { get; set; }
        public byte NumberOfPlayers { get; set; } // Guess 
        public List<PlayerStats> Players { get; set; } = new List<PlayerStats>();
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
                var stats = Encoding.ASCII.GetString(data, PropertyOffsets.AdditionalStats, 65).TrimEnd('\0');

                PlayerStats player = new PlayerStats
                {
                    SlotID = BitConverter.ToUInt16(data, PropertyPlayerOffsets.SlotID + index + PropertyOffsets.PlayersStart),
                    PlayerIGN = Encoding.ASCII.GetString(data, PropertyPlayerOffsets.PlayerIGN + index + PropertyOffsets.PlayersStart, 20).TrimEnd('\0'),
                    LeaderboardPos = 0, // 0 = First Place, 7 = I assume Eighth Place
                    Kills = BitConverter.ToUInt16(data, PropertyPlayerOffsets.Kills + index + PropertyOffsets.PlayersStart),
                    Deaths = BitConverter.ToUInt16(data, PropertyPlayerOffsets.Deaths + index + PropertyOffsets.PlayersStart),
                    EXP = BitConverter.ToInt32(data, PropertyPlayerOffsets.EXP + index + PropertyOffsets.PlayersStart),
                    Score = BitConverter.ToInt32(data, PropertyPlayerOffsets.EXP + index + PropertyOffsets.PlayersStart),
                    Carats = BitConverter.ToInt32(data, PropertyPlayerOffsets.Carats + index + PropertyOffsets.PlayersStart),
                    Stats = AdditionalStats.Deserialize(stats)
                };
                index += 105; // Each player data is 60 bytes long
                packet.Players.Add(player);
            }


            return packet;
        }


    }

    public class MatchRewardRes : HeaderPacket
    {

        public int NewCarats { get; set; }
        public int NewExp { get; set; }
        public byte[] ServerTime { get; set; }
        public byte NumberOfPlayers { get; set; } // Guess 
        public List<PlayerStats> Players { get; set; } = new List<PlayerStats>();

        // Add other properties as needed
        public class PropertyOffsets
        {
            public static readonly int NewCarats = 6;
            public static readonly int NewExp = 14;
            public static readonly int ServerTime = 22;
            public static readonly int NumberOfPlayers = 54;
            public static readonly int PlayersStart = 55; // Start of players data
            public static readonly int AdditionalStats = 853;
        }

        public class PropertyPlayerOffsets
        {
            public static readonly int SlotID = 0;
            public static readonly int PlayerIGN = 2;
            public static readonly int LeaderboardPos = 22;
            public static readonly int Kills = 26;
            public static readonly int Deaths = 28;
            public static readonly int Score = 30; // Same as EXP?
            public static readonly int NNNNNNNNNNN = 37;
            public static readonly int CaratsReward = 47;
            public static readonly int EXPReward = 55;

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

            if (ServerTime == null || ServerTime.Length != 8)
            {
                // Get the current UTC time
                DateTime utcNow = DateTime.UtcNow;

                // Convert it to PST
                TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                DateTime pstNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, pstZone);


                // Get the current time in milliseconds since Unix epoch
                long unixTime = ((DateTimeOffset)pstNow).ToUnixTimeMilliseconds();
                long windows64Timestamp = (unixTime + 11644473600000) * 10000;
                byte[] timeArray = BitConverter.GetBytes(windows64Timestamp);

                ServerTime = timeArray;
                ServerTime = [0x00, 0x00, 0x00, 0x00, 0x00, 0x0, 0x00, 0x00];
                ServerTime.CopyTo(buffer, PropertyOffsets.ServerTime);
            }
            else
            {
                ServerTime.CopyTo(buffer, PropertyOffsets.ServerTime);

            }

            BitConverter.GetBytes(NewCarats).CopyTo(buffer, PropertyOffsets.NewCarats);
            BitConverter.GetBytes(NewExp).CopyTo(buffer, PropertyOffsets.NewExp);
            

            foreach (var player in Players)
            {
                BitConverter.GetBytes(player.SlotID).CopyTo(buffer, PropertyOffsets.PlayersStart + i + PropertyPlayerOffsets.SlotID);
                Encoding.ASCII.GetBytes(player.PlayerIGN.PadRight(20, '\0')).CopyTo(buffer, PropertyOffsets.PlayersStart + i + PropertyPlayerOffsets.PlayerIGN);
                buffer[PropertyOffsets.PlayersStart + i + PropertyPlayerOffsets.LeaderboardPos] = player.LeaderboardPos;
                BitConverter.GetBytes(player.Kills).CopyTo(buffer, PropertyOffsets.PlayersStart + i + PropertyPlayerOffsets.Kills);
                BitConverter.GetBytes(player.Deaths).CopyTo(buffer, PropertyOffsets.PlayersStart + i + PropertyPlayerOffsets.Deaths);
                BitConverter.GetBytes(player.Score).CopyTo(buffer, PropertyOffsets.PlayersStart + i + PropertyPlayerOffsets.Score); // Seems to be the score but it's the same as base earnt EXP anyway (at least for Battle mode)  
                Encoding.ASCII.GetBytes(player.NNNNNNNNNNN.PadRight(10, '\0')).CopyTo(buffer, PropertyOffsets.PlayersStart + i + PropertyPlayerOffsets.NNNNNNNNNNN);
                BitConverter.GetBytes(player.EXP).CopyTo(buffer, PropertyOffsets.PlayersStart + i + PropertyPlayerOffsets.EXPReward);
                BitConverter.GetBytes(player.Carats).CopyTo(buffer, PropertyOffsets.PlayersStart + i + PropertyPlayerOffsets.CaratsReward);
                Encoding.ASCII.GetBytes(player.Stats.Serialize().PadRight(65, '\0')).CopyTo(buffer, PropertyOffsets.AdditionalStats);

                i += 124; // Each player data is 124 bytes long
            }

            //BitConverter.GetBytes(AdditionalStats).CopyTo(buffer, PropertyOffsets.AdditionalStats + i + 550);




            return buffer;
        }
    }



}

