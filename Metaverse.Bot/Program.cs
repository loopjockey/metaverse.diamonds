using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Metaverse.Bot.Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;

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
                client.Log += LogAsync;

                var commandService = services.GetRequiredService<CommandService>();
                commandService.Log += LogAsync;

                client.JoinedGuild += (g) => client
                    .GetGuild(g.Id)
                    .GetTextChannel(g.DefaultChannel.Id)
                    .SendMessageAsync("Hey there! Please go to https://setup.metaverse.diamonds to find comprehensive setup instructions for your guild.");

                await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("BotToken"));
                await client.StartAsync();

                var commandHandlingService = services.GetRequiredService<CommandHandlingService>();
                await commandHandlingService.InitializeAsync();

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
            var storageConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__StorageAccountConnectionString");
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<Func<string, QueueClient>>((queueName) => new QueueClient(storageConnectionString, queueName))
                .BuildServiceProvider();
        }
    }
}
