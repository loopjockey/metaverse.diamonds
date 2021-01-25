using Discord.Rest;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Metaverse.Functions.Common.Configuration;
using Metaverse.Functions.Data;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Nethereum.Signer;
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
                .AddSingleton<ITableStorageClient>(new TableStorageClient("UseDevelopmentStorage=true"))
                .AddScoped<IGraphQLClient>(_ => new GraphQLHttpClient("https://api.thegraph.com/subgraphs/name/wighawag/eip721-subgraph", new NewtonsoftJsonSerializer()))
                .AddSingleton(new BotTokenSetting("NzkwNTU5MTIwNjAzODczMzIw.X-CXjg.RxQemVh7O3Y4-tiVVZ_twsmyei0"))
                .AddScoped(_ => new DiscordRestClient());
        }
    }
}