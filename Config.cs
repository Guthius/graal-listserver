using System;
using System.Collections;
using System.Text;
using System.IO;

namespace Listserver
{
    public class Config
    {
        private string    _data;
        private Hashtable _config;
        private string    _file;

        /// <summary>
        /// Gets the value of a configuration item. If the item
        /// cannot be found it returns a empty string.
        /// </summary>
        /// <param name="Key">The name of the configuration item.</param>
        /// <returns>Value of the configuration item.</returns>
        public string Get(string Key)
        {
            if (_config.Contains(Key))
            {
                return (string)_config[Key];
            }
            return String.Empty;
        }

        /// <summary>
        /// Changes or adds a new configuration item.
        /// </summary>
        /// <param name="Key">The name of the configuration item.</param>
        /// <param name="Value">Value of the configuration item.</param>
        public void Set(string Key, string Value)
        {
            if (Key != String.Empty)
            {
                _config[Key] = Value;
            }
        }

        /// <summary>
        /// Checks if a element with the specified key exists.
        /// </summary>
        /// <param name="Key">The key to check for.</param>
        public bool Contains(string Key)
        {
            return _config.Contains(Key);
        }

        /// <summary>
        /// Saves the configuration file.
        /// </summary>
        public void Save()
        {
            if (_file != String.Empty)
            {
                /* Open the file for writing. */
                FileStream f = new FileStream(_file, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(f);

                sw.WriteLine("#\n# Graal Listserver Config File\n#");

                /* Write the entire config to the file. */
                string[] sKeys = new string[_config.Count];

                _config.Keys.CopyTo(sKeys, 0);
                foreach (string sKey in sKeys)
                {
                    if (sKey != String.Empty) sw.Write("\n" + sKey + "=" + _config[sKey]);
                }

                /* Close the file stream. */
                sw.Close();
                f.Close();
            }
        }

        /// <summary>
        /// Contructs a configuration object by loading a
        /// configuration file.
        /// </summary>
        /// <param name="File">The configuration file.</param>
        public Config(string File)
        {
            _file = File;
            _config = new Hashtable();

            try
            {
                /* Open the file and read its contents. */
                FileStream f = new FileStream(File, FileMode.Open);
                StreamReader sr = new StreamReader(f);
                _data = sr.ReadToEnd();

                /* Close the file stream. */
                sr.Close();
                f.Close();

                /* Parse the config file. */
                if (_data != String.Empty)
                {
                    string[] sLines = _data.Split("\n".ToCharArray());
                    foreach (string sLine in sLines)
                    {
                        if (sLine != String.Empty && sLine.Substring(0,1) != "#")
                        {
                            string[] sFields = sLine.Split("=".ToCharArray());
                            if (sFields.Length == 2)
                            {
                                if (sFields[0].Trim() != String.Empty)
                                {
                                    _config.Add(sFields[0].Trim().ToLower(), sFields[1].Trim());
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
