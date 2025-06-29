
using SendPacket;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using Gemnet.Packets;
using System.Collections.Concurrent;
using static Gemnet.Packets.Enums.Packets;
using Gemnet.Persistence;
using Gemnet.Network;
using Gemnet.Persistence.Models;

public class PlayerManager
{

    private readonly Database _database;

    private readonly ConcurrentDictionary<NetworkStream, Player> _activePlayers = new();
    private readonly ConcurrentDictionary<int, NetworkStream> _playerIdToStream = new();



    public class Player
    {

        public int UserID { get; set; }
        public int P2PID { get; set; }
        public string UserIGN { get; set; }
        public int CurrentAvatar { get; set; }
        public int Carats { get; set; }
        public int EXP { get; set; }
        public string GUID { get; set; }
        public string Token { get; set; }
        public string ForumName { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }

        // unimplemented yet
        public string GuildName { get; set; }
        public ushort CurrentRoom { get; set; }
        public ushort SlotID { get; set; } // Slot ID in the room
        public bool Ready { get; set; }

        // Session
        public DateTime LoginTime { get; set; }
        public NetworkStream Stream { get; set; }


    }

    public PlayerManager(Database database)
    {
        _database = database;

    }

    public bool TryAddPlayer(NetworkStream stream, Player player)
    {
        player.Stream = stream;
        player.LoginTime = DateTime.UtcNow;

        if (_activePlayers.TryAdd(stream, player))
        {
            _playerIdToStream.TryAdd(player.UserID, stream);
            return true;
        }
        return false;

    }


    public bool TryRemovePlayer(NetworkStream stream)
    {
        if (_activePlayers.TryRemove(stream, out var player))
        {
            _playerIdToStream.TryRemove(player.UserID, out _);
            return true;
        }
        return false;
    }

    public Player GetPlayerByStream(NetworkStream stream)
    {
        _activePlayers.TryGetValue(stream, out var player);
        return player;
    }

    public Player GetPlayerById(int userId)
    {
        if (_playerIdToStream.TryGetValue(userId, out var stream))
        {
            return GetPlayerByStream(stream);
        }
        return null;
    }


}