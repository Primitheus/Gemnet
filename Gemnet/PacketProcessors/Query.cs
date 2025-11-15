using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemnet.Network.Packets;
using static Gemnet.AvatarDef.Enums.AvatarDef;
using static Gemnet.Program;
using Newtonsoft.Json;
using System.Globalization;
using Org.BouncyCastle.Asn1.Ocsp;
using Gemnet.Persistence.Models;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Asn1.Cmp;
using Microsoft.VisualBasic;
using Gemnet.PacketProcessors.Extra;
using Gemnet.Network.Packets.Helpers;
using System.Text.RegularExpressions;
using System.Net.Quic;

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
            Console.WriteLine($"RoomMaster: {player.UserIGN}");

            var room = _gameManager.CreateRoom(
                stream,
                request.P2PID,
                request.GroupP2PID,
                request.RoomName,
                request.MaxPlayers,
                (byte)request.MatchType,
                (byte)request.BattleType,
                (byte)request.RoundNumber,
                request.GameMode1,
                request.GameMode2,
                request.GameMode3

            );

            response.Unknown1 = 0;
            response.RoomID = room.RoomId;

            response.Unknown2 = 7;


            //_ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
            byte[] data = { 0x02, 0x40, 0x00, 0x0F, 0x87, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x00, 0x07, 0x00, 0x00 };
            BitConverter.GetBytes(room.RoomId).CopyTo(data, 10);
            BitConverter.GetBytes((ushort)7).CopyTo(data, 12);


            _ = ServerHolder.ServerInstance.SendPacket(data, stream);


        }

        public static void LeaveRoom(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            Console.WriteLine($"Leaving Room");
            LeaveRoomReq request = LeaveRoomReq.Deserialize(body);


            /////////// SHOULD PROBABLY MOVE THIS TO SOMEWHERE ELSE, NEEDS TO BE SENT AT THE VERY END.

            LeaveRoomRes response = new LeaveRoomRes();

            response.Type = 576;
            response.Action = action;

            response.Unknown = request.Unknown;
            response.RoomID = request.RoomID;

            string hexOutput = string.Join(", ", response.Serialize().Select(b => $"0x{b:X2}"));
            Console.WriteLine($"Test Leave Room: {hexOutput}");

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

            ////////////////////////////////////////////////////////////////////////////////////////////
            var player = _playerManager.GetPlayerByStream(stream);
            UserUpdateRoom(player, request.RoomID);

            var result = _gameManager.LeaveRoom(stream, request.RoomID);
            if (result)
            {
                Console.WriteLine($"Successfully left room {request.RoomID}");

            }
            else
            {
                Console.WriteLine($"Failed to leave room {request.RoomID}");
            }
            
            //byte[] data = { 0x02, 0x40, 0x00, 0x0c, 0x83, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00 };

            //_ = ServerHolder.ServerInstance.SendPacket(data, stream);

        }

        public static void UserUpdateRoom(PlayerManager.Player player, ushort RoomID)
        {

            Console.WriteLine($"User Update Room for Room ID: {RoomID}");

            RoomUpdatePlayersRes response = new RoomUpdatePlayersRes();

            response.Type = 576;
            response.Action = 0x11;

            var playersInRoom = _gameManager.GetPlayersInRoom(RoomID);

            var room = _gameManager.GetRoom(RoomID);

            response.PlayerWhoLeft = player.UserIGN;

            if (player.SlotID == room.RMSlotID)
            {
                Console.WriteLine($"Player Leaving was a Master!");
                var newMaster = _gameManager.HandleRoomMasterChange(room);

                if (newMaster != null)
                {
                    Console.WriteLine($"New Room Master: {newMaster.UserIGN}");

                    UpdateRoomMasterRes response2 = new UpdateRoomMasterRes();
                    response2.Type = 576;
                    response2.Action = 0x17;

                    response2.NewRoomMaster = newMaster.UserIGN;
                    response2.Unknown1 = 1;

                    _ = ServerHolder.ServerInstance.SendToRoom(response2.Serialize(), RoomID);

                }

            }


            Console.WriteLine($"Player Who Left: {response.PlayerWhoLeft}");

            _ = ServerHolder.ServerInstance.SendToRoom(response.Serialize(), RoomID);

        }

        public static void GetRoomList(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            GetRoomListReq request = GetRoomListReq.Deserialize(body);

            byte[] someDataOriginal = { 0xb8, 0xfd, 0x7f, 0x02, 0xd8, 0x8c, 0x0d, 0x01 };
            byte[] someDataTest = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            if (request.ChannelID == 10 || request.ChannelID == 1)
            {
                Console.WriteLine($"[CHANNEL] FREE");
                GetRoomListRes response = new GetRoomListRes();

                response.Type = type;
                response.Action = action;

                /* 
                
                This room comes out as a "FFA Classic Battle" and is password protected.
                It'll take some RE and more packet captures to figure out which values control what property.

                Some properties that might or might not be inlcuded are:

                    - Map?
                    
                Note: The current unknown values are guesses where the property ends or begins.
                The guesses could very well be inccorect, for example two values could actually be 
                one value that controls one property. And one value could actually be two propeties that I mixed into one value.

                GameModes
                    - Classic Battle: 
                    - Battle: 
                    - Hyper Battle: 
                    - Boss Mode: 
                    - Moving Screen: 
                    - King of The Kill: 
                    - Potion Battle: 
                    - Caged Beast: 
                    - Arena: 

                */

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
                        BattleType = room.BattleType,
                        RoundNumber = room.RoundNumber,
                        GameMode1 = room.GameMode1,
                        GameMode2 = room.GameMode2,
                        GameMode3 = room.GameMode3,
                        Time = someDataTest,
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

            response.SlotID = player.SlotID;

            response.UnknownValue5 = 0;
            response.UnknownValue6 = 0;

            response.RoomID = room.RoomId;
            response.RoomMaster = room.RMIGN;
            response.SomeID = room.GroupP2PID;
            response.UnknownValue7 = player.SlotID; // P2P ID
            response.RoomName = room.RoomTitle;
            response.isPassword = room.isPassword;
            response.MaxPlayers = room.MaxPlayers;
            response.PlayerNumber = room.CurrentPlayers;
            response.GameState = room.GameState;
            response.unknownValue7 = 1;
            response.MatchType = room.MatchType;
            response.unknownValue8 = 0;
            response.BattleType = room.BattleType;
            response.RoundNumber = room.RoundNumber;
            response.GameMode1 = room.GameMode1;
            response.GameMode2 = room.GameMode2;
            response.GameMode3 = room.GameMode3;

            response.Map1 = room.Map1;
            response.Map2 = room.Map2;
            response.Map3 = room.Map3;

            response.Country = "US";
            response.Region = "NA";


            // string hexOutput = string.Join(", ", response.Serialize().Select(b => $"0x{b:X2}"));
            // Console.WriteLine($"Output: {hexOutput}");

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


            var roomId = request.RoomID;
            var room = _gameManager.GetRoom(roomId);
            var playerData = _gameManager.GetPlayersInRoom(roomId);

            response.unknownValue3 = room.RMSlotID; // Maybe Master Slot ID?

            response.PlayerNumber = (byte)playerData.Count;

            // Get the joining player (assuming you have their UserID or P2PID)
            var joiningPlayer = _playerManager.GetPlayerByStream(stream);


            // Create response object if not exists
            if (response.Players == null)
            {
                response.Players = new List<Player>();
            }

            // Process joining player first if found
            if (joiningPlayer != null)
            {
                Console.WriteLine($"Processing joining player: {joiningPlayer.UserIGN}, P2PID: {joiningPlayer.P2PID}, SlotID: {joiningPlayer.SlotID}");


                List<int> finalItemIds = _playerManager.GetItemsOfAvatar(joiningPlayer.CurrentAvatar);

                response.Players.Add(new Player
                {
                    unknownValue1 = joiningPlayer.SlotID, // Slot ID Guess?
                    PlayerLevel = 1,
                    EXP = joiningPlayer.EXP,
                    IGN = joiningPlayer.UserIGN,
                    Team = joiningPlayer.Team,
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
            foreach (var player in playerData.Where(p => p.UserID != joiningPlayer.UserID))
            {

                List<int> finalItemIds = _playerManager.GetItemsOfAvatar(player.CurrentAvatar);

                response.Players.Add(new Player
                {
                    unknownValue1 = player.SlotID, // Splot ID Guess?
                    PlayerLevel = 11,
                    EXP = player.EXP,
                    IGN = player.UserIGN,
                    Team = player.Team,
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
                Team = player.Team,
                P2PID = player.P2PID,
                SomeID = room.GroupP2PID,
                ItemID = finalItemIds.ToArray(),

                unknownValue2 = 0x4e,
                unknownValue3 = 0x4e,

                unknownValue4 = 1000,

                Country = "US",
                Region = "NA"
            });


            Console.WriteLine($"User Joined: {player.UserIGN}, Room ID: {roomId}, SlotID: {player.SlotID}");


            //byte[] data = { 0x02, 0x40, 0x00, 0x1b, 0x18, 0x00, 0x4E, 0x69, 0x6D, 0x6F, 0x6E, 0x69, 0x78, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            SelectTeamRes response2 = new SelectTeamRes();
            response2.Type = 576;
            response2.Action = 0x18;

            response2.UserIGN = player.UserIGN;
            response2.Team = player.Team;

            bool EXCLUDE = true;
            _ = ServerHolder.ServerInstance.SendToRoomExcludeSender(response.Serialize(), roomId, stream);
            _ = ServerHolder.ServerInstance.SendToRoomExcludeSender(response2.Serialize(), roomId, stream);


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

            var player = _playerManager.GetPlayerByStream(stream);
        
            bool NOT = false;

            if (request.Message.StartsWith("/give"))
            {
                var arguements = request.Message.Split(' ');

                if (arguements.Length >= 3)
                {
                    var playerIgn = arguements[1];
                    var item = arguements[2];
                    var amount = arguements.Length > 3 ? int.Parse(arguements[3]) : 1; // Default amount is 1 if not specified

                    if (player.UserID != 0)
                    {
                        if (item == "carats" && amount > 0)
                        {
                            ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryAddCarats, new
                            {
                                amount,
                                ID = player.UserID
                            });
                            response.Message = $"{player.UserIGN} Gave {playerIgn} {amount} Carats.";
                        }
                        else if (item == "astros" && amount > 0)
                        {
                            ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryAddAstros, new
                            {
                                amount,
                                ID = player.UserID,
                            });
                            response.Message = $"{player.UserIGN} Gave {playerIgn} {amount} Astros.";
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

            if (request.Message.StartsWith("/promote", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = request.Message.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2)
                {
                    // No message was provided
                    Util.Announce("Usage: /promote [message]");
                }
                else
                {
                    string promoteMessage = parts[1].Trim();
                    Util.UserUpdateRoom(promoteMessage, stream);
                }
            }

            if (request.Message.StartsWith("/ff", StringComparison.OrdinalIgnoreCase))
            {

                var room = _gameManager.GetRoom(player.CurrentRoom);
                if (room != null && player.SlotID == room.RMSlotID)
                {
                    //room.GameState = 1; // Change game state to 'in-game'
                    Console.WriteLine($"Force Quit");

                    
                    byte[] data = { 0x02, 0x40, 0x00, 0x06, 0xAB, 0x00 };
                    _ = ServerHolder.ServerInstance.SendToRoom(data, room.RoomId);

                }

            }

            response.UserIGN = player.UserIGN;

            _ = ServerHolder.ServerInstance.SendToRoom(response.Serialize(), player.CurrentRoom);

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

            Console.WriteLine($"Match Reward for match: {request.MatchID}.");

            var playerA = _playerManager.GetPlayerByStream(stream);
            var room = _gameManager.GetRoom(playerA.CurrentRoom);

            MatchRewardRes response = new MatchRewardRes();
            response.Type = type;
            response.Action = action;

            // If the Sender is Room Master.
            if (playerA.UserIGN == room.RMIGN)
            {

                response.Players = request.Players;
                response.Players = MatchResult.CalculateLeaderboardPositions(response.Players);

                _gameManager.SaveMatchRewards(room.RoomId, response.Players);
                room.RewardsReady.Set();

                _ = ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryAddCarats, new
                {
                    amount = response.Players.First(p => p.PlayerIGN == playerA.UserIGN).Carats,
                    ID = playerA.UserID
                });

                _ = ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryAddEXP, new
                {
                    amount = response.Players.First(p => p.PlayerIGN == playerA.UserIGN).EXP,
                    ID = playerA.UserID
                });

                var query = ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryGetPlayerInfo, new
                {
                    username = playerA.UserIGN
                });

                response.NewCarats = query.FirstOrDefault()?.Carats ?? 0;
                response.NewExp =  query.FirstOrDefault()?.EXP ?? 0;

                // response.NewCarats = response.Players.First(p => p.PlayerIGN == playerA.UserIGN).Carats + _playerManager.GetCarats(playerA.UserID);
                // response.NewExp = response.Players.First(p => p.PlayerIGN == playerA.UserIGN).EXP + _playerManager.GetEXP(playerA.UserID);

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

                // foreach (var player in request.Players)
                // {
                //     Console.WriteLine($"Player: {player.PlayerIGN}, Kills: {player.Kills}, Deaths: {player.Deaths}, EXP: {player.EXP}, Carats: {player.Carats}, Additional Stats: {player.Stats.Serialize()}");
                // }


            }
            else
            {
                room.RewardsReady.Wait();

                response.Players = _gameManager.GetMatchRewards(room.RoomId);

                _ = ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryAddCarats, new
                {
                    amount = response.Players.First(p => p.PlayerIGN == playerA.UserIGN).Carats,
                    ID = playerA.UserID
                });

                _ = ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryAddEXP, new
                {
                    amount = response.Players.First(p => p.PlayerIGN == playerA.UserIGN).EXP,
                    ID = playerA.UserID
                });

                var query = ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryGetPlayerInfo, new
                {
                    username = playerA.UserIGN
                });


                response.NewCarats = query.FirstOrDefault()?.Carats ?? 0;
                response.NewExp =  query.FirstOrDefault()?.EXP ?? 0;

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

            }
            
            // byte[] data = {
            //     // Offset 0x00000000 to 0x00000416
            //     0x00, 0x40, 0x04, 0x17, 0x2A, 0x00, 0x64, 0x14, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x12, 0x72, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x53, 0x65, 0x69,
            //     0x6A, 0x75, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02,
            //     0x00, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4E, 0x4E, 0x4E, 0x4E,
            //     0x4E, 0x4E, 0x4E, 0x4E, 0x4E, 0x4E, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07,
            //     0x00, 0x50, 0x72, 0x69, 0x6D, 0x69, 0x74, 0x68, 0x65, 0x75, 0x73, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x02, 0x00, 0x01, 0x00, 0x6F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x4E, 0x4E, 0x4E, 0x4E, 0x4E, 0x4E, 0x4E, 0x4E, 0x4E, 0x4E, 0x28, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6F, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x53, 0x54, 0x44, 0x2C, 0x39, 0x32, 0x2C, 0x52, 0x55, 0x4E, 0x2C,
            //     0x31, 0x30, 0x31, 0x2C, 0x41, 0x54, 0x4B, 0x2C, 0x31, 0x30, 0x32, 0x2C,
            //     0x44, 0x4D, 0x47, 0x2C, 0x31, 0x39, 0x2C, 0x47, 0x44, 0x2C, 0x30, 0x2C,
            //     0x54, 0x46, 0x2C, 0x30, 0x2C, 0x53, 0x4B, 0x4C, 0x2C, 0x30, 0x2C, 0x4A,
            //     0x4D, 0x50, 0x2C, 0x31, 0x39, 0x31, 0x2C, 0x45, 0x41, 0x54, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //     0x00, 0x00, 0x00
            // };

                // _ = ServerHolder.ServerInstance.SendPacket(data, stream);


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

            // foreach (var players in _gameManager.GetPlayersInRoom(player.CurrentRoom))
            // {
            //     _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), players.Stream);
            // }

            _ = ServerHolder.ServerInstance.SendToRoom(response.Serialize(), player.CurrentRoom);

            


        }

        public static void StartMatch(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action = 0x23;
            
            var player = _playerManager.GetPlayerByStream(stream);
            var room = _gameManager.GetRoom(player.CurrentRoom);

            room.GameState = (byte)GameState.InProgress;

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
            _ = ServerHolder.ServerInstance.SendToRoom(response.Serialize(), player.CurrentRoom);

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

            _ = ServerHolder.ServerInstance.SendToRoom(response.Serialize(), player.CurrentRoom);

        }

        public static void LoadGame2(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action = 0x24;

            byte[] data = { 0x02, 0x40, 0x00, 0x0e, 0x24, 0x00, 0x07, 0x02, 0x01, 0x03, 0x04, 0x00, 0x06, 0x05 };
            var player = _playerManager.GetPlayerByStream(stream);

            Console.WriteLine("Loading 2");
            _ = ServerHolder.ServerInstance.SendPacket(data, stream);

        }

        public static void ChangeMap(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action = 0x20;

            ChangeMapReq request = ChangeMapReq.Deserialize(body);
            ChangeMapRes response = new ChangeMapRes();

            var player = _playerManager.GetPlayerByStream(stream);
            var room = _gameManager.GetRoom(player.CurrentRoom);



            response.Action = action;
            response.Type = 576;

            response.unknownValue1 = request.unknownValue1;
            response.ItemToggle = request.ItemToggle;
            response.BattleType = request.BattleType;

            response.RoundNumber = request.RoundNumber;
            response.GameMode1 = request.GameMode1;
            response.GameMode2 = request.GameMode2;
            response.GameMode3 = request.GameMode3;

            response.Map1 = request.Map1;
            response.Map2 = request.Map2;
            response.Map3 = request.Map3;

            room.Map1 = request.Map1;
            room.Map2 = request.Map2;
            room.Map3 = request.Map3;


            Console.WriteLine("Change Map");
            bool NOT = false;
            _ = ServerHolder.ServerInstance.SendToRoom(response.Serialize(), player.CurrentRoom);

            byte[] data = { 0x02, 0x40, 0x00, 0x06, 0xa9, 0x00 };
            _ = ServerHolder.ServerInstance.SendToRoom(data, player.CurrentRoom);

        }

        public static void FIN(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;


            var player = _playerManager.GetPlayerByStream(stream);
            player.Ready = false;

            var room = _gameManager.GetRoom(player.CurrentRoom);
            room.GameState = (byte)GameState.Waiting;

            byte[] data = { 0x02, 0x40, 0x00, 0x06, 0xAB, 0x00 };
            _ = ServerHolder.ServerInstance.SendPacket(data, stream);


        }

        public static void SelectTeam(ushort type, ushort action, byte[] body, NetworkStream stream)
        {

            var player = _playerManager.GetPlayerByStream(stream);

            SelectTeamReq request = SelectTeamReq.Deserialize(body);
            player.Team = request.Team;


            SelectTeamRes response = new SelectTeamRes();

            response.Action = 0x18;
            response.Type = 576;

            response.UserIGN = player.UserIGN;
            response.Team = request.Team;            

            Console.WriteLine($"User Selected Team: {request.Team}");

            _ = ServerHolder.ServerInstance.SendToRoom(response.Serialize(), player.CurrentRoom);


        }
    }
}
