using Listserver;
using Listserver.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Console.Title = "Graal 2.1.5 List Server";

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(" -------------------------");
Console.WriteLine("  Graal 2.1.5 List Server ");
Console.WriteLine(" -------------------------\n");
Console.ForegroundColor = ConsoleColor.Gray;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .CreateLogger();

IHostBuilder CreateHostBuilder(string[] args)
    => Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(options =>
            {
                options.ClearProviders();
                options.AddSerilog();
            });

            services.Configure<ServerSettings>(context.Configuration);
            services.AddSingleton<IDatabase, JsonDatabase>();
            services.AddHostedService<Server>();
        });

Log.Information("Starting server...");

try
{
    await CreateHostBuilder(args).RunConsoleAsync();
}
catch (Exception ex)
{
    Log.Error(ex, "Failed to start server");
}

Console.WriteLine();
Console.WriteLine("Press ANY key to exit");
Console.ReadKey();
