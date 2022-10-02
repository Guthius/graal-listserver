using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenGraal.Net;
using OpenGraal.Server;
using OpenGraal.Server.Database;
using Serilog;
using Serilog.Events;

Console.Title = "Graal 2.1.5 Server";

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(" --------------------");
Console.WriteLine("  Graal 2.1.5 Server ");
Console.WriteLine(" --------------------");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Gray;

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
            services.AddSingleton<IDatabase, JsonDatabase>();
            services.AddScoped<LobbyProtocol>();
            services.AddSingleton<SessionManager<LobbyProtocol>>();
            services.AddHostedService<Server<LobbyProtocol>>();
        })
        .UseSerilog()
        .RunConsoleAsync();
}
catch (Exception ex)
{
    Log.Error(ex, "Failed to start server");
}

Console.WriteLine();
Console.WriteLine("Press ANY key to exit");
Console.ReadKey();
