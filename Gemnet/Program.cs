using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gemnet;
using Gemnet.Packets;
using Gemnet.Settings;
using Gemnet.Persistence;
using Gemnet.Persistence.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace Gemnet
{
    internal class Program
    {
        public static class ServerHolder
        {
            public static Server ServerInstance { get; set; }
            public static Database DatabaseInstance { get; set; }
            public static PlayerManager _playerManager { get; set; }
            public static GameManager _gameManager { get; set; }
        }

        public static async Task Main(string[] args)
        {
            // Create host builder with dependency injection
            var host = CreateHostBuilder(args).Build();
            
            try
            {
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Application terminated unexpectedly");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure logging
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.AddDebug();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });

                    // Configure settings
                    var settings = Settings.Settings.ImportSettings("./settings.json");
                    if (settings == null)
                    {
                        throw new InvalidOperationException("Could not import settings from 'settings.json'");
                    }
                    services.AddSingleton(settings);

                    // Configure database
                    Database.ConnectionString = settings.DBConnectionString;
                    var database = new Database();
                    database.Connect();

                    if (!database.IsConnected())
                    {
                        throw new InvalidOperationException("Could not connect to the database");
                    }

                    DBGeneral.CheckAndCreateDatabase(database);
                    services.AddSingleton(database);

                    // Configure managers
                    var playerManager = new PlayerManager(database);
                    var gameManager = new GameManager(playerManager);
                    
                    services.AddSingleton(playerManager);
                    services.AddSingleton(gameManager);

                    // Configure server
                    services.AddSingleton<Server>();

                    // Store references for backward compatibility
                    ServerHolder.DatabaseInstance = database;
                    ServerHolder._playerManager = playerManager;
                    ServerHolder._gameManager = gameManager;
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("settings.json", optional: false);
                })
                .UseConsoleLifetime()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ServerHostedService>();
                });
    }

    public class ServerHostedService : IHostedService
    {
        private readonly ILogger<ServerHostedService> _logger;
        private readonly Server _server;
        private readonly Settings.Settings.SData _settings;

        public ServerHostedService(
            ILogger<ServerHostedService> logger,
            Server server,
            Settings.Settings.SData settings)
        {
            _logger = logger;
            _server = server;
            _settings = settings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Gemnet server...");
            
            try
            {
                // Store server reference for backward compatibility
                Program.ServerHolder.ServerInstance = _server;
                
                await _server.StartAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start server");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Gemnet server...");
            
            try
            {
                await _server.StopAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping server");
            }
        }
    }
}
