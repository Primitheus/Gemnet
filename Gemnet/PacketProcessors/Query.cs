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


namespace Gemnet.PacketProcessors
{
    internal class Query {

        private static List<RoomInfo> rooms = new List<RoomInfo>();
        private static List<Player> players = new List<Player>();
        private static List<PlayerInfo> playerinfo = new List<PlayerInfo>();

        public static RoomInfo GetRoomByID(string ID) {
            return rooms.FirstOrDefault(room => room.SomeID == ID);
        }

        public static Player GetPlayerByName(string UserIGN) {
            return players.FirstOrDefault(player => player.IGN == UserIGN);
        }

        public static List<PlayerInfo> GetPlayersBySomeID(string someID)
        {
            return playerinfo.Where(player => player.SomeID == someID).ToList();
        }

        public static string GetRoomID(int value)
        {
            RoomInfo room = rooms.FirstOrDefault(r => r.unknownValue1 == value);
            return room != null ? room.SomeID : "0"; // Return -1 (or any default value) if no matching room is found.
        }

        public static List<Player> GetPlayersInRoom(string SomeID){
            return players.Where(player => player.SomeID == SomeID).ToList();
        }



        public static void GetEquippedAvatar(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            EquippedAvatarRes response = new EquippedAvatarRes();
            PlayerManager playerM = new PlayerManager();

            response.Type = type;
            response.Action = action;

            Console.WriteLine($"Get Equipped Avatar");

            var UserID = playerM.GetPlayerUserID;
            var AvatarID = playerM.GetPlayerCurrentAvatar(stream);

            if (AvatarID == 0)
            {
                Console.WriteLine("No Avatar Equipped, Equipping Default Avatar");
                var AvatarQuery = ServerHolder.DatabaseInstance.Select<ModelAvatar>(ModelAvatar.QueryGetAvatarIDs, new
                {
                    ID = UserID,
                });

                if (AvatarQuery != null && AvatarQuery.Any())
                {
                    AvatarID = AvatarQuery.First().AvatarID;
                    playerM.UpdatePlayerCurrentAvatar(stream, AvatarID);
                }
                else
                {
                    Console.WriteLine("No Avatar Found");
                    return;
                }

            } 
            
            response.AvatarID = AvatarID;
            
            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

        }

        public static void ChangeAvatar(ushort type, ushort action, byte[] body, NetworkStream stream) {
            action++;

            ChangeAvatarReq request = ChangeAvatarReq.Deserialize(body);   
            ChangeAvatarRes response = new ChangeAvatarRes(); 
            response.Type = type;
            response.Action = action;

            PlayerManager playerM = new PlayerManager();
            var IGN = playerM.GetPlayerIGN(stream);
            Console.WriteLine($"Player {IGN} Change Avatar to: {request.AvatarID}");

            var Query = ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryUpdateAvatar, new 
            {
                avatar = request.AvatarID,
                ID = playerM.GetPlayerUserID(stream)
            });

            playerM.UpdatePlayerCurrentAvatar(stream, request.AvatarID);
            Console.WriteLine($"Changed Avatar: {playerM.GetPlayerCurrentAvatar(stream)}");

            
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

