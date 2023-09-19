using Gemnet.Network.Packets;
using Gemnet.Persistence;
using Gemnet.Persistence.Models;
using GemnetCS.Network.Packets;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Program;

namespace Gemnet.PacketProcessors
{
    internal class Inventory
    {
        public static void GetCash(ushort type, ushort action, byte[] body, NetworkStream stream)
        {

            action++;

            GetCashRes response = new GetCashRes();

            response.Type = type;
            response.Action = action;

            response.UserID = 1;
            response.Astros = 1000000;
            response.Medals = 1000000;

            Console.WriteLine($"Get Cash: Astros={response.Astros}, Medals={response.Medals}");

            byte[] packet = response.Serialize();

            _ = ServerHolder.ServerInstance.SendPacket(packet, stream);
        }

        public static void BuyItem(ushort type, ushort action, byte[] body, NetworkStream stream) 
        {
            action++;

            BuyItemReq request = BuyItemReq.Deserialize(body);

            Console.WriteLine($"Buying ItemID={request.ItemID}");

            BuyItemRes response = new BuyItemRes();


            var CashQueryCarats = ServerHolder.DatabaseInstance.SelectFirst<ModelAccount>(ModelAccount.QueryCashCarats, new
            {
                ID = 1,

            });

            if (CashQueryCarats != null)
            {
                if (CashQueryCarats.Carats > 0)
                {
                    response.Type = type;
                    response.Action = action;

                    ServerHolder.DatabaseInstance.Execute(ModelInventory.InsertItem, new
                    {
                        OID = 1,
                        ID = request.ItemID
                    });

                    var ServerID = ServerHolder.DatabaseInstance.SelectFirst<ModelInventory>(ModelInventory.GetServerID, new
                    {
                        OID = 1,

                    });

                    response.ServerID = ServerID.ServerID;
                    response.Carats = 5000000;

                    _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

                }
            }

        }

        public static void OpenBox(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            OpenBoxReq request = OpenBoxReq.Deserialize(body);
            Console.WriteLine($"Opening Box with ServerID={request.ServerID}");

            ServerHolder.DatabaseInstance.Execute(ModelInventory.DeleteItem, new
            {
                SID = request.ServerID
            });

            OpenBoxRes response = new OpenBoxRes();

            int itemid = 2070006;
            int itemend = 3069;

            ServerHolder.DatabaseInstance.Execute(ModelInventory.InsertItem, new
            {
                OID = 1,
                ID = itemid
            });

            var ServerID = ServerHolder.DatabaseInstance.SelectFirst<ModelInventory>(ModelInventory.GetServerID, new
            {
                OID = 1,

            });

            response.Type = type;
            response.Action = action;

            response.ServerID = ServerID.ServerID;
            response.ItemID = itemid;
            response.ItemEnd = itemend;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

        }

    }
}
