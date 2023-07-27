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
    .MinimumLevel.Verbose()
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
    hostBuilder.Services.AddSingleton<IWorld, WorldService>();
            
    hostBuilder.Services.AddHostedService<WorldService>();
            
    hostBuilder.Services.AddGameService<LobbyUser, LobbyParser>();
    hostBuilder.Services.AddGameService<GameUser, GameParser>();
    hostBuilder.Services.AddSerilog();

    var host = hostBuilder.Build();
    
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to start server");
}