        public static void CreateRoom(ushort type, ushort action, byte[] body, NetworkStream stream) {

            action++;
            CreateRoomReq request = CreateRoomReq.Deserialize(body);

            Console.WriteLine($"Create Room: Name='{request.RoomName}', ID='{request.SomeID}', MatchType='{request.MatchType}', GameMode='{request.GameMode}'");

            byte[] someData = { 0xb8, 0xfd, 0x7f, 0x02, 0xd8, 0x8c, 0x0d, 0x01 };

            CreateRoomRes response = new CreateRoomRes();

            response.Type = 576;
            response.Action = action;

            Random random = new Random();
            int randomResult = random.Next(1, 101);
            response.Result = randomResult;
            string ign;

            Server.clientUsernames.TryGetValue(stream, out string username);
            Console.WriteLine($"RoomMaster: {username}");
            // Create a new RoomInfoJson object or update an existing one
            RoomInfo newRoom = new RoomInfo
            {
                unknownValue1 = response.Result,
                unknownValue2 = response.Result,
                unknownValue3 = response.Result,
                unknownValue4 = 1,
                RoomMasterIGN = username,
                P2PID = request.P2PID,
                SomeID = request.SomeID,
                RoomName = request.RoomName,
                unknownValue6 = request.unknownvalue4,
                MaxPlayers = request.MaxPlayers,
                PlayerNumber = request.PlayerNumber,
                GameState = 87,
                unknownValue7 = request.unknownvalue5,
                MatchType = request.MatchType,
                unknownValue8 = 1,
                unknownValue9 = request.unknownvalue6,
                unknownValue10 = request.unknownvalue7,
                RoundNumber =  request.RoundNumber,
                GameMode = request.GameMode,
                unknownValue11 = request.unknownvalue8,
                unknownValue12 = request.unknownvalue9,
                Time = someData,
                Country = "US",
                Region = "NA",
            };


            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

            int[] itemID = { 1000175, 0, 0, 0, 0, 0, 0, 0, 0, 0 , 0 };

            Player newPlayer = new Player {
                unknownValue1 = 0,
                unknownValue2 = 11,
                EXP = 1337,
                IGN = username,
                P2PID = request.P2PID,
                SomeID = request.SomeID,
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

            players.Add(newPlayer);
            rooms.Add(newRoom);


        }

        public static void LeaveRoom(ushort type, ushort action, byte[] body, NetworkStream stream) {
            action++;
            
            //Some reason this is broken, So i'll just send a static response for now.

            /*
            
            LeaveRoomReq request = LeaveRoomReq.Deserialize(body);

            LeaveRoomRes response = new LeaveRoomRes();
            
            response.Type = 576;
            response.Action = action;
            response.Result = 9;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize());

            */

            Console.WriteLine($"Leaving Room");

            /*

            LeaveRoomReq request = LeaveRoomReq.Deserialize(body);

            LeaveRoomRes response = new LeaveRoomRes();
            
            response.Type = 576;
            response.Action = action;
            response.Result = 9;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize());

            */

            byte[] data = { 0x02, 0x40, 0x00, 0x0c, 0x83, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00 };

            _ = ServerHolder.ServerInstance.SendPacket(data, stream);

        }

        public class RoomInfo
        {
            public int unknownValue1 {get; set;}
            public int unknownValue2 {get; set;}
            public int unknownValue3 {get; set;}
            public int unknownValue4 {get; set;}
            public string RoomMasterIGN {get; set;}
            public int P2PID {get; set;}
            public string SomeID {get; set;}
            public string RoomName {get; set;}
            public int unknownValue6 {get; set;}
            public int MaxPlayers {get; set;}
            public int PlayerNumber {get; set;}
            public int GameState {get; set;}
            public int unknownValue7 {get; set;}
            public int MatchType {get; set;}
            public int unknownValue8 {get; set;}
            public int unknownValue9 {get; set;}
            public int unknownValue10 {get; set;}
            public int RoundNumber {get; set;}
            public int GameMode {get; set;}
            public int unknownValue11 {get; set;}
            public int unknownValue12 {get; set;}
            public byte[] Time {get; set;}
            public string Country {get; set;}
            public string Region {get; set;}
        }

        public class PlayerInfo
        {
            public int unknownValue1 {get; set;}
            public int unknownValue2 {get; set;}
            public int EXP {get; set;}
            public string IGN { get; set; }
            public int P2PID {get; set;}
            public string SomeID {get; set;}
            public int[] ItemID { get; set; }
            public int unknownValue4 {get; set;}
            public int unknownValue5 {get; set;}
            public int unknownValue6 {get; set;}
            public int unknownValue7 {get; set;}
            public int unknownValue8 {get; set;}
            public int unknownValue9 {get; set;}
            public int unknownValue10 {get; set;}
            public string Country {get; set;}
            public string Region {get; set;}

        }


        public static void GetRoomList(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            GetRoomListReq request = GetRoomListReq.Deserialize(body);

            byte[] someData = { 0xb8, 0xfd, 0x7f, 0x02, 0xd8, 0x8c, 0x0d, 0x01 };

            if (request.ChannelID == 10 || request.ChannelID == 1) {
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


                foreach (var room in rooms)
                {
                    response.Rooms.Add(new RoomInfoA
                    {
                        unknownValue1 = room.unknownValue1,
                        unknownValue2 = room.unknownValue2,
                        unknownValue3 = room.unknownValue3,
                        unknownValue4 = room.unknownValue4,
                        RoomMasterIGN = room.RoomMasterIGN,
                        P2PID = room.P2PID,
                        SomeID = room.SomeID,
                        RoomName = room.RoomName,
                        unknownValue6 = room.unknownValue6,
                        MaxPlayers = room.MaxPlayers,
                        PlayerNumber = room.PlayerNumber,
                        GameState = room.GameState,
                        unknownValue7 = room.unknownValue7,
                        MatchType = room.MatchType,
                        unknownValue8 = room.unknownValue8,
                        unknownValue9 = room.unknownValue9,
                        unknownValue10 = room.unknownValue10,
                        RoundNumber = room.RoundNumber,
                        GameMode = room.GameMode,
                        unknownValue11 = room.unknownValue11,
                        unknownValue12 = room.unknownValue12,
                        Time = someData,
                        Country = "US",
                        Region = "NA",
                    });

                }

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

            } 

        }

        public static void JoinRoom(ushort type, ushort action, byte[] body, NetworkStream stream) {
            action++;
            JoinRoomReq request = JoinRoomReq.Deserialize(body);

            Console.WriteLine($"Join Room: {request.SomeID}");



            JoinRoomRes response = new JoinRoomRes();

            response.Type = 576;
            response.Action = action;

            response.UnknownValue1 = request.UnknownValue1;
            response.UnknownValue2 = request.UnknownValue2;

            response.UnknownValue3 = 6; // Slot ID 

            var roomData = GetRoomByID(request.SomeID);
            var RoomMaster = GetPlayerByName(roomData.RoomMasterIGN);
        
            response.UnknownValue5 = RoomMaster.unknownValue1;
            response.UnknownValue6 = RoomMaster.unknownValue2;

            response.RoomMaster = roomData.RoomMasterIGN;
            response.SomeID = roomData.SomeID;
            response.UnknownValue7 = request.UnknownValue7; // P2P ID
            response.RoomName = roomData.RoomName;
            response.UnknownValue9 = roomData.unknownValue6;
            response.MaxPlayers = roomData.MaxPlayers;
            response.PlayerNumber = roomData.PlayerNumber + 1;
            response.GameState = roomData.GameState;
            response.unknownValue7 = roomData.unknownValue7;
            response.MatchType = roomData.MatchType;
            response.unknownValue8 = roomData.unknownValue8;
            response.unknownValue9 = roomData.unknownValue9;
            response.RoundNumber = roomData.RoundNumber;
            response.GameMode = roomData.GameMode;
            response.UnknownValue11 = roomData.unknownValue11;
            response.UnknownValue12 = roomData.unknownValue12;
            response.Country = "US";
            response.Region = "NA";

            var ign = "";

            if (Server.clientUsernames.TryGetValue(stream, out string username))
            {
                ign = username;
            }
            else
            {
                ign = "UnknownUser";
            }

            int[] itemID = { 1000175, 0, 0, 0, 0, 0, 0, 0, 0, 0 , 0 };

            Player newPlayer = new Player {
                unknownValue1 = 6,
                unknownValue2 = 1,
                EXP = 1337,
                IGN = ign,
                P2PID = request.UnknownValue7,
                SomeID = request.SomeID,
                ItemID = itemID,
                unknownValue4 = 78,
                unknownValue5 = 78,
                unknownValue6 = 232,
                unknownValue7 = 3,
                unknownValue8 = 1020001,
                unknownValue9 = 1,
                unknownValue10 = 5,
                Country = "US",
                Region = "NA",

            };

            players.Add(newPlayer);
            UserJoined(type, 0x10, stream, ign);

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

        }

        public static void JoinGetPlayers(ushort type, ushort action, byte[] body, NetworkStream stream) {
            action++;

            Console.WriteLine("Get Players");

            GetPlayersReq request = GetPlayersReq.Deserialize(body);
            
            GetPlayersRes response = new GetPlayersRes();


            Console.WriteLine($"Get Players {request.unknownValue1}, {request.unknownValue2}");

            response.Action = action;
            response.Type = 576;

            response.unknownValue1 = request.unknownValue1;
            response.unknownValue2 = 8;
            response.unknownValue3 = 0;
            response.PlayerNumber = 2;

            var roomID = GetRoomID(request.unknownValue2);
            var playerData = GetPlayersInRoom(roomID);

            
            foreach (var player in playerData)
            {
                Console.WriteLine($"Get Player {player.IGN}");

                Player newPlayer = new Player {
                    unknownValue1 = player.unknownValue1,
                    unknownValue2 = player.unknownValue2,
                    EXP = player.EXP,
                    IGN = player.IGN,
                    P2PID = player.P2PID,
                    SomeID = player.SomeID,
                    ItemID = player.ItemID,
                    unknownValue4 = player.unknownValue4,
                    unknownValue5 = player.unknownValue5,
                    unknownValue6 = player.unknownValue6,
                    unknownValue7 = player.unknownValue7,
                    unknownValue8 = player.unknownValue8,
                    unknownValue9 = player.unknownValue9,
                    Country = "US",
                    Region = "NA"

                };

                if (response.Players == null) {

                    response.Players = new List<Player>();
                }

                Console.WriteLine("Add The Player");
                response.Players.Add(newPlayer);

            }
            
            Console.WriteLine("Send Player List Packet");
             _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);


        }

