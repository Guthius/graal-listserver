using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Listserver
{
    public class JsonDatabase : IDatabase
    {
        private readonly string path;
        private readonly string accountsPath;

        /// <summary>
        /// Represents a server.
        /// </summary>
        class Server
        {
            public bool Premium { get; set; }

            public string Name { get; set; }

            public string Language { get; set; }

            public string Description { get; set; }

            public string Url { get; set; }

            public string Version { get; set; }

            public int Players { get; set; }

            public string IP { get; set; }

            public int Port { get; set; }
        }

        /// <summary>
        /// Represents an account.
        /// </summary>
        class Account
        {
            public string Name { get; set; }

            public string Password { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDatabase"/> class.
        /// </summary>
        /// <param name="configuration">The database configuration.</param>
        public JsonDatabase(IConfiguration configuration)
        {
            path = configuration["DataPath"];
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("No path for the database was specified. please configure 'DataPath' and restart.");
            }

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                accountsPath = Path.Combine(path, "Accounts");
                if (!Directory.Exists(accountsPath))
                {
                    Directory.CreateDirectory(accountsPath);
                }

                Log.Information("Database initialized succesfully.");
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
            var serversPath = Path.Combine(path, "servers.json");
            if (!File.Exists(serversPath))
            {
                return Convert.ToString((char)32);
            }

            var json = File.ReadAllText(serversPath);

            var servers = JsonSerializer.Deserialize<List<Server>>(json);
            var serverList = "";

            foreach (var server in servers)
            {
                var name = server.Name ?? "";
                if (server.Premium)
                {
                    name = "P " + name;
                }

                serverList += (char)(32 + 8);
                serverList += Pack(name);
                serverList += Pack(server.Language ?? "");
                serverList += Pack(server.Description ?? "");
                serverList += Pack(server.Url ?? "");
                serverList += Pack(server.Version ?? "");
                serverList += Pack(server.Players.ToString());
                serverList += Pack(server.IP ?? "");
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
            var accountPath = Path.Combine(accountsPath, name + ".json");
            if (!File.Exists(accountPath))
            {
                return false;
            }

            var accountJson = File.ReadAllText(accountPath);
            var account = JsonSerializer.Deserialize<Account>(accountJson);

            if (account.Password is not null && account.Password.Equals(password))
            {
                return true;
            }

            return false;
        }
    }
}