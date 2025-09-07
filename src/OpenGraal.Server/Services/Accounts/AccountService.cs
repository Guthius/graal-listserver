using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace OpenGraal.Server.Services.Accounts;

internal sealed class AccountService
{
    private readonly string _path;

    public AccountService(IConfiguration configuration)
    {
        _path = configuration.GetValue<string>("DataPath", "Data");
        if (string.IsNullOrEmpty(_path))
        {
            throw new Exception(
                "No path for the database was specified. " +
                "Please configure 'DataPath' and restart.");
        }

        _path = Path.Combine(_path, "Accounts");
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }
    }

    public bool AccountExists(string name, string password)
    {
        var accountPath = Path.Combine(_path, name + ".json");
        if (!File.Exists(accountPath))
        {
            return false;
        }

        var accountJson = File.ReadAllText(accountPath);
        var account = JsonSerializer.Deserialize<Account>(accountJson);

        return account?.Password is not null && account.Password.Equals(password);
    }
}