        public static void UserJoined(ushort type, ushort action, NetworkStream stream, string IGN)
        {
            Console.WriteLine("Notify User Joined Message");

            UserJoinedRes response = new UserJoinedRes();



            response.Action = action;
            response.Type = 576;

            response.unknownValue1 = 6; // Slot ID
            var playerData = GetPlayerByName(IGN);

            response.Players.Add(new PlayerJoin {
                UserID = 2,
                IGN = playerData.IGN,
                unknownValue1 = 1,
                EXP = playerData.EXP,
                P2PID = playerData.P2PID,
                SomeID = playerData.SomeID,
                ItemID = playerData.ItemID,
                unknownValue2 = playerData.unknownValue4,
                unknownValue3 = playerData.unknownValue5,
                unknownValue4 = playerData.unknownValue6,
                unknownValue5 = playerData.unknownValue7,
                unknownValue6 = playerData.unknownValue8,
                unknownValue7 = playerData.unknownValue9,
                unknownValue8 = playerData.unknownValue10,
                Country = "US",
                Region = "NA"
            });

   

            byte[] data = { 0x02, 0x40, 0x00, 0x1b, 0x18, 0x00, 0x4E, 0x69 ,0x6D, 0x6F, 0x6E, 0x69, 0x78, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        
            bool NOT = true;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream, NOT);
            _ = ServerHolder.ServerInstance.SendPacket(data, stream, false);


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
                        if (item == "carats" && amount > 0) {
                            ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryAddCarats, new 
                            {
                                amount,
                                ID = QueryID.First().UUID,
                            });
                            response.Message = $"{UserIGN} Gave {player} {amount} Carats.";
                        } else if (item == "astros" && amount > 0) {
                            ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryAddAstros, new 
                            {
                                amount,
                                ID = QueryID.First().UUID,
                            });
                            response.Message = $"{UserIGN} Gave {player} {amount} Astros.";
                        } else {
                            response.Message = $"Incorrect or Invalid Arguments.";
                        }
                    }
                }
                else
                {  
                    response.Message = $"Incorrect or Invalid Arguments.";

                }
            } else {
                response.Message = request.Message;
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

        public static void UserReady(ushort type, ushort action, byte[] body, NetworkStream stream) 
        {
            action = 0x22;

            UserReadyReq request = UserReadyReq.Deserialize(body);
            Console.WriteLine($"Notify Message User Ready.");

            UserReadyRes response = new UserReadyRes();

            response.Action = action;
            response.Type = 576;

            response.unknownValue1 = request.unknownValue1;
            response.unknownValue2 = request.unknownValue1;
            var ign = "";
            if (Server.clientUsernames.TryGetValue(stream, out string username))
            {
                ign = username;
            }
            else
            {
                ign = "UnknownUser";
            }

            response.IGN = ign;

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

            int rand1 = random.Next(1, 256);
            int rand2 = random.Next(1, 256);

            response.unknownValue1 = rand1;
            response.unknownValue2 = rand2;

            response.unknownValue3 = request.unknownValue1;
            response.unknownValue5 = request.unknownValue3;

            byte[] data = {0x02, 0x40, 0x00, 0x12, 0x23, 0x00, 0xdf, 0x45, 0x00, 0x00, 0xe9, 0x03, 0xee, 0x03, 0xf1, 0x03, 0xef, 0x03};

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

            if (Server.clientUsernames.TryGetValue(stream, out string username))
            {
                response.IGN = username;
            }
            else
            {
                response.IGN = "UnknownUser";
            }

            response.unknownValue1 = request.unknownValue1;

            Console.WriteLine("Loading 1");
            //Task.Delay(10000);
            bool NOT = true;
            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream, NOT);

        }

        public static void LoadGame2(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action = 0x24;

            byte[] data = { 0x02, 0x40, 0x00, 0x0e, 0x24, 0x00, 0x07, 0x02, 0x01, 0x03, 0x04, 0x00, 0x06, 0x05};

            Console.WriteLine("Loading 2");
            bool NOT = true;
            //Task.Delay(10000);
            _ = ServerHolder.ServerInstance.SendPacket(data, stream, NOT);

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
    }
}
