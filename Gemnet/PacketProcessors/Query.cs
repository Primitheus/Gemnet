using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemnet.Network.Packets;
using static Gemnet.AvatarDef.Enums.AvatarDef;
using static Program;
using Newtonsoft.Json;
using System.Globalization;
using Org.BouncyCastle.Asn1.Ocsp;
using Gemnet.Persistence.Models;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Asn1.Cmp;
using Microsoft.VisualBasic;
using Gemnet.PacketProcessors.Extra;

namespace Gemnet.PacketProcessors
{
    internal class Query
    {


        private static PlayerManager _playerManager = ServerHolder._playerManager;
        private static GameManager _gameManager = ServerHolder._gameManager;


        public static void GetEquippedAvatar(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            try
            {
                action++;
                EquippedAvatarRes response = new EquippedAvatarRes();

                response.Type = type;
                response.Action = action;

                Console.WriteLine($"Get Equipped Avatar");

                var player = _playerManager.GetPlayerByStream(stream);

                if (player.CurrentAvatar == 0)
                {
                    Console.WriteLine("No Avatar Equipped, Equipping Default Avatar");
                    var AvatarQuery = ServerHolder.DatabaseInstance.Select<ModelAvatar>(ModelAvatar.QueryGetAvatarIDs, new
                    {
                        ID = player.UserID,
                    });

                    if (AvatarQuery != null && AvatarQuery.Any())
                    {
                        player.CurrentAvatar = AvatarQuery.First().AvatarID;
                    }
                    else
                    {
                        Console.WriteLine("No Avatar Found");
                        return;
                    }
                }

                response.AvatarID = player.CurrentAvatar;
                string hexOutput = string.Join(", ", response.Serialize().Select(b => $"0x{b:X2}"));

                Console.WriteLine(hexOutput);

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetEquippedAvatar: {ex.Message}");
            }
        }

        public static void ChangeAvatar(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            ChangeAvatarReq request = ChangeAvatarReq.Deserialize(body);
            ChangeAvatarRes response = new ChangeAvatarRes();
            response.Type = type;
            response.Action = action;

            var player = _playerManager.GetPlayerByStream(stream);

            Console.WriteLine($"Player {player.UserIGN} Change Avatar to: {request.AvatarID}");

            var Query = ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryUpdateAvatar, new
            {
                avatar = request.AvatarID,
                ID = player.UserID
            });

            player.CurrentAvatar = request.AvatarID;
            Console.WriteLine($"Changed Avatar: {player.CurrentAvatar}");


            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
        }

