using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemnet.Network.Packets;
using static Program;
using Newtonsoft.Json;
using System.Globalization; // Make sure to add a reference to the Newtonsoft.Json library

// Read the JSON data from the file

// Deserialize the JSON data into a list of RoomInfoJson

namespace Gemnet.PacketProcessors
{
    internal class Query {


        public static void GetEquippedAvatar(ushort type, ushort action)
        {
            action++;
            Console.WriteLine($"Get Equipped Avatar");
            byte[] data = { 0x00, 0x40, 0x00, 0x0a, 0xa1, 0x00, 0xd3, 0xfa, 0x23, 0x00 };

            _ = ServerHolder.ServerInstance.SendPacket(data);


        }

        public static void Unknown8(ushort type, ushort action)
        {
            action++;
            Console.WriteLine($"Unknown 8");

            byte[] data = { 0x00, 0x40, 0x00, 0x06, 0xa7, 0x00 };

            _ = ServerHolder.ServerInstance.SendPacket(data);


        }
        
        public static void Unknown9(ushort type, ushort action)
        {
            action++;
            Console.WriteLine("Unknown 9");

            byte[] data = { 0x00, 0x40, 0x00, 0x06, 0xa5, 0x00 };

            _ = ServerHolder.ServerInstance.SendPacket(data);
        }

        public static void CreateRoom(ushort type, ushort action, byte[] body) {

            action++;
            CreateRoomReq request = CreateRoomReq.Deserialize(body);

            Console.WriteLine($"Create Room: Name='{request.RoomName}', ID='{request.SomeID}', MatchType='{request.MatchType}', GameMode='{request.GameMode}'");

            string jsonFilePath = "rooms.json";
            string jsonData = File.ReadAllText(jsonFilePath);
            List<RoomInfoJson> roomInfoJsonList = JsonConvert.DeserializeObject<List<RoomInfoJson>>(jsonData);
            byte[] someData = { 0xb8, 0xfd, 0x7f, 0x02, 0xd8, 0x8c, 0x0d, 0x01 };

            // Create a new RoomInfoJson object or update an existing one
            RoomInfoJson newRoom = new RoomInfoJson
            {
                unknownValue1 = 1,
                unknownValue2 = 1,
                unknownValue3 = 1,
                unknownValue4 = 1,
                RoomMasterIGN = "Nimonix",
                unknownValue5 = request.unknownvalue2,
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
                unknownValue12 =request.unknownvalue9,
                Time = someData,
                Country = "GB",
                Region = "EU",

            };

            roomInfoJsonList.Add(newRoom);

            // Serialize the updated list back to JSON
            string updatedJsonData = JsonConvert.SerializeObject(roomInfoJsonList, Formatting.Indented);

            // Write the JSON data back to rooms.json
            File.WriteAllText(jsonFilePath, updatedJsonData);

            CreateRoomRes response = new CreateRoomRes();

            response.Type = 576;
            response.Action = action;
            response.Result = 6;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize());

        }

        public static void LeaveRoom(ushort type, ushort action, byte[] body) {
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

            _ = ServerHolder.ServerInstance.SendPacket(data);

        }

        public class RoomInfoJson
        {
            public int unknownValue1 {get; set;}
            public int unknownValue2 {get; set;}
            public int unknownValue3 {get; set;}
            public int unknownValue4 {get; set;}
            public string RoomMasterIGN {get; set;}
            public int unknownValue5 {get; set;}
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

        public static void GetRoomList(ushort type, ushort action, byte[] body)
        {
            action++;
            GetRoomListReq request = GetRoomListReq.Deserialize(body);

            byte[] someData = { 0xb8, 0xfd, 0x7f, 0x02, 0xd8, 0x8c, 0x0d, 0x01 };

            if (request.ChannelID == 10) {
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

                List<RoomInfoJson> roomInfoJsonList = JsonConvert.DeserializeObject<List<RoomInfoJson>>(jsonData);

                foreach (var roomJson in roomInfoJsonList)
                {
                    response.Rooms.Add(new RoomInfo
                    {
                        unknownValue1 = roomJson.unknownValue1,
                        unknownValue2 = roomJson.unknownValue2,
                        unknownValue3 = roomJson.unknownValue3,
                        unknownValue4 = roomJson.unknownValue4,
                        RoomMasterIGN = roomJson.RoomMasterIGN,
                        unknownValue5 = roomJson.unknownValue5,
                        SomeID = roomJson.SomeID,
                        RoomName = roomJson.RoomName,
                        unknownValue6 = roomJson.unknownValue6,
                        MaxPlayers = roomJson.MaxPlayers,
                        PlayerNumber = roomJson.PlayerNumber,
                        GameState = roomJson.GameState,
                        unknownValue7 = roomJson.unknownValue7,
                        MatchType = roomJson.MatchType,
                        unknownValue8 = roomJson.unknownValue8,
                        unknownValue9 = roomJson.unknownValue9,
                        unknownValue10 = roomJson.unknownValue10,
                        RoundNumber = roomJson.RoundNumber,
                        GameMode = roomJson.GameMode,
                        unknownValue11 = roomJson.unknownValue11,
                        unknownValue12 = roomJson.unknownValue12,
                        Time = someData,
                        Country = roomJson.Country,
                        Region = roomJson.Region,
                    });
                }

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize());

            } 

        }

        public static void GetReward(ushort type, ushort action, byte[] body)
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

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize());

        }

        

    }
}
