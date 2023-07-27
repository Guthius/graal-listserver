using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenGraal.Net;
using OpenGraal.Server.Lobby.Packets;
using OpenGraal.Server.Services.Accounts;

namespace OpenGraal.Server.Lobby;

internal sealed class LobbyUser : User
{
    private readonly ILogger<LobbyUser> _logger;
    private readonly IConfiguration _configuration;
    private readonly AccountService _accountService;
    private readonly LobbyService _lobbyService;
    private string _accountName = string.Empty;

    public LobbyUser(
        ILogger<LobbyUser> logger, 
        IConfiguration configuration, 
        AccountService accountService,
        LobbyService lobbyService)
    {
        _logger = logger;
        _configuration = configuration;
        _accountService = accountService;
        _lobbyService = lobbyService;
    }
    
    public void Login(string accountName, string password)
    {
        if (!_accountService.AccountExists(accountName, password))
        {
            _logger.LogDebug(
                "[{SessionId}] Login failed for {Address}",
                Id, Address);

            Send(new Disconnect("Invalid account name or password."));

            return;
        }
        
        _accountName = accountName;

        _logger.LogDebug(
            "[{SessionId}] {Address} has logged in as {AccountName}",
            Id, Address, _accountName);

        SendLogin();
    }
    
    private void SendLogin()
    {
        /* Get the message of the day. */
        var motd = _configuration["Motd"] ?? string.Empty;
        if (motd.Length > 0)
        {
            motd = motd.Replace("%{AccountName}", _accountName);

            Send(new Motd(motd));
        }

        /* Check if the 'Pay by Credit Card' button should be shown. */
        if (_configuration.GetValue<bool>("PayByCreditCard"))
        {
            var url = (_configuration["PayByCreditCardUrl"] ?? "").Trim();
            if (url.Length > 0)
            {
                Send(new PayByCreditCard(url));
            }
        }

        /* Check if the 'Pay by Phone' button should be shown. */
        if (_configuration.GetValue<bool>("PayByPhone"))
        {
            Send(new PayByPhone());
        }

        /* Check if the 'Show More' button should be shown. */
        if (_configuration.GetValue<bool>("ShowMore"))
        {
            var url = (_configuration["ShowMoreUrl"] ?? "").Trim();
            if (url.Length > 0)
            {
                Send(new ShowMore(url));
            }
        }

        var serverList = _lobbyService.GetServerList();
        
        Send(new ServerList(serverList));
    }
}