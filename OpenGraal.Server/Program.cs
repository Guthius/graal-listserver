using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenGraal.Net;
using OpenGraal.Server.Game;
using OpenGraal.Server.Lobby;
using OpenGraal.Server.Services.Accounts;
using OpenGraal.Server.World;
using Serilog;
using Serilog.Events;

Console.Title = "OpenGraal Server";

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(" ------------------");
Console.WriteLine("  OpenGraal Server ");
Console.WriteLine(" ------------------");
Console.ResetColor();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting server...");

try
{
    await Host.CreateDefaultBuilder(args)
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<AccountService>();
            
            services.AddSingleton<LobbyService>();
            services.AddScoped<LobbyProtocol>();
            services.AddHostedService<Service<LobbyProtocol>>();
            
            services.AddSingleton(typeof(ConnectionManager<>));
            services.AddSingleton<IWorld, WorldService>();
            services.AddHostedService<WorldService>();

            services.AddScoped<GameProtocol>();
            services.AddHostedService<Service<GameProtocol>>();
        })
        .UseSerilog()
        .RunConsoleAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to start server");
}