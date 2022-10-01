namespace Listserver.Database;

public interface IDatabase
{
    string GetServers();
    bool AccountExists(string accountName, string password);
}