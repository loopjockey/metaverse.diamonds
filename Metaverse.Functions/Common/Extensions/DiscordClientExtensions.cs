using Discord;
using Discord.Rest;
using System;
using System.Threading.Tasks;

namespace Metaverse.Functions.Common.Extensions
{
    public static class DiscordClientExtensions
    {
        public static async Task<IAsyncDisposable> ScopedLoginAsync(this DiscordRestClient restClient, TokenType tokenType, string token) 
        {
            await restClient.LoginAsync(tokenType, token);
            return new DiscordLogoutDisposable(restClient);
        }

        private class DiscordLogoutDisposable : IAsyncDisposable
        {
            private readonly DiscordRestClient _restClient;

            public DiscordLogoutDisposable(DiscordRestClient restClient)
            {
                _restClient = restClient;
            }

            public async ValueTask DisposeAsync()
            {
                await _restClient.LogoutAsync();
            }
        }
    }
}
