namespace OpenGraal.Server.Database;

internal interface IDatabase
{
    List<ServerInfo> GetServers();
    bool AccountExists(string accountName, string password);
}