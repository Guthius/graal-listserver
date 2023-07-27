using System.Collections.Concurrent;
using Serilog;

namespace OpenGraal.Data;

public static class LevelManager
{
    private static readonly ConcurrentDictionary<string, Level> Levels = new();

    public static async Task LoadAsync(string path)
    {
        await Task.WhenAll(Directory
            .GetFiles(path, "*.nw", SearchOption.AllDirectories)
            .Select(async x =>
            {
                var fileName = Path.GetFileName(x).ToLowerInvariant();

                var level = Level.LoadNw(x);
                if (level is null)
                {
                    return;
                }
                
                Levels[fileName] = level;
            }));

        Log.Information("Succesfully loaded {LevelCount} levels", Levels.Count);
    }

    public static Level? GetByName(string fileName)
    {
        fileName = fileName.ToLowerInvariant();
        
        return Levels.TryGetValue(fileName, out var level) ? level : default;
    }
}