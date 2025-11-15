
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

        // User Info
        public int UserID { get; set; }
        public string UserIGN { get; set; }
        public int CurrentAvatar { get; set; }
        public int Carats { get; set; }
        public int EXP { get; set; }
        public string GUID { get; set; }
        public string Token { get; set; }
        public string ForumName { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }

        // Guild Info
        public string GuildName { get; set; }

        // Room Tracking
        public ushort CurrentRoom { get; set; }
        public ushort SlotID { get; set; } // Slot ID in the room
        public bool Ready { get; set; }
        public byte Team { get; set; } // 0x00 = Blue, 0x01 = Red. 

        // Session
        public DateTime LoginTime { get; set; }
        public NetworkStream Stream { get; set; }
        public int P2PID { get; set; }


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

    
    // Online Player
    public Player GetPlayerById(int userId)
    {
        if (_playerIdToStream.TryGetValue(userId, out var stream))
        {
            return GetPlayerByStream(stream);
        }
        return null;
    }


    // Get player by IGN from the database.
    public Player GetPlayerByIGN(string ign)
    {
        var playerData = _database.Select<ModelAccount>(ModelAccount.QueryGetPlayerInfo, new { username = ign }).FirstOrDefault();

        if (playerData == null)
        {
            return null; // Player not found
        }

        return new Player
        {
            UserID = playerData.UUID,
            UserIGN = playerData.IGN,
            EXP = playerData.EXP,
            CurrentAvatar = playerData.CurrentAvatar,


        };
    }


    public List<int> GetItemsOfAvatar(int avatarId)
    {

        var avatarData = _database.Select<ModelAvatar>(ModelAvatar.QueryGetAvatarData, new { AID = avatarId });
        int[] serverIds = null;

        foreach (var item in avatarData)
        {
            serverIds = new int[]
            {


                item.Job,
                item.Hair,
                item.Forehead,
                item.Top,
                item.Bottom,
                item.Gloves,
                item.Shoes,
                item.Eyes,
                item.Nose,
                item.Mouth,
                item.Scroll,
                item.ExoA,
                item.ExoB,
                item.Null,
                item.Back,
                item.Neck,
                item.Ears,
                item.Glasses,
                item.Mask,
                item.Waist,
                item.Scroll_BU,
                item.Unknown_1,
                item.Unknown_2,
                item.Inventory_1,
                item.Inventory_2,
                item.Inventory_3,
                item.Unknown_3,
                item.Unknown_4,
                item.Unknown_5,
                item.Unknown_6,
                item.Unknown_7,
                item.Title,
                item.Merit,
                item.Avalon,
                item.Hair_BP,
                item.Top_BP,
                item.Bottom_BP,
                item.Gloves_BP,
                item.Shoes_BP,
                item.Back_BP,
                item.Neck_BP,
                item.Ears_BP,
                item.Glasses_BP,
                item.Mask_BP,
                item.Waist_BP,

            };
        }

        List<int> finalItemIds = new List<int>();

        foreach (var serverId in serverIds)
        {
            if (serverId == 0)
            {
                finalItemIds.Add(0);
                continue;
            }

            var itemData = _database.Select<ModelInventory>(ModelInventory.GetItemFromServerID, new { SID = serverId })
                .FirstOrDefault();

            finalItemIds.Add(itemData?.ItemID ?? 0);
        }


        return finalItemIds;
    }

    public int GetCarats(int userId)
    {
        var playerData = _database.Select<ModelAccount>(ModelAccount.QueryCashCarats, new { ID = userId }).FirstOrDefault();


        return playerData?.Carats ?? -1;
    }

    public int GetEXP(int userId)
    {
        var playerData = _database.Select<ModelAccount>(ModelAccount.QueryCashExp, new { ID = userId }).FirstOrDefault();

        return playerData?.EXP ?? -1;
    }


    public List<Player> GetAllOnlinePlayers()
    {
        return _activePlayers.Values.ToList();

    }

    public bool IsPlayerOnline(string UserIGN)
    {
        return _activePlayers.Values.Any(player => player.UserIGN == UserIGN);    

    }


}