﻿using System;
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

        if (LoginQuery == null) {
                
                Console.WriteLine("Email or Password is incorrect.");
                SendLoginFailResponse("Email or Password is incorrect.", stream);
                return;
        } else {

            if (LoginQuery.State == (int)AccountState.Banned) {

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

            else {

                    LoginRes response = new LoginRes();
                    Random random = new Random();
                    int randomResult = random.Next(1, 101);
                    response.Type = type;
                    response.Action = action;
                    response.UserID = LoginQuery.UUID;
                    response.IGN = LoginQuery.IGN+(randomResult.ToString());
                    response.Exp = LoginQuery.EXP;
                    response.Carats = LoginQuery.Carats;
                    response.GUID = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
                    response.Token = GenerateRandomString(30);
                    response.Region = "NA";
                    response.Country = "US";
                    response.ForumName = LoginQuery.ForumName;
                    
                    Console.WriteLine($"Token: {response.Token}");

                    PlayerManager.Player player = new PlayerManager.Player {

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
        public static void GetBuddyList(ushort type, ushort action, NetworkStream stream)
        {
            action++;
            Console.WriteLine($"Get Buddy List");
            byte[] data = {0x00, 0x10, 0x0b, 0xc0, 0xa9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7e, 0x06, 0xeb, 0x72, 0x00, 0x00, 0x00, 0x00, 0x7e, 0x06, 0xeb, 0x72, 0xc7, 0xc1, 0xfb, 0xfe, 0x94, 0x08, 0x56, 0x22, 0x00, 0x00, 0x00, 0x00, 0x98, 0xec, 0xe6, 0x06, 0x50, 0xf5, 0xe6, 0x06, 0x70, 0xe9, 0xe6, 0x06, 0xef, 0xc1, 0xfb, 0xfe, 0xc0, 0x11, 0x82, 0x76, 0xcc, 0xea, 0xe6, 0x06, 0x00, 0x00, 0x00, 0x00, 0xd7, 0xc1, 0xfb, 0xfe, 0x88, 0xe9, 0xe6, 0x06, 0x88, 0xea, 0xe6, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x98, 0xea, 0xe6, 0x06, 0x98, 0xea, 0xe6, 0x06, 0xf2, 0x2d, 0x13, 0x75, 0x00, 0x00, 0x00, 0x00, 0x57, 0xc1, 0xfb, 0xfe, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7d, 0x2d, 0x13, 0x75, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0xe8, 0xea, 0xe6, 0x06, 0xc8, 0xea, 0xe6, 0x06, 0xcc, 0xea, 0xe6, 0x06, 0x07, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x4e, 0x00, 0x74, 0x00, 0x54, 0x00, 0x65, 0x00, 0x72, 0x00, 0x6d, 0x00, 0x69, 0x00, 0x6e, 0x00, 0x61, 0x00, 0x74, 0x00, 0x65, 0x00, 0x54, 0x00, 0x68, 0x00, 0x72, 0x00, 0x65, 0x00, 0x61, 0x00, 0x64, 0x00, 0x00, 0x00, 0x57, 0xc1, 0xfb, 0xfe, 0x28, 0xea, 0xe6, 0x06, 0x26, 0xfc, 0xc2, 0x77, 0x78, 0xeb, 0xe6, 0x06, 0xb0, 0xa7, 0x0f, 0x75, 0x0f, 0xcd, 0x09, 0x8d, 0xfe, 0xff, 0xff, 0xff, 0xdc, 0xea, 0xe6, 0x06, 0x06, 0x0d, 0x13, 0x75, 0xc8, 0xea, 0xe6, 0x06, 0xcc, 0xea, 0xe6, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe8, 0xea, 0xe6, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xe8, 0xea, 0xe6, 0x06, 0x0b, 0x00, 0x00, 0x00, 0x30, 0xeb, 0xe6, 0x06, 0x4a, 0xfc, 0xc2, 0x77, 0xbe, 0xca, 0xf8, 0x76, 0xec, 0x01, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x0e, 0x34, 0x01, 0x08, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x00, 0x00, 0x80, 0x0f, 0x28, 0x01, 0x30, 0xeb, 0xe6, 0x06, 0x40, 0x90, 0x27, 0x01, 0x48, 0x90, 0x27, 0x01, 0x80, 0x0f, 0x28, 0x01, 0x6c, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x80, 0x0f, 0x28, 0x01, 0x03, 0x00, 0x00, 0x00, 0x6c, 0x00, 0x00, 0x00, 0x64, 0xeb, 0xe6, 0x06, 0x76, 0x90, 0x27, 0x01, 0x10, 0x0e, 0x34, 0x01, 0x28, 0x5f, 0x27, 0x01, 0x03, 0x00, 0x00, 0x00, 0x45, 0x5f, 0x27, 0x01, 0x72, 0x00, 0x00, 0x00, 0xf0, 0xf0, 0x27, 0x01, 0x00, 0x00, 0x00, 0x00, 0x72, 0x00, 0x00, 0x00, 0x3c, 0xeb, 0xe6, 0x06, 0x10, 0x0e, 0x34, 0x01, 0xb0, 0xeb, 0xe6, 0x06, 0xfc, 0x43, 0x27, 0x01, 0x58, 0xd7, 0x27, 0x01, 0xff, 0xff, 0xff, 0xff, 0x45, 0x5f, 0x27, 0x01, 0x68, 0x08, 0x34, 0x01, 0x5c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0xeb, 0xe6, 0x06, 0xb3, 0x60, 0x27, 0x01, 0x30, 0x0b, 0x34, 0x01, 0xc0, 0xeb, 0xe6, 0x06, 0x77, 0x43, 0x27, 0x01, 0x13, 0x00, 0x00, 0x00, 0x4c, 0x2c, 0x27, 0x01, 0xf0, 0xf0, 0x27, 0x01, 0x43, 0x2c, 0x27, 0x01, 0x68, 0x08, 0x34, 0x01, 0xc8, 0x9e, 0xae, 0x05, 0x5c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x98, 0xeb, 0xe6, 0x06, 0x94, 0xeb, 0xe6, 0x06, 0x48, 0xee, 0xe6, 0x06, 0xfc, 0x43, 0x27, 0x01, 0x68, 0xd3, 0x27, 0x01, 0xff, 0xff, 0xff, 0xff, 0x43, 0x2c, 0x27, 0x01, 0xb4, 0x14, 0x27, 0x01, 0xf0, 0xf0, 0x27, 0x01, 0xf0, 0xf0, 0x27, 0x01, 0xdc, 0xd1, 0x27, 0x01, 0xe8, 0x07, 0x00, 0x00, 0x68, 0x08, 0x34, 0x01, 0x5c, 0x00, 0x00, 0x00, 0xc8, 0x9e, 0xae, 0x05, 0xf0, 0x0a, 0x34, 0x01, 0xbe, 0x14, 0x27, 0x01, 0xf0, 0x0a, 0x34, 0x01, 0xe0, 0x34, 0x98, 0x1c, 0x40, 0x42, 0xa1, 0x1c, 0xac, 0x42, 0xa1, 0x1c, 0xb8, 0x1b, 0x4c, 0x03, 0x9b, 0x3c, 0xa7, 0x65, 0x56, 0x55, 0x4d, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x70, 0x77, 0x6f, 0x72, 0x6b, 0x65, 0x72, 0x2e, 0x63, 0x70, 0x70, 0x20, 0x20, 0x20, 0x20, 0x2d, 0x20, 0x20, 0x36, 0x31, 0x20, 0x43, 0x48, 0x45, 0x43, 0x4b, 0x28, 0x43, 0x4d, 0x44, 0x2c, 0x20, 0x74, 0x68, 0x72, 0x65, 0x61, 0x64, 0x31, 0x37, 0x2c, 0x20, 0x42, 0x30, 0x30, 0x38, 0x39, 0x37, 0x34, 0x49, 0x30, 0x31, 0x38, 0x38, 0x31, 0x37, 0x29, 0x20, 0x46, 0x55, 0x4e, 0x43, 0x61, 0x38, 0x2c, 0x20, 0x52, 0x45, 0x53, 0x30, 0x30, 0x20, 0x6c, 0x65, 0x6e, 0x67, 0x74, 0x68, 0x28, 0x31, 0x30, 0x29, 0x0a, 0x00, 0x01, 0x94, 0x03, 0x48, 0x00, 0x94, 0x03, 0x08, 0x00, 0x00, 0x00, 0x34, 0x0d, 0xe0, 0xfe, 0x9c, 0xec, 0xe6, 0x06, 0x00, 0xf0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xf1, 0xff, 0x02, 0x00, 0x0c, 0xed, 0xe6, 0x06, 0x64, 0xe4, 0xc3, 0x77, 0x30, 0x7b, 0x27, 0x93, 0x64, 0x00, 0x22, 0x7c, 0xfc, 0x01, 0x93, 0x03, 0x00, 0x00, 0x93, 0x03, 0x02, 0x00, 0x00, 0x00, 0xac, 0xed, 0xe6, 0x06, 0x07, 0x00, 0x02, 0x00, 0xa9, 0x1a, 0x10, 0x00, 0x94, 0x08, 0x56, 0x22, 0xf1, 0xff, 0x02, 0x00, 0xef, 0xff, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x08, 0x00, 0x11, 0x00, 0xa9, 0x1a, 0x10, 0x00, 0x02, 0x00, 0x00, 0x00, 0x5c, 0xf5, 0xe6, 0x06, 0x78, 0x74, 0x96, 0x03, 0xc0, 0xae, 0xe6, 0x21, 0x6d, 0x00, 0x00, 0x00, 0xe0, 0xa9, 0x95, 0x03, 0x0c, 0x00, 0x00, 0x00, 0x50, 0x38, 0x98, 0x1c, 0x48, 0x00, 0x94, 0x03, 0x90, 0x01, 0x94, 0x03, 0xd0, 0x34, 0x98, 0x1c, 0x40, 0x7a, 0x96, 0x03, 0xfe, 0xff, 0xff, 0xff, 0x88, 0xec, 0xe6, 0x06, 0x5c, 0xf5, 0xe6, 0x06, 0xb0, 0xed, 0xe6, 0x06, 0xcd, 0x4d, 0xc8, 0x77, 0xa4, 0x45, 0x02, 0xe2, 0xfe, 0xff, 0xff, 0xff, 0x64, 0xe4, 0xc3, 0x77, 0xc2, 0xe1, 0xc3, 0x77, 0x64, 0x00, 0x22, 0x7c, 0x6d, 0x00, 0x00, 0x00, 0x68, 0x00, 0x22, 0x7c, 0x05, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xf0, 0xe6, 0xf1, 0xff, 0x02, 0x00, 0x00, 0x00, 0x8c, 0xed, 0xe6, 0x06, 0xf8, 0x0f, 0xea, 0x72, 0x4c, 0xfa, 0xc1, 0x26, 0x00, 0x08, 0x56, 0x22, 0xf0, 0xe6, 0xc1, 0x07, 0x20, 0xfa, 0xc1, 0x07, 0xf0, 0xe6, 0xc1, 0x07, 0x98, 0xed, 0xe6, 0x06, 0x07, 0x00, 0x00, 0x00, 0xf0, 0xae, 0xe6, 0x21, 0x0b, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x78, 0x00, 0x00, 0x00, 0x9c, 0xed, 0xe6, 0x06, 0xc0, 0xed, 0xe6, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x93, 0x03, 0xd0, 0x34, 0x98, 0x1c, 0xb8, 0x16, 0x34, 0x7c, 0x64, 0x00, 0x22, 0x7c, 0x07, 0x00, 0x02, 0x00, 0xa9, 0x1a, 0x10, 0x00, 0xb4, 0xed, 0xe6, 0x06, 0x21, 0x0b, 0xea, 0x72, 0x94, 0xed, 0xe6, 0x06, 0x40, 0x34, 0x98, 0x1c, 0x01, 0x00, 0x00, 0x00, 0xc8, 0x34, 0x98, 0x1c, 0xd0, 0xed, 0xe6, 0x06, 0xf3, 0xe0, 0xc3, 0x77, 0xd0, 0x34, 0x98, 0x1c, 0xb8, 0x1b, 0x4c, 0x03, 0xac, 0x42, 0xa1, 0x1c, 0xc8, 0x34, 0x98, 0x1c, 0xe4, 0xed, 0xe6, 0x06, 0xad, 0x14, 0x82, 0x76, 0x00, 0x00, 0x93, 0x03, 0x00, 0x00, 0x00, 0x00, 0xd0, 0x34, 0x98, 0x1c, 0x2c, 0xee, 0xe6, 0x06, 0x8a, 0x21, 0x34, 0x7c, 0x00, 0x00, 0x93, 0x03, 0x00, 0x00, 0x00, 0x00, 0x8f, 0x21, 0x34, 0x7c, 0xb8, 0x1b, 0x4c, 0x03, 0x2d, 0x0e, 0x4c, 0x03, 0xac, 0x42, 0xa1, 0x1c, 0xac, 0x1f, 0x4c, 0x03, 0x15, 0x1e, 0x27, 0x01, 0x44, 0xee, 0xe6, 0x06, 0xff, 0xff, 0xff, 0xff, 0xf8, 0xed, 0xe6, 0x06, 0x5c, 0x00, 0x00, 0x00, 0x48, 0xee, 0xe6, 0x06, 0x0d, 0x24, 0x34, 0x7c, 0xa8, 0xa3, 0x37, 0x7c, 0xff, 0xff, 0xff, 0xff, 0x8f, 0x21, 0x34, 0x7c, 0x36, 0x18, 0x17, 0x7c, 0xd0, 0x34, 0x98, 0x1c, 0x7a, 0xa7, 0x4b, 0x03, 0xd0, 0x34, 0x98, 0x1c, 0xe0, 0x34, 0x98, 0x1c, 0x00, 0x00, 0x00, 0x00, 0xec, 0xfe, 0xe6, 0x06, 0x18, 0xfa, 0x4b, 0x03, 0xff, 0xff, 0xff, 0xff, 0x17, 0xa8, 0x4b, 0x03, 0x70, 0xee, 0xe6, 0x06, 0x85, 0xee, 0xe6, 0x06, 0xb8, 0x1b, 0x4c, 0x03, 0x80, 0xfe, 0xe6, 0x06, 0x54, 0x42, 0xa1, 0x1c, 0x80, 0xe7, 0x93, 0x03, 0x70, 0x77, 0x6f, 0x72, 0x6b, 0x65, 0x72, 0x2e, 0x63, 0x70, 0x70, 0x20, 0x20, 0x20, 0x20, 0x2d, 0x20, 0x20, 0x36, 0x31, 0x20, 0x43, 0x48, 0x45, 0x43, 0x4b, 0x28, 0x43, 0x4d, 0x44, 0x2c, 0x20, 0x74, 0x68, 0x72, 0x65, 0x61, 0x64, 0x31, 0x37, 0x2c, 0x20, 0x42, 0x30, 0x30, 0x38, 0x39, 0x37, 0x34, 0x49, 0x30, 0x31, 0x38, 0x38, 0x31, 0x37, 0x29, 0x20, 0x46, 0x55, 0x4e, 0x43, 0x61, 0x38, 0x2c, 0x20, 0x52, 0x45, 0x53, 0x30, 0x30, 0x20, 0x6c, 0x65, 0x6e, 0x67, 0x74, 0x68, 0x28, 0x31, 0x30, 0x29, 0x0a, 0x00, 0x30, 0x29, 0x0a, 0x00, 0xe0, 0xc9, 0x1a, 0x07, 0x00, 0x00, 0x00, 0x00, 0x61, 0x00, 0x70, 0x00, 0x5f, 0x00, 0x73, 0x00, 0x65, 0x00, 0x74, 0x00, 0x5f, 0x00, 0x6f, 0x00, 0x70, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00, 0x5f, 0x00, 0x69, 0x00, 0x6e, 0x00, 0x76, 0x00, 0x69, 0x00, 0x74, 0x00, 0x61, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00, 0x04, 0x00, 0x00, 0x00, 0x48, 0x8b, 0xc9, 0x20, 0x03, 0x00, 0xcb, 0x00, 0x40, 0xfd, 0xe6, 0x06, 0x04, 0x00, 0x00, 0x00, 0x2c, 0xef, 0xe6, 0x06, 0x42, 0x35, 0xeb, 0x72, 0x48, 0x8b, 0xc9, 0x20, 0x40, 0xfd, 0xe6, 0x06, 0x06, 0x90, 0xc9, 0x20, 0x04, 0x00, 0x00, 0x00, 0x8c, 0xf3, 0xe6, 0x06, 0xe0, 0x23, 0xeb, 0x72, 0x48, 0x8b, 0xc9, 0x20, 0x40, 0xfd, 0xe6, 0x06, 0x06, 0x90, 0xc9, 0x20, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0xcb, 0x00, 0x04, 0x00, 0x00, 0x00, 0x48, 0x8b, 0xc9, 0x20, 0x78, 0x24, 0xeb, 0x72, 0x88, 0xef, 0xe6, 0x06, 0x40, 0xfd, 0xe6, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0xcb, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x90, 0xc9, 0x20, 0x00, 0x00, 0x26, 0x04, 0x04, 0x0f, 0xcb, 0x00, 0xa4, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8b, 0xc9, 0x20, 0xff, 0xff, 0x00, 0x05, 0x98, 0x0f, 0xac, 0x03, 0x06, 0x2f, 0x76, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x54, 0xf0, 0xe6, 0x06, 0xa4, 0x00, 0x00, 0x00, 0x78, 0x19, 0xb9, 0x07, 0x04, 0x00, 0x00, 0x00, 0xc4, 0xef, 0xe6, 0x06, 0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0x7f, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xe9, 0x03, 0x00, 0x00, 0x66, 0x00, 0x02, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x64, 0xe4, 0xc3, 0x77, 0x88, 0x66, 0x27, 0x93, 0x00, 0x00, 0x56, 0x22, 0xf4, 0x59, 0x56, 0x22, 0x00, 0x00, 0x56, 0x22, 0xf4, 0x59, 0x56, 0x22, 0x00, 0x00, 0x56, 0x22, 0x08, 0x00, 0xe3, 0x00, 0xf7, 0xa5, 0x00, 0x00, 0x08, 0x00, 0xe3, 0x00, 0xf0, 0xa5, 0x00, 0x00, 0x0d, 0x00, 0x00, 0x00, 0xbc, 0xf8, 0xc2, 0x77, 0x09, 0x00, 0x02, 0x00, 0xf7, 0xa5, 0x00, 0x00, 0xbe, 0x13, 0xcb, 0x00, 0xf0, 0xff, 0x00, 0x00, 0x58, 0x03, 0xe2, 0x21, 0x06, 0x90, 0xc9, 0x20, 0x00, 0x07, 0x00, 0x00, 0x5d, 0x05, 0xeb, 0x72, 0x00, 0x00, 0x00, 0x00, 0x38, 0xca, 0x56, 0x22, 0x48, 0x00, 0xe2, 0x21, 0x88, 0x02, 0xe2, 0x21, 0xf0, 0x8f, 0xe5, 0x21, 0x50, 0x22, 0xe2, 0x21, 0x06, 0x90, 0xc9, 0x20, 0x30, 0xf0, 0xe6, 0x06, 0x00, 0x00, 0x00, 0x00, 0x30, 0xf2, 0xe6, 0x06, 0xcd, 0x4d, 0xc8, 0x77, 0xa4, 0x45, 0x02, 0xe2, 0xfe, 0xff, 0xff, 0xff, 0x64, 0xe4, 0xc3, 0x77, 0xc2, 0xe1, 0xc3, 0x77, 0x00, 0x00, 0x56, 0x22, 0x38, 0x01, 0x00, 0x00, 0x60, 0x7f, 0xe5, 0x21, 0x38, 0x01, 0x00, 0x00, 0x60, 0x7f, 0xe5, 0x21, 0x94, 0x04, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x2a, 0x43, 0x76, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x56, 0x22, 0x68, 0x7f, 0xe5, 0x21, 0x00, 0x00, 0x56, 0x22, 0x68, 0x7f, 0xe5, 0x21, 0x0b, 0x00, 0x2b, 0x00, 0x97, 0xe6, 0x04, 0x00, 0x0b, 0x00, 0x2b, 0x00, 0x8c, 0xe6, 0x04, 0x00, 0x04, 0x00, 0x00, 0x04, 0x50, 0x7f, 0xe5, 0x21, 0x01, 0x00, 0x00, 0x00, 0x60, 0x7f, 0xe5, 0x21, 0x28, 0xf1, 0xe6, 0x06, 0xf3, 0xe0, 0xc3, 0x77, 0x38, 0x01, 0x00, 0x00, 0x00, 0x00, 0x56, 0x22, 0xf0, 0x8f, 0xe5, 0x21, 0x60, 0x7f, 0xe5, 0x21, 0x40, 0xf2, 0xe6, 0x06, 0x63, 0xc9, 0xc4, 0x77, 0x00, 0x00, 0x56, 0x22, 0x0a, 0x00, 0x00, 0x00, 0x77, 0xc9, 0xc4, 0x77, 0x7c, 0x64, 0x27, 0x93, 0x00, 0x00, 0x56, 0x22, 0x00, 0x07, 0x00, 0x00, 0x68, 0x7f, 0xe5, 0x21, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0xf1, 0xe6, 0x06, 0x1b, 0x00, 0x00, 0x00, 0x50, 0x90, 0x2d, 0x1e, 0x04, 0x00, 0x00, 0x00, 0x50, 0x90, 0x2d, 0x1e, 0x04, 0x00, 0x00, 0x00, 0xc4, 0x97, 0x1a, 0x07, 0x04, 0x00, 0x00, 0x00, 0x50, 0x01, 0x1a, 0x07, 0x03, 0x00, 0x00, 0x03, 0x0f, 0x00, 0x00, 0x00, 0xe4, 0xf2, 0xe6, 0x06, 0x21, 0xf9, 0xc2, 0x77, 0xcf, 0x2e, 0x76, 0x74, 0xc0, 0x2e, 0x00, 0x00, 0x90, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xc4, 0xf1, 0xe6, 0x06, 0x47, 0x20, 0x01, 0x00, 0xe0, 0xf1, 0xe6, 0x06, 0xa4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0x50, 0x90, 0x2d, 0x1e, 0x06, 0x2f, 0x76, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0xf2, 0xe6, 0x06, 0xa4, 0x00, 0x00, 0x00, 0x78, 0x19, 0xb9, 0x07, 0x04, 0x00, 0x00, 0x00, 0xe0, 0xf1, 0xe6, 0x06, 0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0x7f, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xe9, 0x03, 0x00, 0x00, 0x66, 0x00, 0x02, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa0, 0x1a, 0x0f, 0xe7, 0x8b, 0xab, 0xcf, 0x11, 0x8c, 0xa3, 0x00, 0x80, 0x5f, 0x48, 0xa1, 0x92, 0x04, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x02, 0x00, 0xf8, 0x80, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa8, 0xf2, 0xe6, 0x06, 0x0d, 0x00, 0x00, 0x00, 0xbc, 0xf8, 0xc2, 0x77, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x68, 0xa5, 0x56, 0x22, 0x26, 0x00, 0x00, 0x00, 0x90, 0x01, 0x00, 0x00, 0xb8, 0xf3, 0xe6, 0x06, 0x0f, 0x00, 0x00, 0x00, 0xe0, 0xf2, 0xe6, 0x06, 0xd1, 0xf8, 0xc2, 0x77, 0xcd, 0x17, 0x76, 0x74, 0x90, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xc8, 0xf2, 0xe6, 0x06, 0x90, 0x01, 0x00, 0x00, 0x03, 0x01, 0x00, 0x00, 0x03, 0x01, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x00, 0x5c, 0xf3, 0xe6, 0x06, 0xc0, 0xb4, 0xb3, 0xff, 0xff, 0xff, 0xff, 0xff, 0xc0, 0x2e, 0x00, 0x00, 0x78, 0x19, 0xb9, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5c, 0xf3, 0xe6, 0x06, 0x96, 0x76, 0x76, 0x74, 0x90, 0x01, 0x00, 0x00, 0xc0, 0x2e, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x2a, 0x43, 0x76, 0x74, 0xb2, 0xbf, 0xd9, 0xfe, 0x60, 0x08, 0x56, 0x22, 0xb0, 0x8d, 0x20, 0x1e, 0x00, 0x00, 0x00, 0x00, 0x94, 0xf3, 0xe6, 0x06, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x56, 0x22, 0x60, 0xa5, 0x56, 0x22, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x98, 0xa6, 0x56, 0x22, 0xc8, 0xff, 0x56, 0x22, 0xc4, 0x00, 0x56, 0x22, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x56, 0x22, 0x00, 0x04, 0x00, 0x00, 0x60, 0xa5, 0x56, 0x22, 0x30, 0xf4, 0xe6, 0x06, 0x61, 0x30, 0xc4, 0x77, 0x38, 0x01, 0x56, 0x22, 0x3d, 0x30, 0xc4, 0x77, 0x0c, 0x62, 0x27, 0x93, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x56, 0x22, 0x68, 0xa5, 0x56, 0x22, 0x9f, 0x68, 0x95, 0x76, 0xc0, 0x2e, 0x00, 0x00, 0x94, 0xf3, 0xe6, 0x06, 0x01, 0x00, 0x00, 0x00, 0xac, 0xf3, 0xe6, 0x06, 0xb8, 0xf3, 0xe6, 0x06, 0xb8, 0xb6, 0x56, 0x22, 0x50, 0x01, 0x56, 0x22, 0xb0, 0x8d, 0x20, 0x1e, 0x00, 0x00, 0x56, 0x22, 0x50, 0x01, 0x56, 0x22, 0xc4, 0x97, 0x56, 0x22, 0x1c, 0x9b, 0x56, 0x22, 0x50, 0x01, 0x56, 0x22, 0x04, 0x00, 0x00, 0x04, 0xf0, 0x91, 0x7e, 0x00, 0x74, 0x00, 0x00, 0x74, 0x2a, 0x00, 0x00, 0x00, 0xc7, 0x16, 0xdc, 0x73, 0x3f, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x12, 0x17, 0xdc, 0x73, 0x2a, 0x00, 0x00, 0x00, 0x74, 0x00, 0x00, 0x00, 0xf0, 0xe6, 0xc1, 0x07, 0x9a, 0x01, 0x00, 0x9b, 0x2a, 0x00, 0x00, 0x00, 0x30, 0x75, 0x00, 0x00, 0xd8, 0xcf, 0xe5, 0x21, 0x54, 0x9c, 0x56, 0x22, 0xff, 0xff, 0xff, 0xff, 0x08, 0x00, 0x00, 0x00, 0x80, 0x01, 0x56, 0x22, 0x00, 0x00, 0x00, 0x00, 0x50, 0x01, 0x56, 0x22, 0xff, 0xff, 0xff, 0x7f, 0x3f, 0x00, 0x00, 0x00, 0x68, 0xa5, 0x56, 0x22, 0xb8, 0xb6, 0x56, 0x22, 0xff, 0xff, 0xff, 0x7f, 0x2a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x50, 0x01, 0x00, 0x00, 0xc4, 0x97, 0x01, 0x01, 0x50, 0xf3, 0xe6, 0x06, 0x5b, 0x4d, 0xea, 0x72, 0xec, 0xfe, 0xe6, 0x06, 0xcd, 0x4d, 0xc8, 0x77, 0xac, 0x5f, 0x02, 0xe2, 0xfe, 0xff, 0xff, 0xff, 0x3d, 0x30, 0xc4, 0x77, 0x35, 0x2c, 0xc4, 0x77, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b, 0x00, 0x18, 0x7f, 0x73, 0x03, 0xc0, 0xae, 0xe6, 0x21, 0x54, 0xf4, 0xe6, 0x06, 0x04, 0x00, 0x67, 0x00, 0x61, 0xe0, 0x32, 0xbe, 0x03, 0x96, 0x00, 0x00, 0xdb, 0x3e, 0xc4, 0x77, 0x38, 0x42, 0xd1, 0x77, 0x00, 0x7f, 0x73, 0x03, 0x01, 0x00, 0x00, 0x00, 0x10, 0x7f, 0x73, 0x03, 0x84, 0xf4, 0xe6, 0x06, 0xf3, 0xe0, 0xc3, 0x77, 0x18, 0x7f, 0x73, 0x03, 0x3c, 0x7f, 0x73, 0x03, 0x02, 0x00, 0x00, 0x00, 0x10, 0x7f, 0x73, 0x03, 0xd0, 0xf4, 0xe6, 0x06, 0xcd, 0x98, 0x0c, 0x77, 0x00, 0x00, 0x3b, 0x00, 0x00, 0x00, 0x00, 0x00, 0xda, 0x98, 0x0c, 0x77, 0xf8, 0xdc, 0xfb, 0xfe, 0x3c, 0x7f, 0x73, 0x03, 0x18, 0x7f, 0x73, 0x03, 0x02, 0x00, 0x00, 0x00, 0x14, 0x8b, 0x20, 0x74, 0xd0, 0xbe, 0xb4, 0x03, 0x80, 0x00, 0x71, 0x03, 0x80, 0x00, 0x71, 0x03, 0x20, 0x78, 0x70, 0x03, 0xac, 0x00, 0x71, 0x03, 0xcc, 0xf4, 0xe6, 0x06, 0xc7, 0x70, 0x20, 0x74, 0xa4, 0x12, 0x76, 0x20, 0xe8, 0xf4, 0xe6, 0x06, 0xac, 0x00, 0x71, 0x03, 0xe8, 0xf4, 0xe6, 0x06, 0xfd, 0x9e, 0x20, 0x74, 0x40, 0x03, 0x71, 0x03, 0x18, 0x7f, 0x73, 0x03, 0x00, 0x00, 0x00, 0x00, 0x08, 0xf5, 0xe6, 0x06, 0x99, 0x97, 0x20, 0x74, 0x80, 0x00, 0x71, 0x03, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x18, 0x7f, 0x73, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2c, 0xf5, 0xe6, 0x06, 0x2c, 0xf5, 0xe6, 0x06, 0x82, 0xa2, 0x25, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe8, 0x20, 0x8d, 0x1c, 0xe8, 0x20, 0x8d, 0x1c, 0xb2, 0x42, 0xa1, 0x1c, 0xc0, 0x31, 0x93, 0x03, 0x43, 0x87, 0x3f, 0xf8, 0xca, 0xd5, 0x4b, 0x03, 0xb2, 0x42, 0xa1, 0x1c, 0xe6, 0x15, 0x4b, 0x03, 0x54, 0x42, 0xa1, 0x1c, 0x07, 0x00, 0x00, 0x00, 0xe0, 0x31, 0x93, 0x03, 0xd4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x84, 0xf5, 0xe6, 0x06, 0x87, 0x91, 0x4b, 0x03, 0x54, 0x42, 0xa1, 0x1c, 0xd4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x84, 0xf5, 0xe6, 0x06, 0x54, 0x42, 0xa1, 0x1c, 0x80, 0xe7, 0x93, 0x03, 0x40, 0x42, 0xa1, 0x1c, 0x99, 0x91, 0x4b, 0x03, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00};
            _ = ServerHolder.ServerInstance.SendPacket(data, stream);

        }

        public static void TO_LOBBY(ushort type, ushort action, byte[] body, NetworkStream stream) {
            action++;
            ToLobbyReq request = ToLobbyReq.Deserialize(body);
            Console.WriteLine($"To Lobby");

            ToLobbyRes response = new ToLobbyRes();
            response.Type = 528;
            response.Action = action;

            response.Result = 1;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
        } 


    }
}
