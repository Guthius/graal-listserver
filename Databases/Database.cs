using System;
using System.Collections.Generic;
using System.Text;

namespace Listserver.Databases
{
    interface Database
    {
        string GetServers();

        bool AccountExists(string Username, string Password);
    }
}
