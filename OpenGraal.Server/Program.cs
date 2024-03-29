using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenGraal.Net;
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
    .CreateBootstrapLogger();

Log.Information("Starting server...");

try
{
    await Host.CreateDefaultBuilder(args)
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<AccountService>();
            services.AddSingleton<LobbyManager>();
            services.AddScoped<LobbyProtocol>();
            services.AddSingleton(typeof(ConnectionManager<>));
            services.AddHostedService<Service<LobbyProtocol>>();
        })
        .UseSerilog()
        .RunConsoleAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to start server");
}