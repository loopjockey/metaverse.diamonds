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

        public static string GetUniqueUserName(this DiscordRestClient restClient) 
        {
            var userName = restClient.CurrentUser.Username;
            var discriminator = restClient.CurrentUser.Discriminator;
            return $"{userName}#{discriminator}";
        }

        public static string GenerateVerificationMessage(this DiscordRestClient restClient, long expiryTicks)
        {
            var userName = restClient.GetUniqueUserName();
            return $"I agree to link this user {userName} to my current address. Expires: {expiryTicks}";
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
