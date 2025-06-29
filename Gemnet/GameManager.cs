using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using Gemnet.Network;
using Gemnet.Network.Packets;
using Microsoft.VisualBasic;


public enum GameState : byte
{
    Waiting = 0x57,
    InProgress = 0x50,
}



public class GameManager
{
    private readonly PlayerManager _playerManager;
    private readonly ConcurrentDictionary<int, GameRoom> _gameRooms = new();
    private int _nextRoomId = 1;

    public GameManager(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }

    public class GameRoom
    {
        public string Password { get; set; } = string.Empty;


        public ushort RoomId { get; set; }

        public int RMExp { get; set; }
        public string RMIGN { get; set; } = "DefaultIGN"; // Default value, can be changed
        public int RMP2PID { get; set; }
        public string GroupP2PID { get; set; } = "1337"; // Default value, can be changed
        public string RoomTitle { get; set; } = "Room Title";
        public byte isPassword => (byte)(Password.Length > 0 ? 0x4f : 0x4f); // 1 if password is set, 0 otherwise
        public byte MaxPlayers { get; set; }

        public byte CurrentPlayers => (byte)Players.Count;
        public byte GameState { get; set; } = 0x57;
        public byte MatchType { get; set; }
        public byte RoundNumber { get; set; }
        public int GameMode1 { get; set; }
        public int GameMode2 { get; set; }
        public int GameMode3 { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }

        public ConcurrentDictionary<int, PlayerManager.Player> Players { get; } = new();
        //public ConcurrentDictionary<int, PlayerManager.Player> Spectators { get; } = new();  


    }

    public GameRoom CreateRoom(
        NetworkStream creatorStream,
        int playerP2pId,
        string groupP2pId,
        string roomTitle,
        //string password,
        byte maxPlayters,
        byte matchType,
        byte roundNumber,
        int gm1,
        int gm2,
        int gm3)

    {
        var creator = _playerManager.GetPlayerByStream(creatorStream);
        creator.P2PID = playerP2pId;

        if (creator == null)
        {
            throw new InvalidOperationException("Player not found for the given stream");
        }

        ushort roomId = (ushort)Interlocked.Increment(ref _nextRoomId);

        var room = new GameRoom
        {

            //Password = password,

            RoomId = roomId,
            RMExp = creator.EXP,
            RMIGN = creator.UserIGN,
            RMP2PID = playerP2pId,
            GroupP2PID = groupP2pId,
            RoomTitle = roomTitle,
            MaxPlayers = maxPlayters,
            MatchType = matchType,
            RoundNumber = roundNumber,
            GameMode1 = gm1,
            GameMode2 = gm2,
            GameMode3 = gm3,
            Country = "US",
            Region = "NA"

        };

        if (!room.Players.TryAdd(creator.UserID, creator))
        {
            throw new InvalidOperationException("Failed to add creator to the room");

        }

        if (!_gameRooms.TryAdd(roomId, room))
        {
            throw new InvalidOperationException("Failed to create room (ID Conflict)");
        }

        Console.WriteLine($"Room Created: {roomId} by {creator.UserIGN} (P2PID: {playerP2pId}) Successfully");

        return room;


    }

    // Get All Rooms
    public List<GameRoom> GetAllRooms()
    {
        return _gameRooms.Values.ToList();
    }

    public List<GameRoom> GetAllRoomsByChannel(int ChannelID)
    {
        throw new NotImplementedException();
    }

    public GameRoom GetRoom(ushort roomId)
    {
        if (_gameRooms.TryGetValue(roomId, out var room))
        {
            return room;
        }
        return null; // or throw an exception if preferred
    }

    public GameRoom GetRoom(string groupP2pId)
    {
        return _gameRooms.Values.FirstOrDefault(r => r.GroupP2PID == groupP2pId);
    }


    // Remove a room by id.
    public bool RemoveRoom(ushort roomId)
    {
        return _gameRooms.TryRemove(roomId, out _);
    }



    public bool RemoveRoomAndPlayers(ushort roomId)
    {
        if (_gameRooms.TryRemove(roomId, out var room))
        {
            room.Players.Clear();
            return true;
        }
        return false;
    }


    // Join Room
    public bool JoinRoom(NetworkStream stream, ushort roomId)
    {
        var player = _playerManager.GetPlayerByStream(stream);
        if (player == null)
        {
            throw new InvalidOperationException("Failed To Find Player");
        }

        if (!_gameRooms.TryGetValue(roomId, out var room))
        {
            throw new InvalidOperationException("Room Not Found");
        }

        // if (room.isPassword == 0x43 && room.Password != password)
        // {
        //     throw new InvalidOperationException("Password is Incorrect.");
        // }

        if (room.Players.TryAdd(player.UserID, player))
        {
            return true;
        }

        return false;

    }

    public bool LeaveRoom(NetworkStream stream, ushort roomId)
    {
        var player = _playerManager.GetPlayerByStream(stream);
        if (player == null)
        {
            throw new InvalidOperationException("Failed To Find Player");
        }

        if (_gameRooms.TryGetValue(roomId, out var room))
        {
            if (room.Players.TryRemove(player.UserID, out _))
            {
                // If the room is empty after removing the player, remove the room itself
                if (room.Players.Count == 0)
                {
                    _gameRooms.TryRemove(roomId, out _);
                    Console.WriteLine($"Room {roomId} has been removed as it is now empty.");
                }
                return true;
            }
        }
        return false;
    }

    // Get Players
    public List<PlayerManager.Player> GetPlayersInRoom(ushort roomId)
    {
        if (_gameRooms.TryGetValue(roomId, out var room))
        {
            return room.Players.Values.ToList();
        }
        return new List<PlayerManager.Player>();
    }



}