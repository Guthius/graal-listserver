using JetBrains.Annotations;

namespace OpenGraal.Server.Game.Files;

public sealed class FileManager
{
    private readonly string _basePath;

    private sealed record Folder(FileCategory Category, string Path, string SearchPattern);

    private readonly List<Folder> _folders = new();
    private readonly Dictionary<string, File> _files = new();

    public FileManager(string basePath)
    {
        _basePath = basePath;
    }
    
    public void AddFolder(FileCategory category, string path)
    {
        string searchPatterh;

        var pos = path.LastIndexOf('/');
        if (pos != -1)
        {
            searchPatterh = path[(pos + 1)..];

            path = path[..pos];
        }
        else
        {
            searchPatterh = path;
            
            path = string.Empty;
        }

        if (!string.IsNullOrEmpty(_basePath))
        {
            path = Path.Combine(_basePath, path);
        }
        
        var folder = new Folder(category, path, searchPatterh);

        _folders.Add(folder);

        LoadFiles(
            folder.Category,
            folder.Path,
            folder.SearchPattern,
            true);
    }

    private void LoadFiles(FileCategory category, string path, string searchPattern, bool recursive)
    {
        if (!Directory.Exists(path))
        {
            return;
        }
        
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        var files = Directory.GetFiles(path, searchPattern, searchOption);

        foreach (var filePath in files)
        {
            var fileName = Path
                .GetFileName(filePath)
                .ToLowerInvariant();

            if (_files.ContainsKey(fileName))
            {
                continue;
            }
            
            var fileInfo = new FileInfo(filePath);
            var file = new File(category, filePath, fileInfo.LastWriteTimeUtc);
            
            _files[fileName] = file;
        }
    }

    public void Reload()
    {
        _files.Clear();

        foreach (var folder in _folders)
        {
            LoadFiles(
                folder.Category,
                folder.Path,
                folder.SearchPattern,
                true);
        }
    }

    [Pure]
    public File? GetFile(string fileName)
    {
        fileName = fileName.ToLowerInvariant();

        return _files.TryGetValue(fileName, out var file) ? file : null;
    }

    [Pure]
    public File? GetFile(string fileName, FileCategory category)
    {
        fileName = fileName.ToLowerInvariant();
        
        if (_files.TryGetValue(fileName, out var file) && (file.Category & category) > 0)
        {
            return file;
        }

        return null;
    }
}