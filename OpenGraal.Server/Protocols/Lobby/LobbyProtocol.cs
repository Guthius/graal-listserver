﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenGraal.Net;
using OpenGraal.Server.Protocols.Lobby.Packets;
using OpenGraal.Server.Services.Accounts;
using OpenGraal.Server.Services.Lobby;

namespace OpenGraal.Server.Protocols.Lobby;

internal sealed class LobbyProtocol : Protocol
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LobbyProtocol> _logger;
    private readonly LobbyManager _lobbyManager;
    private readonly AccountService _accountService;
    private string _accountName = string.Empty;

    public LobbyProtocol(IConfiguration configuration, ILogger<LobbyProtocol> logger, LobbyManager lobbyManager,
        AccountService accountService) : base(logger)
    {
        _configuration = configuration;
        _logger = logger;
        _lobbyManager = lobbyManager;
        _accountService = accountService;

        Bind<IdentifyPacket>(0, OnIdentify);
        Bind<LoginPacket>(1, OnLogin);
    }

    private static void OnIdentify(ISession session, IdentifyPacket packet)
    {
        if (packet.ClientVersion != "newmain")
        {
            session.Send(new DisconnectPacket
            {
                Message = "You are using a unsupported client."
            });
        }
    }

    private void OnLogin(ISession session, LoginPacket packet)
    {
        if (!_accountService.AccountExists(packet.AccountName, packet.Password))
        {
            _logger.LogError("[{SessionId}] Login failed for {Address}",
                session.Id, session.Address);

            session.Send(new DisconnectPacket
            {
                Message = "Invalid account name or password."
            });

            return;
        }

        _accountName = packet.AccountName;

        _logger.LogInformation("[{SessionId}] {Address} has logged in as {AccountName}",
            session.Id, session.Address, _accountName);

        SendLogin(session);
    }

    private void SendLogin(ISession session)
    {
        /* Get the message of the day. */
        var motd = _configuration["Motd"];
        if (motd.Length > 0)
        {
            motd = motd.Replace("%{AccountName}", _accountName);

            session.Send(new MotdPacket
            {
                Message = motd
            });
        }

        /* Check if the 'Pay by Credit Card' button should be shown. */
        if (_configuration.GetValue<bool>("PayByCreditCard"))
        {
            var url = _configuration["PayByCreditCardUrl"].Trim();
            if (url.Length > 0)
            {
                session.Send(new PayByCreditCardPacket
                {
                    Url = url
                });
            }
        }

        /* Check if the 'Pay by Phone' button should be shown. */
        if (_configuration.GetValue<bool>("PayByPhone"))
        {
            session.Send(new PayByPhonePacket());
        }

        /* Check if the 'Show More' button should be shown. */
        if (_configuration.GetValue<bool>("ShowMore"))
        {
            var url = _configuration["ShowMoreUrl"].Trim();
            if (url.Length > 0)
            {
                session.Send(new ShowMorePacket
                {
                    Url = url
                });
            }
        }

        session.Send(new ServerListPacket
        {
            ServerInfos = _lobbyManager.GetServerInfos()
        });
    }
}