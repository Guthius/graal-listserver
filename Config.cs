using System.Collections.Generic;
using System.IO;

namespace Listserver
{
    public class Config
    {
        private readonly Dictionary<string, string> settings;
        private readonly string path;

        /// <summary>
        /// Gets the string value of a configuration item. If the item cannot be found it returns a empty string.
        /// </summary>
        /// <param name="key">The key of the configuration item.</param>
        /// <returns>Value of the configuration item.</returns>
        public string Get(string key)
        {
            if (settings.ContainsKey(key))
            {
                return settings[key];
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the string value of a configuration item. If the item cannot be found it returns a empty string.
        /// </summary>
        /// <param name="key">The key of the configuration item.</param>
        /// <param name="defaultValue">The default value of the configuration item.</param>
        /// <returns>Value of the configuration item.</returns>
        public string Get(string key, string defaultValue = "")
        {
            if (settings.ContainsKey(key))
            {
                return settings[key];
            }
            return defaultValue ?? "";
        }

        /// <summary>
        /// Gets the boolean value of a configuration item. If the item cannot be found it returns a empty string.
        /// </summary>
        /// <param name="key">The key of the configuration item.</param>
        /// <param name="defaultValue">The default value of the configuration item.</param>
        /// <returns>Value of the configuration item.</returns>
        public bool GetBool(string key, bool defaultValue = false)
        {
            var value = Get(key);
            if (!string.IsNullOrEmpty(key) && bool.TryParse(value, out var result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets the integer value of a configuration item. If the item cannot be found it returns a empty string.
        /// </summary>
        /// <param name="key">The key of the configuration item.</param>
        /// <param name="defaultValue">The default value of the configuration item.</param>
        /// <returns>Value of the configuration item.</returns>
        public int GetInt(string key, int defaultValue = 0)
        {
            var value = Get(key);
            if (!string.IsNullOrEmpty(key) && int.TryParse(value, out var result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets or sets the value of the configuration item with the specified key.
        /// </summary>
        /// <param name="key">The key of the configuration item.</param>
        /// <returns>The value of the configuration item.</returns>
        public string this[string key]
        {
            get => Get(key);
            set
            {
                if (key != string.Empty)
                {
                    settings[key] = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        /// <param name="path">The configuration file path.</param>
        public Config(string path)
        {
            this.path = path;
            settings = new Dictionary<string, string>();

            try
            {
                var lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (line.Length > 0 && line[0] != '#')
                    {
                        var j = line.IndexOf('=');
                        if (j == -1)
                        {
                            continue;
                        }

                        var key = line.Substring(0, j).Trim();
                        if (key.Length > 0)
                        {
                            var value = line.Substring(j + 1).Trim();

                            settings.Add(key, value);
                        }
                    }
                }
            }
            catch { }
        }
    }
}