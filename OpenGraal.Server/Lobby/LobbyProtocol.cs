using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenGraal.Net;
using OpenGraal.Server.Lobby.Packets;
using OpenGraal.Server.Services.Accounts;

namespace OpenGraal.Server.Lobby;

internal sealed class LobbyProtocol : Protocol
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LobbyProtocol> _logger;
    private readonly LobbyManager _lobbyManager;
    private readonly AccountService _accountService;
    private string _accountName = string.Empty;

    public LobbyProtocol(
        IConfiguration configuration,
        ILogger<LobbyProtocol> logger,
        LobbyManager lobbyManager,
        AccountService accountService)
        : base(logger)
    {
        _configuration = configuration;
        _logger = logger;
        _lobbyManager = lobbyManager;
        _accountService = accountService;

        Bind<IdentifyPacket>(0, OnIdentify);
        Bind<LoginPacket>(1, OnLogin);
    }

    private static void OnIdentify(IConnection connection, IdentifyPacket packet)
    {
        if (packet.ClientVersion != "newmain")
        {
            connection.Send(new DisconnectPacket("You are using a unsupported client."));
        }
    }

    private void OnLogin(IConnection connection, LoginPacket packet)
    {
        if (!_accountService.AccountExists(packet.AccountName, packet.Password))
        {
            _logger.LogDebug(
                "[{SessionId}] Login failed for {Address}",
                connection.Id, connection.Address);

            connection.Send(new DisconnectPacket("Invalid account name or password."));

            return;
        }

        _accountName = packet.AccountName;

        _logger.LogDebug(
            "[{SessionId}] {Address} has logged in as {AccountName}",
            connection.Id, connection.Address, _accountName);

        SendLogin(connection);
    }

    private void SendLogin(IConnection connection)
    {
        /* Get the message of the day. */
        var motd = _configuration["Motd"] ?? string.Empty;
        if (motd.Length > 0)
        {
            motd = motd.Replace("%{AccountName}", _accountName);

            connection.Send(new MotdPacket(motd));
        }

        /* Check if the 'Pay by Credit Card' button should be shown. */
        if (_configuration.GetValue<bool>("PayByCreditCard"))
        {
            var url = (_configuration["PayByCreditCardUrl"] ?? "").Trim();
            if (url.Length > 0)
            {
                connection.Send(new PayByCreditCardPacket(url));
            }
        }

        /* Check if the 'Pay by Phone' button should be shown. */
        if (_configuration.GetValue<bool>("PayByPhone"))
        {
            connection.Send(new PayByPhonePacket());
        }

        /* Check if the 'Show More' button should be shown. */
        if (_configuration.GetValue<bool>("ShowMore"))
        {
            var url = (_configuration["ShowMoreUrl"] ?? "").Trim();
            if (url.Length > 0)
            {
                connection.Send(new ShowMorePacket(url));
            }
        }

        var serverList = _lobbyManager.GetServerList();
        
        connection.Send(new ServerListPacket(serverList));
    }
}