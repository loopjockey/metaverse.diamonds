using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using Discord;
using Discord.Rest;
using Metaverse.Functions.Models;
using Metaverse.Functions.Common.Extensions;

namespace Metaverse.Functions.Http
{
    public class GetGuildsVisibleToUserFunction
    {
        private readonly DiscordRestClient _discordClient;

        public GetGuildsVisibleToUserFunction(DiscordRestClient discordClient)
        {
            _discordClient = discordClient;
        }

        [FunctionName(nameof(GetGuildsVisibleToUser))]
        public async Task<IActionResult> GetGuildsVisibleToUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/guilds")] HttpRequest req,
            ILogger log)
        {
            var authToken = req.Headers.GetAuthorizationHeader();
            var discordSession = await _discordClient.ScopedLoginAsync(TokenType.Bearer, authToken);
            await using (discordSession)
            {
                var userId = _discordClient.CurrentUser.Id;
                var userUrl = _discordClient.CurrentUser.GetAvatarUrl();
                var username = _discordClient.CurrentUser.Username;
                var discriminator = _discordClient.CurrentUser.DiscriminatorValue;
                var user = new { id = userId, url = userUrl, name = $"{username}#{discriminator}" };
                var guilds = from g in await _discordClient.GetGuildSummariesAsync().FlattenAsync()
                             select new DiscordGuild(g);

                return new OkObjectResult(new { user, guilds });
            }
        }
    }
}
