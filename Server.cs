using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Listserver
{
    public class Server : BackgroundService
    {
        private readonly IDatabase database;
        private readonly ServerSettings options;
        private Socket socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="options">The server options.</param>
        public Server(IDatabase database, IOptions<ServerSettings> options)
        {
            this.database = database;
            this.options = options.Value;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, options.Port));
            socket.Listen((int)SocketOptionName.MaxConnections);

            Log.Information("The server is running on port {Port}.", options.Port);
            Log.Information("Listening for connections...");

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var client =
                    await Task.Factory.FromAsync(
                        socket.BeginAccept,
                        socket.EndAccept,
                        null);

                if (client is null) continue;

                _ = new Player(client, database, options).Run(stoppingToken);

                await Task.Delay(10, stoppingToken);
            }

            Log.Information("Server has stopped.");
        }
    }
}
