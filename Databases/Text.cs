using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace Listserver.Databases
{
    public class TextDB : Database
    {
        private string _folder;
        private bool _canuse = true;

        private string Pack(string Data)
        {
            return Convert.ToString((char)(Data.Length + 32)) + Data;
        }

        public bool Init()
        {
            /* Get the path for the text database from the config file. */
            _folder = Program.Config["textdb_path"];
            if (_folder == String.Empty)
            {
                _canuse = false;
                Log.Write(LogLevel.Error, "DB", "No path for the text database was specified. please configure 'textdb_path' and restart.");
                return false;
            }

            /* Check if the folder exists, if it doesnt try to create it. */
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
                if (!Directory.Exists(_folder))
                {
                    _canuse = false;
                    return false;
                }
                else
                {
                    /* Create some additional directories and files. */
                    CreateDBStructure();
                }
            }
            return true;
        }

        /// <summary>
        /// Generates a text database folder structure.
        /// </summary>
        private void CreateDBStructure()
        {
            Directory.CreateDirectory(_folder + @"\accounts");
            Directory.CreateDirectory(_folder + @"\playerdata");
            File.Create(_folder + @"\servers.dat");
            File.Create(_folder + @"\worlds.dat");
        }

        public string GetServers()
        {
            if (!_canuse) return Convert.ToString((char)32);

            string sSection  = "";
            string sFilename = _folder + @"\servers.dat";
            string sList     = "";

            if (File.Exists(sFilename))
            {
                IniFile oData = new IniFile(sFilename);
                try
                {
                    int iServers = int.Parse(oData.IniReadValue("main", "servers"));
                    if (iServers > 0)
                    {
                        for (int i = 1; i < (iServers + 1); i++)
                        {
                            sSection = "server" + i.ToString();

                            string Name = oData.IniReadValue(sSection, "name");
                            
                            if (oData.IniReadValue(sSection, "type").ToLower() == "gold")
                            {
                                Name = "P " + Name;
                            }

                            sList += "(";
                            sList += Pack(Name);
                            sList += Pack(oData.IniReadValue(sSection, "lang"));
                            sList += Pack(oData.IniReadValue(sSection, "desc"));
                            sList += Pack(oData.IniReadValue(sSection, "url"));
                            sList += Pack(oData.IniReadValue(sSection, "version"));
                            sList += Pack(oData.IniReadValue(sSection, "players"));
                            sList += Pack(oData.IniReadValue(sSection, "ip"));
                            sList += Pack(oData.IniReadValue(sSection, "port"));
                        }

                        return Convert.ToString((char)(iServers + 32)) + sList;
                    }
                }
                catch { }
            }

            return Convert.ToString((char)32);
        }

        public bool AccountExists(string Username, string Password)
        {
            if (!_canuse) return false;

            string sFilename = _folder + @"\accounts\" + Username.ToLower() + ".dat";
            if (File.Exists(sFilename))
            {
                IniFile sData = new IniFile(sFilename);
                if (sData.IniReadValue("account", "password") == Password)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class IniFile
    {
        public string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key,string val,string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key,string def, StringBuilder retVal,
            int size,string filePath);

        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <PARAM name="INIPath"></PARAM>
        public IniFile(string INIPath)
        {
            path = INIPath;
        }
        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// Section name
        /// <PARAM name="Key"></PARAM>
        /// Key Name
        /// <PARAM name="Value"></PARAM>
        /// Value Name
        public void IniWriteValue(string Section,string Key,string Value)
        {
            WritePrivateProfileString(Section,Key,Value,this.path);
        }
        
        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public string IniReadValue(string Section,string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section,Key,"",temp, 
                                            255, this.path);
            return temp.ToString();

        }
    }
}
