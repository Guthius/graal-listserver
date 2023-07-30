using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenGraal.Net;
using OpenGraal.Server.Game;
using OpenGraal.Server.Game.Players;
using OpenGraal.Server.Game.Worlds;
using OpenGraal.Server.Lobby;
using OpenGraal.Server.Services.Accounts;
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
    var hostBuilder = Host.CreateApplicationBuilder(args);

    hostBuilder.Services.AddSingleton<AccountService>();
    hostBuilder.Services.AddSingleton<LobbyService>();
    hostBuilder.Services.AddSingleton<World>();

    hostBuilder.Services.AddHostedService<World>();

    hostBuilder.Services.AddGameService<LobbyUser, LobbyParser>();
    hostBuilder.Services.AddGameService<GameUser, GameCommandParser>();
    hostBuilder.Services.AddSerilog();

    var host = hostBuilder.Build();

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to start server");
}