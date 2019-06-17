using System;
using System.Text;
using System.Data;
using System.Data.Odbc;

namespace Listserver.Databases
{
    public class MySQL : Database
    {
        private string _connstring;
        private OdbcConnection _conn;

        private string Pack(string Data)
        {
            return Convert.ToString((char)(Data.Length + 32)) + Data;
        }

        public string GetServers()
        {
            string sList = "";

            try
            {
                OdbcCommand dbCommand = new OdbcCommand("SELECT * FROM servers;", _conn);
                OdbcDataReader dbReader;

                dbReader = dbCommand.ExecuteReader();
                while (dbReader.Read())
                {
                    string Name = dbReader.GetString(2);
                    if (dbReader.GetString(1) == "Gold")
                    {
                        Name = "P " + Name;
                    }

                    sList += "(";
                    sList += Pack(Name);
                    sList += Pack(dbReader.GetString(3));
                    sList += Pack(dbReader.GetString(4));
                    sList += Pack(dbReader.GetString(5));
                    sList += Pack(dbReader.GetString(6));
                    sList += Pack(dbReader.GetString(7));
                    sList += Pack(dbReader.GetString(8));
                    sList += Pack(dbReader.GetString(9));
                }
                
                sList = Convert.ToString((char)(dbReader.RecordsAffected + 32)) + sList;
            }
            catch (Exception e)
            {
                Log.Write(LogLevel.Error, "Server", e.Message, 4);
            }

            return sList;
        }

        /// <summary>
        /// Tries to create a connection with the MySQL server. Returns true on success.
        /// </summary>
        /// <param name="Hostname">The hostname or IP address of the MySQL server.</param>
        /// <param name="Username">MySQL Username</param>
        /// <param name="Password">MySQL Password</param>
        /// <param name="Database">MySQL Database containing the GServer tables.</param>
        /// <returns>True on success.</returns>
        public bool Connect(string Hostname, string Username, string Password, string Database)
        {
            try
            {
                /* Open connection with the MySQL server. */
                _connstring = "DRIVER={MySQL ODBC 3.51 Driver};SERVER=" + Hostname + ";DATABASE=" + Database + ";UID=" + Username + ";PASSWORD=" + Password + ";OPTION=3;";
                _conn = new OdbcConnection(_connstring);
                _conn.Open();

                if (_conn.State == ConnectionState.Open)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Write(LogLevel.Error, "Server", e.Message);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Checks if the specified account exists.
        /// </summary>
        /// <param name="Username">Username of the account to check.</param>
        /// <param name="Password">Password of the account to check.</param>
        /// <returns>True if the account exists.</returns>
        public bool AccountExists(string Username, string Password)
        {
            OdbcCommand dbCommand = new OdbcCommand("SELECT COUNT(*) AS rows FROM accounts WHERE accname = '"+Username+"' AND encrpass = '"+ Password + "';", _conn);
            return ((Int64)dbCommand.ExecuteScalar() == 0) ? false : true;
        }

        /// <summary>
        /// Simple check to see if we are still connected with the MySQL server.
        /// </summary>
        public bool Connected
        {
            get
            {
                return (_conn.State == ConnectionState.Open) ? true : false;
            }
        }
    }
}
