using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemnet.Network.Packets;
using static Program;

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

            Console.WriteLine($"Create Room: Name='{request.RoomName}', ID1='{request.SomeID}', ID2='{request.SomeID2}', {request.unkownvalue1}, {request.unkownvalue2}");

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

                */

                response.unknownValue1 = 1;
                response.unknownValue2 = 1;
                response.unknownValue3 = 1;
                response.unknownValue4 = 8;
                response.unknownValue5 = 1;
                response.unknownValue6 = 1;
                response.unknownValue7 = 308; // Might actually be a string, though unlikely.

                response.RoomMasterIGN = "Gemnet";

                response.unknownValue8 = 9;

                response.SomeID = "31"; // Seems to always be 31 needs further testing.
                response.RoomName = "Example Room";

                response.unknownValue9 = 67;

                response.MaxPlayers = 99;
                response.PlayerNumber = 99;

                response.unknownValue10 = 343;

                response.MatchType = 257; // 257 = team, 1 = single. Might need adjust property ends or begins (offsets)
                response.GameMode = 259; // Might need to adjust where the offsets.

                response.unknownValue12 = 1;
                response.unknownValue13 = 1;

                response.Time = someData; // It is not time
                response.Country = "GB";
                response.Region = "EU";

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize());

            } 

        }

    }
}
