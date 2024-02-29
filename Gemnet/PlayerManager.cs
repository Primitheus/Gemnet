
using SendPacket;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using Gemnet.Packets;
using System.Collections.Concurrent;
using static Gemnet.Packets.Enums.Packets;

public class PlayerManager {


    public class Player {
        public string UserIGN { get; set; }
        public int UserID { get; set; }
        public int CurrentAvatar { get; set; }
        public int Carats { get; set; }
        public int EXP { get; set; }
        public string GUID {get; set;}
        public string Token {get; set;}
        public string ForumName {get; set;}
        public string Country { get; set; }
        public string Region { get; set; }
    }

    public static Dictionary<NetworkStream, Player> Players = new Dictionary<NetworkStream, Player>();

    public string GetPlayerIGN(NetworkStream stream) 
    {
        if (Players.ContainsKey(stream))
        {
            return Players[stream].UserIGN;
        } else {
            return null;
        }
    }

    public int GetPlayerUserID(NetworkStream stream) 
    {
        if (Players.ContainsKey(stream))
        {
            return Players[stream].UserID;
        } else {
            return 0;
        }
    }

    public int GetPlayerCurrentAvatar(NetworkStream stream) 
    {
        if (Players.ContainsKey(stream))
        {
            return Players[stream].CurrentAvatar;
        } else {
            return 0;
        }
    }

    public void UpdatePlayerCurrentAvatar(NetworkStream stream, int newAvatar)
    {
        if (Players.ContainsKey(stream))
        {
            Players[stream].CurrentAvatar = newAvatar;
        }
        else
        {
            Console.WriteLine("Player not found.");
        }
    }



}