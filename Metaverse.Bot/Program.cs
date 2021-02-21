using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Metaverse.Bot.Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Metaverse.Core;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace Metaverse.Bot
{
    class Program
    {
        private static readonly string DefaultBotToken = "NzkwNTU5MTIwNjAzODczMzIw.X-CXjg.RxQemVh7O3Y4-tiVVZ_twsmyei0";
        private static readonly string DefaultStorageConnectionString = "UseDevelopmentStorage=true";
        private const ulong GlobalBotAdminId = 167519206223380482;

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
                        .SendMessageAsync($"Your guild has been registered! Please navigate to https://guilds.metaverse.diamonds/{g.Id} to complete setup (you may need to refresh the page).");

                var hardStopTokenSource = new CancellationTokenSource();
                client.MessageReceived += (m) =>
                {
                    if (m?.Author?.Id == GlobalBotAdminId && m.Content == "!mvadmin hard_stop_all")
                    {
                        hardStopTokenSource.Cancel();
                        return m.AddReactionAsync(new Emoji("🗸"));
                    }
                    return Task.CompletedTask;
                };

                try
                {
                    var botToken = Environment.GetEnvironmentVariable("BotToken") ?? DefaultBotToken ?? throw new Exception("Bot token not specified");
                    await client.LoginAsync(TokenType.Bot, botToken);
                    await client.StartAsync();

                    var commandHandlingService = services.GetRequiredService<CommandHandlingService>();
                    await commandHandlingService.InitializeAsync();

                    var queueFactory = services.GetRequiredService<Func<string, QueueClient>>();
                    await Task.WhenAll(
                        queueFactory(ApplyRoleToUserCommand.QueueName).CreateIfNotExistsAsync(),
                        queueFactory(GuildRewardReferenceCommand.AddRewardQueueName).CreateIfNotExistsAsync(),
                        queueFactory(GuildRewardReferenceCommand.RemoveRewardQueueName).CreateIfNotExistsAsync(),
                        queueFactory(UpdateGuildConfigurationCommand.QueueName).CreateIfNotExistsAsync());

                    var builder = new HostBuilder();
                    builder.ConfigureWebJobs();
                    var host = builder.Build();
                    using (host)
                    {
                        await host.RunAsync(hardStopTokenSource.Token);
                    }
                }
                finally 
                {
                    await client.LogoutAsync();
                }
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            var storageConnectionString = 
                Environment.GetEnvironmentVariable("ConnectionStrings__StorageAccountConnectionString") ?? 
                DefaultStorageConnectionString ??
                throw new Exception("Storage connection");

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
