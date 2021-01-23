using Discord;
using Discord.Rest;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Metaverse.Bot.Data;
using Metaverse.Bot.Ethereum;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Nethereum.Web3;
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
                .AddSingleton<ITableStorageClient>(new TableStorageClient("UseDevelopmentStorage=true"))
                .AddScoped<IGraphQLClient>(_ => new GraphQLHttpClient("https://api.thegraph.com/subgraphs/name/wighawag/eip721-subgraph", new NewtonsoftJsonSerializer()))
                .AddScoped(_ => new DiscordRestClient());
        }
    }
}