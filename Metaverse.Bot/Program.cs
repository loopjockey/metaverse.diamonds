using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Metaverse.Bot.Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Metaverse.Bot
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();

                client.JoinedGuild += (g) => client
                    .GetGuild(g.Id)
                    .GetTextChannel(g.DefaultChannel.Id)
                    .SendMessageAsync("Hey there! Please go to https://setup.metaverse.diamonds to find comprehensive setup instructions.");

                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                await client.LoginAsync(TokenType.Bot, "NzkwNTU5MTIwNjAzODczMzIw.X-CXjg.RxQemVh7O3Y4-tiVVZ_twsmyei0");
                await client.StartAsync();

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await Task.Delay(Timeout.Infinite);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
    }
}
