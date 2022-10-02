using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace OpenGraal.Server.Database;

internal sealed class JsonDatabase : IDatabase
{
    private readonly string _path;
    private readonly string _accountsPath;
    
    /// <summary>
    /// Represents an account.
    /// </summary>
    private class Account
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDatabase"/> class.
    /// </summary>
    /// <param name="configuration">The database configuration.</param>
    public JsonDatabase(IConfiguration configuration)
    {
        _path = configuration["DataPath"];
        if (string.IsNullOrEmpty(_path))
        {
            throw new Exception(
                "No path for the database was specified. " +
                "Please configure 'DataPath' and restart.");
        }

        try
        {
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            _accountsPath = Path.Combine(_path, "Accounts");
            if (!Directory.Exists(_accountsPath))
            {
                Directory.CreateDirectory(_accountsPath);
            }

            Log.Information("Database initialized succesfully");
        }
        catch (Exception ex)
        {
            throw new Exception("Error creating data directory structure.", ex);
        }
    }

    /// <summary>
    /// Gets the list of servers as a single string.
    /// </summary>
    /// <returns></returns>
    public List<ServerInfo> GetServers()
    {
        var serversPath = Path.Combine(_path, "servers.json");
        if (!File.Exists(serversPath))
        {
            return new List<ServerInfo>();
        }

        var json = File.ReadAllText(serversPath);

        var servers = JsonSerializer.Deserialize<List<ServerInfo>>(json);

        return servers ?? new List<ServerInfo>();
    }

    /// <summary>
    /// Checks whether an account with the specified credentials exists.
    /// </summary>
    /// <param name="name">The name of the account.</param>
    /// <param name="password">The password.</param>
    /// <returns>True if the account exists and the password is valid; otherwise, false.</returns>
    public bool AccountExists(string name, string password)
    {
        var accountPath = Path.Combine(_accountsPath, name + ".json");
        if (!File.Exists(accountPath))
        {
            return false;
        }

        var accountJson = File.ReadAllText(accountPath);
        var account = JsonSerializer.Deserialize<Account>(accountJson);

        return account?.Password is not null && account.Password.Equals(password);
    }
}