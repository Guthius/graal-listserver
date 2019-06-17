using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Listserver
{
    public class JsonDatabase : IDatabase
    {
        readonly string path;
        readonly string accountsPath;
        readonly JsonSerializer serializer = new JsonSerializer();

        /// <summary>
        /// Represents a server.
        /// </summary>
        class Server
        {
            [JsonProperty("premium")]
            public bool Premium { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("language")]
            public string Language { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("players")]
            public int Players { get; set; }

            [JsonProperty("ip")]
            public string IP { get; set; }

            [JsonProperty("port")]
            public int Port { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDatabase"/> class.
        /// </summary>
        /// <param name="configuration">The database configuration.</param>
        public JsonDatabase(Config configuration)
        {
            path = configuration.Get("data_path", "Data");
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new Exception("No path for the database was specified. please configure 'data_path' and restart.");
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
        string Pack(string data) => Convert.ToString((char)(data.Length + 32)) + data;

        /// <summary>
        /// Gets the list of servers as a single string.
        /// </summary>
        /// <returns></returns>
        public string GetServers()
        {
            string serverList = "";

            string serversPath = Path.Combine(path, "servers.json");
            if (File.Exists(serversPath))
            {
                using (var reader = new JsonTextReader(File.OpenText(serversPath)))
                {
                    var servers = serializer.Deserialize<List<Server>>(reader);
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
            }

            return Convert.ToString((char)32);
        }

        /// <summary>
        /// Checks whether a account with the specified credentials exists.
        /// </summary>
        /// <param name="accountName">The name of the account.</param>
        /// <param name="password">The password.</param>
        /// <returns>True if the account exists; otherwise, false.</returns>
        public bool AccountExists(string accountName, string password)
        {
            string accountPath = Path.Combine(accountsPath, accountName + ".json");
            if (File.Exists(accountPath))
            {
                using (var reader = new JsonTextReader(File.OpenText(accountPath)))
                {
                    var accountData = (JObject)JToken.ReadFrom(reader);

                    var passwordToken = accountData["password"];
                    if (passwordToken != null && passwordToken.ToString() == password)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}