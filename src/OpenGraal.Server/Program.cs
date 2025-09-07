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
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSingleton<AccountService>();
    builder.Services.AddSingleton<LobbyManager>();
    builder.Services.AddScoped<LobbyProtocol>();
    builder.Services.AddSingleton(typeof(ConnectionManager<>));
    builder.Services.AddHostedService<Service<LobbyProtocol>>();
    builder.Services.AddSerilog();

    var host = builder.Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to start server");
}