using System;
using Gemnet.Network.Header;
using Gemnet.Network.Packets;
using Gemnet.Packets.Login;
using Gemnet.Persistence.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using static Program;


namespace Gemnet.PacketProcessors
{
    internal class Login
    {
        public static void VersionCheck(ushort type, ushort action)
        {

            //Console.WriteLine("[Server] Version Check Successful");
            action++;
            _ = ServerHolder.ServerInstance.SendPacket(type, 0x6, action);


        }

        public static void CredentialCheck(ushort type, ushort action, byte[] body)
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


            if (LoginQuery == null)
            {
                Console.WriteLine("Username Or Password Incorrect.");

                LoginFailRes response = new LoginFailRes();

                response.Type = 528;
                response.Action = 645;

                response.Error = "Email or Password is Incorrect.";
                response.Code = 29374;

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize());


            } else {

                LoginRes response = new LoginRes();

                response.Type = type;
                response.Action = action;
                response.UserID = LoginQuery.UUID;
                response.IGN = LoginQuery.IGN;
                response.Exp = LoginQuery.EXP;
                response.Carats = LoginQuery.Carats;
                response.GUID = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
                response.Token = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234";
                response.ForumName = LoginQuery.ForumName;

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize());
            }

           
        }

        public static void ServerTime(ushort type, ushort action)
        {
            action++;
            DateTime now = DateTime.Now;
            now.ToUniversalTime();
            byte[] timeArray = BitConverter.GetBytes(now.Ticks);
            string time = now.ToString();

            ServerTime serverTime = new ServerTime();
            serverTime.Type = type;
            serverTime.Action = action;

            serverTime.Time = timeArray;


            Console.WriteLine($"[Server Time : {time}] Data={BitConverter.ToString(timeArray)}");

            _ = ServerHolder.ServerInstance.SendPacket(serverTime.Serialize());

        }

        public static void CashUnknown(ushort type, ushort action)
        {
            action++;
            byte[] data = { 0x01, 0x00 };
            _ = ServerHolder.ServerInstance.SendPacket(type, 0x8, action, data);


        }
        public static void GetBuddyList(ushort type, ushort action)
        {
            action++;
            Console.WriteLine($"Get Buddy List");

            _ = ServerHolder.ServerInstance.SendPacket(type, 0x06, action);
        }

        public static void TO_LOBBY(ushort type, ushort action, byte[] body) {
            action++;
            ToLobbyReq request = ToLobbyReq.Deserialize(body);
            Console.WriteLine($"To Lobby");

            ToLobbyRes response = new ToLobbyRes();
            response.Type = 528;
            response.Action = action;

            response.Result = 1;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize());
        } 

    }
}
