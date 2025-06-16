using System;
using Gemnet.Network.Header;
using Gemnet.Network.Packets;
using Gemnet.Packets.Login;
using Gemnet.Persistence.Models;
using System.Net.Sockets;
using Org.BouncyCastle.Asn1.Ocsp;
using static Program;
using static Server;
using System.Text;

namespace Gemnet.PacketProcessors
{
    public enum AccountState
    {
        Active = 0,
        Locked = 1,
        TimedOut = 2,
        Banned = 3

    }

    internal class Login
    {
        public static void VersionCheck(ushort type, ushort action, NetworkStream stream)
        {

            //Console.WriteLine("[Server] Version Check Successful");
            action++;
            _ = ServerHolder.ServerInstance.SendPacket(type, 0x6, action, stream);


        }

        public static void CredentialCheck(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            Console.WriteLine("[Credential Check]");
            LoginReq request = LoginReq.Deserialize(body);

            var LoginQuery = ServerHolder.DatabaseInstance.SelectFirst<ModelAccount>(ModelAccount.QueryLoginAccount, new
            {
                Email = request.Email,
                Password = request.Password,
            });

            Console.WriteLine($"[Auth] Type=0x{request.Type:X2}, Length={request.Size}, Action=0x{request.Action:X2}, Username='{request.Email}', Password={request.Password}");


            static void SendLoginFailResponse(string errorMessage, NetworkStream stream)
            {
                LoginFailRes response = new LoginFailRes
                {
                    Type = 528,
                    Action = 645,
                    Error = errorMessage,
                    Code = 29374
                };

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
            }

            if (LoginQuery == null)
            {

                Console.WriteLine("Email or Password is incorrect.");
                SendLoginFailResponse("Email or Password is incorrect.", stream);
                return;
            }
            else
            {

                if (LoginQuery.State == (int)AccountState.Banned)
                {

                    Console.WriteLine("Account is Banned.");
                    SendLoginFailResponse("Account is Banned.", stream);
                    return;
                }

                else if (LoginQuery.State == (int)AccountState.Locked)
                {
                    Console.WriteLine("Account is Locked.");
                    SendLoginFailResponse("Account is Locked.", stream);
                    return;
                }
                else if (LoginQuery.State == (int)AccountState.TimedOut)
                {
                    Console.WriteLine("Account is Timed Out.");
                    SendLoginFailResponse("Account is Timed Out.", stream);
                    return;
                }

                else
                {

                    LoginRes response = new LoginRes();
                    Random random = new Random();
                    int randomResult = random.Next(1, 101);
                    response.Type = type;
                    response.Action = action;
                    response.UserID = LoginQuery.UUID;
                    response.IGN = LoginQuery.IGN + (randomResult.ToString());
                    response.Exp = LoginQuery.EXP;
                    response.Carats = LoginQuery.Carats;
                    response.GUID = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
                    response.Token = GenerateRandomString(30);
                    response.Region = "NA";
                    response.Country = "US";
                    response.ForumName = LoginQuery.ForumName;

                    Console.WriteLine($"Token: {response.Token}");

                    PlayerManager.Player player = new PlayerManager.Player
                    {

                        UserID = response.UserID,
                        UserIGN = response.IGN,
                        Carats = response.Carats,
                        EXP = response.Exp,
                        Token = response.Token,
                        ForumName = response.ForumName,
                        Region = response.Region,
                        Country = response.Country,
                        GUID = response.GUID,
                        CurrentAvatar = LoginQuery.CurrentAvatar,

                    };

                    PlayerManager.Players.Add(stream, player);

                    clientUsernames.Add(stream, response.IGN);
                    clientUserID.Add(stream, response.UserID);

                    _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
                }
            }


        }

        static string GenerateRandomString(int length)
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"; // Add any other characters you want to include
            Random random = new Random();
            StringBuilder stringBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(characters.Length);
                stringBuilder.Append(characters[index]);
            }

