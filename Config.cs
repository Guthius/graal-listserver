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
        /// Gets the boolean value of a configuration item. If the item cannot be found it returns a empty string.
        /// </summary>
        /// <param name="key">The key of the configuration item.</param>
        /// <returns>Value of the configuration item.</returns>
        public bool GetBool(string key)
        {
            var value = Get(key);
            if (!string.IsNullOrEmpty(key) && bool.TryParse(value, out var result))
            {
                return result;
            }
            return false;
        }

        /// <summary>
        /// Gets the integer value of a configuration item. If the item cannot be found it returns a empty string.
        /// </summary>
        /// <param name="key">The key of the configuration item.</param>
        /// <returns>Value of the configuration item.</returns>
        public int GetInt(string key)
        {
            var value = Get(key);
            if (!string.IsNullOrEmpty(key) && int.TryParse(value, out var result))
            {
                return result;
            }
            return 0;
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
        /// Checks if a element with the specified key exists.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        public bool Contains(string key) => settings.ContainsKey(key);

        /// <summary>
        /// Saves the configuration file.
        /// </summary>
        public void Save()
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                using (var stream = File.OpenWrite(path))
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine("#\n# Graal Listserver Config File\n#");
                    foreach (var setting in settings)
                    {
                        writer.WriteLine(setting.Key + "=" + setting.Value);
                    }
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