        public static void ClearAvatarSlot(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            ClearAvatarSlotReq request = ClearAvatarSlotReq.Deserialize(body);

            ClearAvatarSlotRes response = new ClearAvatarSlotRes();
            response.Type = type;
            response.Action = action;

            String SlotName = Enum.GetName(typeof(SlotName), request.Slot);

            Console.WriteLine($"Clear Avatar: {request.AvatarID} Slot: {SlotName}");

            var Query = ModelAvatar.GetQueryUpdateAvatar(SlotName);
            int ServerID = 0;
            var QueryUpdate = ServerHolder.DatabaseInstance.Select<ModelAvatar>(Query, new
            {
                ServerID,
                AID = request.AvatarID,
            });

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);


        }

        public static void UpdateAvatar(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            UpdateAvatarReq request = UpdateAvatarReq.Deserialize(body);
            UpdateAvatarRes response = new UpdateAvatarRes();

            response.Type = type;
            response.Action = action;

            String SlotName = Enum.GetName(typeof(SlotName), request.Slot);

            Console.WriteLine($"Update: Avatar {request.AvatarID}, {request.Slot}, {SlotName}, {request.ServerID}");

            var Query = ModelAvatar.GetQueryUpdateAvatar(SlotName);

            var QueryUpdate = ServerHolder.DatabaseInstance.Select<ModelAvatar>(Query, new
            {
                request.ServerID,
                AID = request.AvatarID,
            });


            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
        }

        public static void HandleCreateRoom(ushort type, ushort action, byte[] body, NetworkStream stream)
        {

            action++;
            CreateRoomReq request = CreateRoomReq.Deserialize(body);

            Console.WriteLine($"Create Room: Name='{request.RoomName}', ID='{request.GroupP2PID}', MatchType='{request.MatchType}', GameMode='{request.GameMode1}'");
            Console.WriteLine($"Max Players: {request.MaxPlayers}, Player Number: {request.PlayerNumber}, P2PID: {request.P2PID}, Round Number: {request.RoundNumber}");

            CreateRoomRes response = new CreateRoomRes();

            response.Type = 576;
            response.Action = action;

            var player = _playerManager.GetPlayerByStream(stream);
            player.SlotID = 7; // Default Slot ID for Room Master

            Server.clientUsernames.TryGetValue(stream, out string username);
            Console.WriteLine($"RoomMaster: {player.UserIGN}");

            var room = _gameManager.CreateRoom(
                stream,
                request.P2PID,
                request.GroupP2PID,
                request.RoomName,
                request.MaxPlayers,
                (byte)request.MatchType,
                (byte)request.RoundNumber,
                request.GameMode1,
                request.GameMode2,
                request.GameMode3

            );

            response.Unknown1 = 0;
            response.RoomID = room.RoomId;
            response.Unknown2 = 7;

            // string hexOutput = string.Join(", ", response.Serialize().Select(b => $"0x{b:X2}"));
            // Console.WriteLine($"Property Output: {hexOutput}");

            int[] itemID = { 1000175, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            player.CurrentRoom = room.RoomId;
            player.SlotID = 7;

            Player newPlayer = new Player
            {
                unknownValue1 = 7,
                PlayerLevel = 11,
                EXP = 1337,
                IGN = username,
                P2PID = request.P2PID,
                SomeID = request.GroupP2PID,
                ItemID = itemID,
                unknownValue4 = 78,
                unknownValue5 = 78,
                unknownValue6 = 232,
                unknownValue7 = 3,
                unknownValue8 = 0,
                unknownValue9 = 0,
                unknownValue10 = 0,
                Country = "US",
                Region = "NA",

            };

            //_ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
            byte[] data = { 0x02, 0x40, 0x00, 0x0F, 0x87, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x00, 0x07, 0x00, 0x00 };
            BitConverter.GetBytes(room.RoomId).CopyTo(data, 10);
            BitConverter.GetBytes((ushort)7).CopyTo(data, 12);

            string hexOutput = string.Join(", ", data.Select(b => $"0x{b:X2}"));
            Console.WriteLine($"Property Output: {hexOutput}");

            _ = ServerHolder.ServerInstance.SendPacket(data, stream);

        }

        public static void LeaveRoom(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            Console.WriteLine($"Leaving Room");
            LeaveRoomReq request = LeaveRoomReq.Deserialize(body);
            LeaveRoomRes response = new LeaveRoomRes();

            response.Type = 576;
            response.Action = action;

            response.Unknown = request.Unknown;
            response.RoomID = request.RoomID;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

            var result = _gameManager.LeaveRoom(stream, request.RoomID);
            if (result)
            {
                Console.WriteLine($"Successfully left room {request.RoomID}");
            }
            else
            {
                Console.WriteLine($"Failed to leave room {request.RoomID}");
            }

            

            _gameManager.LeaveRoom(stream, request.RoomID);
            UserUpdateRoom(stream, request.RoomID);
            
            //byte[] data = { 0x02, 0x40, 0x00, 0x0c, 0x83, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00 };

            //_ = ServerHolder.ServerInstance.SendPacket(data, stream);

        }

        public static void UserUpdateRoom(NetworkStream stream, ushort RoomID)
        {

            Console.WriteLine($"User Update Room for Room ID: {RoomID}");

            RoomUpdatePlayersRes response = new RoomUpdatePlayersRes();

            response.Type = 576;
            response.Action = 0x11;

            response.unknownValue1 = 0;

            var player = _playerManager.GetPlayerByStream(stream);
            var playersInRoom = _gameManager.GetPlayersInRoom(RoomID);
            var firstPlayer = playersInRoom.FirstOrDefault();

            if (player.SlotID == 7)
            {
                response.PlayerWhoLeft = player.UserIGN;
                response.NewRoomMaster = firstPlayer?.UserIGN ?? "Unknown";
                firstPlayer.SlotID = 7;

            }
            else
            {
                response.PlayerWhoLeft = player.UserIGN;
                response.NewRoomMaster = playersInRoom.FirstOrDefault(p => p.SlotID == 7)?.UserIGN ?? "Unknown";

            }

            player.CurrentRoom = 0;
            player.SlotID = 0;
            player.Ready = false;
            player.P2PID = 0;

            Console.WriteLine($"Player Who Left: {response.PlayerWhoLeft}");
            Console.WriteLine($"New Room Master: {response.NewRoomMaster}");

            string hexOutput = string.Join(", ", response.Serialize().Select(b => $"0x{b:X2}"));
            Console.WriteLine($"Output: {hexOutput}");

            RoomUpdatePlayersRes response2 = new RoomUpdatePlayersRes();
            response2.Type = 576;
            response2.Action = 0x17;

            response2.unknownValue1 = 1;
            response2.PlayerWhoLeft = response.NewRoomMaster;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream, true);
            _ = ServerHolder.ServerInstance.SendPacket(response2.Serialize(), stream, true);
            

        }

        public class RoomInfo
        {
            public int unknownValue1 { get; set; }
            public int unknownValue2 { get; set; }
            public int unknownValue3 { get; set; }
            public int unknownValue4 { get; set; }
            public string RoomMasterIGN { get; set; }
            public int P2PID { get; set; }
            public string SomeID { get; set; }
            public string RoomName { get; set; }
            public int unknownValue6 { get; set; }
            public int MaxPlayers { get; set; }
            public int PlayerNumber { get; set; }
            public int GameState { get; set; }
            public int unknownValue7 { get; set; }
            public int MatchType { get; set; }
            public int unknownValue8 { get; set; }
            public int unknownValue9 { get; set; }
            public int unknownValue10 { get; set; }
            public int RoundNumber { get; set; }
            public int GameMode1 { get; set; }
            public int GameMode2 { get; set; }
            public int GameMode3 { get; set; }
            public byte[] Time { get; set; }
            public string Country { get; set; }
            public string Region { get; set; }
        }

        public class PlayerInfo
        {
            public int unknownValue1 { get; set; }
            public int unknownValue2 { get; set; }
            public int EXP { get; set; }
            public string IGN { get; set; }
            public int P2PID { get; set; }
            public string SomeID { get; set; }
            public int[] ItemID { get; set; }
            public int unknownValue4 { get; set; }
            public int unknownValue5 { get; set; }
            public int unknownValue6 { get; set; }
            public int unknownValue7 { get; set; }
            public int unknownValue8 { get; set; }
            public int unknownValue9 { get; set; }
            public int unknownValue10 { get; set; }
            public string Country { get; set; }
            public string Region { get; set; }

        }


        public static void GetRoomList(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            GetRoomListReq request = GetRoomListReq.Deserialize(body);

            byte[] someData = { 0xb8, 0xfd, 0x7f, 0x02, 0xd8, 0x8c, 0x0d, 0x01 };

            if (request.ChannelID == 10 || request.ChannelID == 1)
            {
                Console.WriteLine($"[CHANNEL] FREE");
                GetRoomListRes response = new GetRoomListRes();

                response.Type = type;
                response.Action = action;

                /* 
                
                This room comes out as a "FFA Classic Battle" and is password protected.
                It'll take some RE and more packet captures to figure out which values control what property.

                Some properties that are definitely included here are:

                    - GameMode
                    - Password Protected
                    - InGame/Waiting
                    - Single/Team

                Some properties that might or might not be inlcuded are:

                    - Room ID. (ProudNet)
                    - Who's currently inside the room (Not Likely).
                    - Map (most likely not here unless it's Boss Mode?)


                Note: The current unknown values are guesses where the property ends or begins.
                The guesses could very well be inccorect, for example two values could actually be 
                one value that controls one property. And one value could actually be two propeties that I mixed into one value.

                GameModes
                    - Classic Battle: 259
                    - Battle: 66305
                    - Hyper Battle: 66306
                    - Boss Mode: 33620225
                    - Moving Screen: 84082945
                    - King of The Kill: 67305729
                    - Potion Battle: 100860161
                    - Caged Beast: 117637377
                    - Arena: 50528513

                */

                string jsonFilePath = "rooms.json";
                string jsonData = File.ReadAllText(jsonFilePath);

                var rooms = _gameManager.GetAllRooms();

                foreach (var room in rooms)
                {
                    response.Rooms.Add(new RoomInfoA
                    {
                        unknownValue1 = 1,
                        unknownValue2 = 12,
                        RoomID = room.RoomId,
                        EXP = room.RMExp,
                        RoomMasterIGN = room.RMIGN,
                        P2PID = room.RMP2PID,
                        SomeID = room.GroupP2PID,
                        RoomName = room.RoomTitle,
                        isPassword = room.isPassword,
                        MaxPlayers = room.MaxPlayers,
                        PlayerNumber = room.CurrentPlayers,
                        GameState = room.GameState,
                        //unknownValue7 = room.unknownValue7,
                        MatchType = room.MatchType,
                        unknownValue8 = 0,
                        unknownValue9 = 1,
                        //unknownValue10 = room.unknownValue10,
                        RoundNumber = room.RoundNumber,
                        GameMode1 = room.GameMode1,
                        GameMode2 = room.GameMode2,
                        GameMode3 = room.GameMode3,
                        Time = someData,
                        Country = "US",
                        Region = "NA",
                    });

                }

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

            }

        }

        public static void JoinRoom(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            JoinRoomReq request = JoinRoomReq.Deserialize(body);

            Console.WriteLine($"Join Room: {request.RoomID} P2PID: {request.P2PID}, SomeID: {request.SomeID}");

            JoinRoomRes response = new JoinRoomRes();

            response.Type = 576;
            response.Action = action;

            response.UnknownValue1 = request.UnknownValue1;
            response.RoomID = request.RoomID;

            var room = _gameManager.GetRoom(request.SomeID);
            var player = _playerManager.GetPlayerByStream(stream);

            player.P2PID = request.P2PID;

            _gameManager.JoinRoom(stream, room.RoomId);

            response.SlotID = room.CurrentPlayers - 2;  // Slot ID 
            player.SlotID = (ushort)response.SlotID;

            response.UnknownValue5 = 0;
            response.UnknownValue6 = 0;

            response.RoomID = room.RoomId;
            response.RoomMaster = room.RMIGN;
            response.SomeID = room.GroupP2PID;
            response.UnknownValue7 = request.P2PID; // P2P ID
            response.RoomName = room.RoomTitle;
            response.isPassword = room.isPassword;
            response.MaxPlayers = room.MaxPlayers;
            response.PlayerNumber = room.CurrentPlayers;
            response.GameState = room.GameState;
            response.unknownValue7 = 1;
            response.MatchType = room.MatchType;
            response.unknownValue8 = 0;
            response.unknownValue9 = 1;
            response.RoundNumber = room.RoundNumber;
            response.GameMode1 = room.GameMode1;
            response.GameMode2 = room.GameMode2;
            response.GameMode3 = room.GameMode3;
            response.Country = "US";
            response.Region = "NA";


            string hexOutput = string.Join(", ", response.Serialize().Select(b => $"0x{b:X2}"));
            Console.WriteLine($"Output: {hexOutput}");

            UserJoined(type, 0x10, stream, room.RoomId, player);
            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

        }

        public static void JoinGetPlayers(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            Console.WriteLine("Get Players");

            GetPlayersReq request = GetPlayersReq.Deserialize(body);
            GetPlayersRes response = new GetPlayersRes();

            Console.WriteLine($"Get Players {request.unknownValue1}, {request.RoomID}");

            response.Action = action;
            response.Type = 576;

            response.unknownValue1 = request.unknownValue1;
            response.RoomID = request.RoomID;
            
            response.unknownValue3 = 7; // Maybe Master Slot ID?

            var roomId = request.RoomID;
            var room = _gameManager.GetRoom(roomId);
            var playerData = _gameManager.GetPlayersInRoom(roomId);

            response.PlayerNumber = (byte)playerData.Count;

            // Get the joining player (assuming you have their UserID or P2PID)
            var joiningPlayerId = _playerManager.GetPlayerByStream(stream)?.UserID ?? 0;
            var joiningPlayer = playerData.FirstOrDefault(p => p.UserID == joiningPlayerId);

            // Create response object if not exists
            if (response.Players == null)
            {
                response.Players = new List<Player>();
            }

            // Process joining player first if found
            if (joiningPlayer != null)
            {
                Console.WriteLine($"Processing joining player: {joiningPlayer.UserIGN}, P2PID: {joiningPlayer.P2PID}");
                

                List<int> finalItemIds = _playerManager.GetItemsOfAvatar(joiningPlayer.CurrentAvatar);

                response.Players.Add(new Player
                {
                    unknownValue1 = joiningPlayer.SlotID, // Slot ID Guess?
                    PlayerLevel = 1,
                    EXP = joiningPlayer.EXP,
                    IGN = joiningPlayer.UserIGN,
                    P2PID = joiningPlayer.P2PID,
                    SomeID = room.GroupP2PID,
                    ItemID = finalItemIds.ToArray(), // Use actual items or fallback to test data
                    unknownValue4 = 0x4e,
                    unknownValue5 = 0x4e,
                    unknownValue6 = 1000,
                    Country = "US",
                    Region = "NA"
                });
            }

            // Process other players
            foreach (var player in playerData.Where(p => p.UserID != joiningPlayerId))
            {
                
                List<int> finalItemIds = _playerManager.GetItemsOfAvatar(player.CurrentAvatar);

                response.Players.Add(new Player
                {
                    unknownValue1 = player.SlotID, // Splot ID Guess?
                    PlayerLevel = 11,
                    EXP = player.EXP,
                    IGN = player.UserIGN,
                    P2PID = player.P2PID,
                    SomeID = room.GroupP2PID,
                    ItemID = finalItemIds.ToArray(),
                    unknownValue4 = 0x4e,
                    unknownValue5 = 0x4e,
                    unknownValue6 = 1000,
                    Country = "US",
                    Region = "NA"
                });
            }


            Console.WriteLine("Send Player List Packet");
            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);


        }

        public static void UserJoined(ushort type, ushort action, NetworkStream stream, ushort roomId, PlayerManager.Player player)
        {
            Console.WriteLine("Notify User Joined Message");

            UserJoinedRes response = new UserJoinedRes();

            response.Action = action;
            response.Type = 576;

            var room = _gameManager.GetRoom(roomId);
            
            
            response.SlotID = player.SlotID;

            Console.WriteLine($"Getting Room Info for Room ID: {roomId}");

            List<int> finalItemIds = _playerManager.GetItemsOfAvatar(player.CurrentAvatar);

            response.Players.Add(new PlayerJoin
            {
                UserID = player.UserID,
                IGN = player.UserIGN,
                PlayerLevel = 1,
                EXP = player.EXP,
                P2PID = player.P2PID,
                SomeID = room.GroupP2PID,
                ItemID = finalItemIds.ToArray(),

                unknownValue2 = 0x4e,
                unknownValue3 = 0x4e,

                unknownValue4 = 1000,

                Country = "US",
                Region = "NA"
            });


            Console.WriteLine($"User Joined: {player.UserIGN}, Room ID: {roomId}, updating game man.");
            var result = _gameManager.JoinRoom(stream, roomId);
            Console.WriteLine($"User Joined Room: {result}");

            //byte[] data = { 0x02, 0x40, 0x00, 0x1b, 0x18, 0x00, 0x4E, 0x69, 0x6D, 0x6F, 0x6E, 0x69, 0x78, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            bool NOT = true;
            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream, NOT);

            //_ = ServerHolder.ServerInstance.SendPacket(data, stream, false);


        }

        public static void Chat(ushort type, ushort action, byte[] body, NetworkStream stream)
        {

            action = 0x12;
            type = 576;

            ChatReq request = ChatReq.Deserialize(body);
            Console.WriteLine($"Chat Message: {request.Message}");

            ChatRes response = new ChatRes();

            response.Type = type;
            response.Action = action;


            Server.clientUsernames.TryGetValue(stream, out string UserIGN);
            bool NOT = false;

            if (request.Message.StartsWith("/give"))
            {
                var arguements = request.Message.Split(' ');

                if (arguements.Length >= 3)
                {
                    var player = arguements[1];
                    var item = arguements[2];
                    var amount = arguements.Length > 3 ? int.Parse(arguements[3]) : 1; // Default amount is 1 if not specified

                    var QueryID = ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryGetIdFromUsername, new
                    {
                        username = player
                    });

                    if (QueryID != null && QueryID.Any())
                    {
                        if (item == "carats" && amount > 0)
                        {
                            ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryAddCarats, new
                            {
                                amount,
                                ID = QueryID.First().UUID,
                            });
                            response.Message = $"{UserIGN} Gave {player} {amount} Carats.";
                        }
                        else if (item == "astros" && amount > 0)
                        {
                            ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryAddAstros, new
                            {
                                amount,
                                ID = QueryID.First().UUID,
                            });
                            response.Message = $"{UserIGN} Gave {player} {amount} Astros.";
                        }
                        else
                        {
                            response.Message = $"Incorrect or Invalid Arguments.";
                        }
                    }
                }
                else
                {
                    response.Message = $"Incorrect or Invalid Arguments.";

                }
            }
            else
            {
                response.Message = request.Message;
            }

            if (request.Message.StartsWith("/announce", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = request.Message.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2)
                {
                    // No message was provided
                    Util.Announce("Usage: /announce [message]");
                }
                else
                {
                    string announcementMessage = parts[1].Trim();
                    Util.Announce(announcementMessage);
                }
            }


            response.UserIGN = UserIGN;
            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream, NOT);

        }

        public static void GetReward(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            RewardsReq request = RewardsReq.Deserialize(body);

            Console.WriteLine($"Rewards/Stats for {request.UserIGN}: Kills: {request.Kills}, EXP: {request.EXP}, CARATS: {request.Carat}.");

            RewardsRes response = new RewardsRes();

            response.Type = type;
            response.Action = action;

            response.UserIGN = request.UserIGN;
            response.EXP = request.EXP;
            response.CARATS = request.Carat;
            response.Kills = request.Kills;
            // static for now needs to be added into the DB then fetched.
            response.NewCarats = 1337;
            response.NewExp = 1337;
            response.NNNNNNNNNN = "NNNNNNNNNN";
            response.unknownvalue1 = request.unknown7;
            response.unknownvalue2 = 1;
            response.unknownvalue3 = request.unknown12;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

        }

        public static void GetMatchReward(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            
            action = 0x2a;

            MatchRewardReq request = MatchRewardReq.Deserialize(body);

            Console.WriteLine($"Match Reward for {request.MatchID}.");

            MatchRewardRes response = new MatchRewardRes();
            response.Type = type;
            response.Action = action;

            response.Players = request.Players;


            foreach (var player in request.Players)
            {
                Console.WriteLine($"Player: {player.PlayerIGN}, Kills: {player.Kills}, Deaths: {player.Deaths}, EXP: {player.EXP}, Carats: {player.Carats}, Additional Stats: {player.AdditionalStats}");
            }


            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

        }

        public static void UserReady(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action = 0x22;

            UserReadyReq request = UserReadyReq.Deserialize(body);
            Console.WriteLine($"Notify Message User Ready.");

            UserReadyRes response = new UserReadyRes();

            response.Action = action;
            response.Type = 576;

            response.unknownValue1 = request.unknownValue1;

            var player = _playerManager.GetPlayerByStream(stream);
            player.Ready = player.Ready ? false : true;

            response.unknownValue1 = player.Ready ? 1 : 0;
            response.unknownValue2 = request.unknownValue1;

            

            response.IGN = player.UserIGN;

            bool NOT = false;
            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream, NOT);

        }

        public static void StartMatch(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action = 0x23;

            Console.WriteLine("Start Match");
            StartMatchReq request = StartMatchReq.Deserialize(body);
            StartMatchRes response = new StartMatchRes();

            response.Action = action;
            response.Type = 576;
            Random random = new Random();

            int rand1 = random.Next(1, 1000);

            response.unknownValue1 = rand1; // match id

            response.unknownValue3 = request.unknownValue1;
            response.unknownValue5 = request.unknownValue3;

            byte[] data = { 0x02, 0x40, 0x00, 0x12, 0x23, 0x00, 0xdf, 0x45, 0x00, 0x00, 0xe9, 0x03, 0xee, 0x03, 0xf1, 0x03, 0xef, 0x03 };

            bool NOT = false;
            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream, NOT);

        }

        public static async Task LoadGame1(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action = 0x15;

            LoadGameReq request = LoadGameReq.Deserialize(body);

            LoadGameRes response = new LoadGameRes();

            response.Action = action;
            response.Type = 576;

            var player = _playerManager.GetPlayerByStream(stream);
            response.IGN = player.UserIGN;

            response.Data = request.Data;

            Console.WriteLine("Loading 1");
            //Task.Delay(10000);
            bool NOT = false;
            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream, NOT);

        }

        public static void LoadGame2(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action = 0x24;

            byte[] data = { 0x02, 0x40, 0x00, 0x0e, 0x24, 0x00, 0x07, 0x02, 0x01, 0x03, 0x04, 0x00, 0x06, 0x05 };

            Console.WriteLine("Loading 2");
            //Task.Delay(10000);
            _ = ServerHolder.ServerInstance.SendPacket(data, stream);

        }

        public static void ChangeMap(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action = 0x20;

            ChangeMapReq request = ChangeMapReq.Deserialize(body);
            ChangeMapRes response = new ChangeMapRes();

            response.Action = action;
            response.Type = 576;

            response.unknownValue1 = request.unknownValue1;
            response.unknownValue2 = request.unknownValue2;
            response.unknownValue3 = request.unknownValue3;
            response.unknownValue4 = request.unknownValue4;
            response.unknownValue5 = request.unknownValue5;
            response.Map1 = request.Map1;
            response.unknownValue7 = request.unknownValue7;
            response.unknownValue8 = request.unknownValue8;

            Console.WriteLine("Change Map");
            bool NOT = false;
            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream, NOT);

            byte[] data = { 0x02, 0x40, 0x00, 0x06, 0xa9, 0x00 };
            _ = ServerHolder.ServerInstance.SendPacket(data, stream, NOT);

        }

        public static void FIN(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            byte[] data = { 0x02, 0x40, 0x00, 0x06, 0xAB, 0x00 };

            _ = ServerHolder.ServerInstance.SendPacket(data, stream);

        

        }
    }
}