            return stringBuilder.ToString();
        }

        public static void ServerTime(ushort type, ushort action, NetworkStream stream)
        {
            action++;

            // Get the current UTC time
            DateTime utcNow = DateTime.UtcNow;

            // Convert it to PST
            TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime pstNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, pstZone);


            // DateTime now = DateTime.Now; // Get the current time
            // long ticks = now.Ticks; // Convert the current time to a 64-bit integer
            // byte[] timeArray = BitConverter.GetBytes(ticks); // Convert the 64-bit integer to a byte array

            // Get the current time in milliseconds since Unix epoch
            long nodejsTimestamp = ((DateTimeOffset)pstNow).ToUnixTimeMilliseconds();
            long windows64Timestamp = (nodejsTimestamp + 11644473600000) * 10000;
            byte[] timeArray = BitConverter.GetBytes(windows64Timestamp);

            ServerTime serverTime = new ServerTime();
            serverTime.Type = type;
            serverTime.Action = action;
            serverTime.Time = timeArray;

            Console.WriteLine($"[Server Time : {utcNow}] Data={BitConverter.ToString(timeArray)}");

            _ = ServerHolder.ServerInstance.SendPacket(serverTime.Serialize(), stream);
        }

        public static void CashUnknown(ushort type, ushort action, NetworkStream stream)
        {
            action++;


            byte[] data = { 0x01, 0x00 };
            _ = ServerHolder.ServerInstance.SendPacket(type, 0x8, action, data, stream);


        }
        public static void GetBuddyList(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            Console.WriteLine($"Get Buddy List");

            GetBuddyListReq request = GetBuddyListReq.Deserialize(body);

            GetBuddyListRes response = new GetBuddyListRes();

            response.Type = type;
            response.Action = action;

            // Temporary hardcoded buddy list
            var Buddies = new List<Buddy>
            {
                new Buddy { UserID = 999, UserIGN = "BUDDY1" },
                new Buddy { UserID = 998, UserIGN = "BUDDY2" },
                new Buddy { UserID = 997, UserIGN = "BUDDY3" },
                new Buddy { UserID = 996, UserIGN = "BUDDY4" },

            };

            response.Buddies = Buddies;

            string hexOutput = string.Join(", ", response.Serialize().Select(b => $"0x{b:X2}"));
            
            Console.WriteLine($"Get Test Data Buddy List: {hexOutput}");


            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

        }

        public static void ADD_BUDDY(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            AddBuddyReq request = AddBuddyReq.Deserialize(body);
            Console.WriteLine($"Add Buddy: {request.UserIGN}");


            // ON SUCCESS
            //_ = ServerHolder.ServerInstance.SendPacket(type, 0x6, action, stream);

            // ON FAILURE For now we will just send a failure response.
            byte[] data = { 0x02, 0x10, 0x02, 0x08, 0xAB, 0x01, 0x00, 0x00, 0x41, 0x49, 0x4C, 0x28, 0x75, 0x6E, 0x6B, 0x6E, 0x6F, 0x77, 0x6E, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x01, 0x5C, 0x13, 0x60, 0x01, 0x02, 0x00, 0x00, 0x00, 0x30, 0x13, 0x60, 0x01, 0xA0, 0xFD, 0xF9, 0x01, 0xCD, 0x98, 0x35, 0x76, 0x00, 0x00,
                0x8D, 0x00, 0x00, 0x00, 0x00, 0x00, 0xDA, 0x98, 0x35, 0x76, 0x45, 0xC9, 0x67, 0xB7, 0x5C, 0x13, 0x60, 0x01, 0x38, 0x13, 0x60, 0x01, 0x02, 0x00, 0x00, 0x00, 0x14, 0x8B, 0xC5,
                0x72, 0x28, 0xBF, 0xB9, 0x01, 0xD8, 0xC6, 0x8D, 0x00, 0xD8, 0xC6, 0x8D, 0x00, 0x68, 0xFE, 0x8D, 0x00, 0x04, 0xC7, 0x8D, 0x00, 0x9C, 0xFD, 0xF9, 0x01, 0xC7, 0x70, 0xC5, 0x72,
                0x0C, 0xC4, 0xB9, 0x01, 0xB8, 0xFD, 0xF9, 0x01, 0x04, 0xC7, 0x8D, 0x00, 0xB8, 0xFD, 0xF9, 0x01, 0xFD, 0x9E, 0xC5, 0x72, 0x98, 0xC9, 0x8D, 0x00, 0x38, 0x13, 0x60, 0x01, 0x00,
                0x00, 0x00, 0x00, 0xD8, 0xFD, 0xF9, 0x01, 0x99, 0x97, 0xC5, 0x72, 0xD8, 0xC6, 0x8D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x38, 0x13, 0x60, 0x01, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFC, 0xFD, 0xF9, 0x01, 0xFC, 0xFD, 0xF9, 0x01, 0x82, 0xA2, 0xCA, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC8, 0x8F, 0x87,
                0x01, 0xC8, 0x8F, 0x87, 0x01, 0x60, 0x52, 0xEB, 0x05, 0xFF, 0xFF, 0xFF, 0xFF, 0x4A, 0x70, 0x7D, 0x01, 0xEA, 0x57, 0x88, 0x00, 0x38, 0x13, 0x60, 0x01, 0x01, 0x00, 0x00, 0x00,
                0xC8, 0x8F, 0x87, 0x01, 0xB4, 0x58, 0x87, 0x00, 0xCB, 0x58, 0x87, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFD, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xE3, 0x7C, 0x03, 0x00, 0x04,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x82, 0x87, 0x01, 0x98, 0x81, 0x87, 0x01, 0x14, 0x82, 0x87, 0x01, 0x60, 0x52, 0xEB, 0x05, 0x98, 0x81, 0x87, 0x01, 0x4A, 0x70 };

            string hexOutput = string.Join(", ", data.Select(b => $"0x{b:X2}"));
            Console.WriteLine($"Add Buddy Response: {hexOutput}");

            _ = ServerHolder.ServerInstance.SendPacket(data, stream);
            
        }

        public static void TO_LOBBY(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            ToLobbyReq request = ToLobbyReq.Deserialize(body);
            Console.WriteLine($"To Lobby");

            ToLobbyRes response = new ToLobbyRes();
            response.Type = 528;
            response.Action = action;

            response.Result = 1;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
        }

        public static void SetOptionInventory(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            Console.WriteLine($"Set Option Inventory");

            byte[] data = { 0x00, 0x10, 0x00, 0x08, 0xD2, 0x00, 0x01, 0x00 };

            _ = ServerHolder.ServerInstance.SendPacket(data, stream);

        }

    }


}
