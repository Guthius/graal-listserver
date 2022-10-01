using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace OpenGraal.Server.Database;

public class JsonDatabase : IDatabase
{
    private readonly string _path;
    private readonly string _accountsPath;

    /// <summary>
    /// Represents a server.
    /// </summary>
    private class Server
    {
        public bool Premium { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int Players { get; set; }
        public string Ip { get; set; } = string.Empty;
        public int Port { get; set; }
    }

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
            throw new Exception("No path for the database was specified. please configure 'DataPath' and restart.");
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
    /// Pack a string using graal encoding.
    /// </summary>
    /// <param name="data">The data to encode.</param>
    /// <returns></returns>
    private static string Pack(string data) => Convert.ToString((char)(data.Length + 32)) + data;

    /// <summary>
    /// Gets the list of servers as a single string.
    /// </summary>
    /// <returns></returns>
    public string GetServers()
    {
        var serversPath = Path.Combine(_path, "servers.json");
        if (!File.Exists(serversPath))
        {
            return Convert.ToString((char)32);
        }

        var json = File.ReadAllText(serversPath);

        var servers = JsonSerializer.Deserialize<List<Server>>(json);
        var serverList = "";

        Debug.Assert(servers != null, nameof(servers) + " != null");
            
        foreach (var server in servers)
        {
            var name = server.Name;
            if (server.Premium)
            {
                name = "P " + name;
            }

            serverList += (char)(32 + 8);
            serverList += Pack(name);
            serverList += Pack(server.Language);
            serverList += Pack(server.Description);
            serverList += Pack(server.Url);
            serverList += Pack(server.Version);
            serverList += Pack(server.Players.ToString());
            serverList += Pack(server.Ip);
            serverList += Pack(server.Port.ToString());
        }

        return Convert.ToString((char)(servers.Count + 32)) + serverList;
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