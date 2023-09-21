using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SendPacket;
using Gemnet;
using Gemnet.Packets;
using System.Runtime;
using Microsoft.VisualBasic;
using Gemnet.Settings;
using Gemnet.Persistence;
using Gemnet.Persistence.Models;

    internal class Program
    {

        private static DateTime m_lastTickUpdate = DateTime.MinValue;
        private static Settings.SData m_settings = null;

        public static class ServerHolder
        {
            public static Server ServerInstance { get; set; }
            public static Database DatabaseInstance { get; set; }

        }


        public static async Task Main(string[] args)
        {

            Console.WriteLine("Importing settings from 'settings.json'...");
            m_settings = Settings.ImportSettings("./settings.json");

            if (m_settings == null)
            {
                Console.WriteLine("Could not import settings.");
                return;
            }

            Console.WriteLine("Connecting to database...");
            Database.ConnectionString = m_settings.DBConnectionString;

            var database = new Database();
            database.Connect();

            if (!database.IsConnected())
            {
                Console.WriteLine("Could not connect to the database.");
                return;
            }

            DBGeneral.CheckAndCreateDatabase(database);


            Server server = new Server(IPAddress.Any, m_settings.Port);

            ServerHolder.ServerInstance = server;
            ServerHolder.DatabaseInstance = database;

            await server.Start();


    }
    }


