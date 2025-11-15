using System;
using System.Collections.Generic;
using System.Linq;



namespace Gemnet.Network.Packets.Helpers
{
    public static class MatchResult
    {
        public static int CalculateCarats(int carats, int UserID)
        {
            int newCarats = 0;


            return newCarats;

        }

        public static int CalculateEXP(int exp)
        {
            int newEXP = 0;


            return newEXP;

        }

        public static List<PlayerStats> CalculateLeaderboardPositions(List<PlayerStats> players)
        {
            var sortedPlayers = players.OrderByDescending(p => p.Score).ToList();

            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                sortedPlayers[i].LeaderboardPos = (byte)i; // 0 for best, 1 for 2nd, etc.
            }

            return sortedPlayers;
        }


        public static int CalculateScore(int kills, int deaths, AdditionalStats stats)
        {
            int score;
            score = (kills * 10) - (deaths * 4) + (stats.ATK - stats.DMG);

            if (score < 0) score = 0;

            return score;

        }

        public static List<PlayerStats> CalculateMatchResult(List<PlayerStats> players)
        {

            foreach (var player in players)
            {


            }

            return players;

        }



    }


}
