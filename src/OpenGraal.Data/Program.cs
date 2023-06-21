
using OpenGraal.Data;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

await LevelManager.LoadAsync(@"C:\Users\dhumm\Desktop\Zodiac");

var level = LevelManager.GetByName("onlinestartlocal.nw");

Console.WriteLine("Hello, World!");
