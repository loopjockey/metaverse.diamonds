using Discord.Rest;
using Metaverse.Functions.Common.Configuration;
using Metaverse.Functions.Common.OpenSea;
using Metaverse.Functions.Data;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nethereum.Signer;
using System;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(MyNamespace.Startup))]

namespace MyNamespace
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddSingleton<HttpClient>()
                .AddSingleton<EthereumMessageSigner>()
                .AddSingleton(c => new BotTokenSetting(c.GetService<IConfiguration>()))
                .AddSingleton(c => new StorageConnectionStringSetting(c.GetService<IConfiguration>()))
                .AddSingleton<ITableStorageClient>(c => new TableStorageClient(c.GetService<StorageConnectionStringSetting>()))
                .AddSingleton(_ => new OpenSeaApiClient())
                .AddScoped(_ => new DiscordRestClient());
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder) 
        {
            builder.ConfigurationBuilder
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}