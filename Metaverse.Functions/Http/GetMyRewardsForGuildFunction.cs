using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using GraphQL.Client.Abstractions;
using Discord;
using Discord.Rest;
using Metaverse.Functions.Data;
using Metaverse.Functions.Common.Extensions;
using System;
using Nethereum.Signer;
using Metaverse.Functions.Common.Configuration;
using Metaverse.Functions.Models;
using Discord.Net;

namespace Metaverse.Functions.Http
{
    public class GetMyRewardsForGuildFunction
    {
        private readonly ITableStorageClient _tableStorageClient;
        private readonly IGraphQLClient _eip721GraphClient;
        private readonly DiscordRestClient _discordClient;
        private readonly EthereumMessageSigner _ethereumMessageSigner;
        private readonly BotTokenSetting _botTokenSetting;

        public GetMyRewardsForGuildFunction(ITableStorageClient tableStorageClient, IGraphQLClient eip721GraphClient, DiscordRestClient discordClient, EthereumMessageSigner ethereumMessageSigner, BotTokenSetting botTokenSetting)
        {
            _tableStorageClient = tableStorageClient;
            _eip721GraphClient = eip721GraphClient;
            _discordClient = discordClient;
            _ethereumMessageSigner = ethereumMessageSigner;
            _botTokenSetting = botTokenSetting;
        }

        [FunctionName(nameof(GetMyRewardsForGuild))]
        public async Task<IActionResult> GetMyRewardsForGuild(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/guilds/{guildId}/rewards/mine")] HttpRequest req,
            ILogger log,
            string guildId)
        {
            var authToken = req.Headers.GetAuthorizationHeader();
            if (authToken == null) return new UnauthorizedResult();
            var (signature, ticks, expiryDate) = req.Headers.GetSignatureHeader();
            if (expiryDate < DateTimeOffset.UtcNow) return new UnauthorizedResult();
            if (!ulong.TryParse(guildId, out var pGuildId)) return new BadRequestObjectResult("Invalid guild ID");

            ulong userId;
            string addressOfSigner;
            var userDiscordSession = await _discordClient.ScopedLoginAsync(TokenType.Bearer, authToken);
            await using (userDiscordSession)
            {
                userId = _discordClient.CurrentUser.Id;
                var verificationMessage = _discordClient.GenerateVerificationMessage(ticks);
                addressOfSigner = _ethereumMessageSigner.EncodeUTF8AndEcRecover(verificationMessage, signature);
            }

            var serverDiscordSession = await _discordClient.ScopedLoginAsync(TokenType.Bot, _botTokenSetting);
            await using (serverDiscordSession)
            {
                var tokenRewards = _tableStorageClient.GetTokenRewardDefinitions(pGuildId);
                try
                {
                    var getGuildTask = _discordClient.GetGuildAsync(pGuildId);
                    var getAddressTokensTask = _eip721GraphClient.GetAddressTokensAsync(addressOfSigner);
                    var getGuildUserTask = _discordClient.GetGuildUserAsync(pGuildId, userId);
                    await Task.WhenAll(getGuildTask, getGuildUserTask, getAddressTokensTask);

                    var rolesWithRewards = new HashSet<ulong>();
                    var applicableRoles = new Dictionary<string, string>();
                    foreach (var reward in tokenRewards)
                    {
                        rolesWithRewards.Add(reward.TargetRoleId);
                        if (applicableRoles.ContainsKey(reward.TargetRoleId.ToString())) continue;
                        var applicableToken = getAddressTokensTask.Result.FirstOrDefault(t => reward.AppliesTo(t.address, t.tokenId));
                        if (applicableToken == default) continue;
                        applicableRoles[reward.TargetRoleId.ToString()] = $"{applicableToken.address}/{applicableToken.tokenId}";                    
                    }

                    var allRoles =
                        from r in getGuildTask.Result.Roles.OrderByDescending(r => r.Position)
                        where rolesWithRewards.Contains(r.Id)
                        let isAdmin = r.Permissions.Administrator
                        let colour = $"{(int)r.Color.R:X2}{(int)r.Color.G:X2}{(int)r.Color.B:X2}"
                        select new DiscordGuildRole(r.Id, r.Name, colour, isAdmin);

                    var currentRoleIds = getGuildUserTask.Result.RoleIds.Select(r => r.ToString()).ToArray();
                    var shopUrl = _tableStorageClient.GetGuildConfiguration(pGuildId, GuildConfigurationEntity.KnownConfigurationKeys.ShopUrl);
                    return new OkObjectResult(new { 
                        applicableRoles,
                        currentRoleIds,
                        shopUrl,
                        allRoles
                    });
                }
                catch (HttpException e)
                {
                    if (e.DiscordCode == 50001)
                        return new NotFoundResult();
                    throw;
                }
            }
            
        }
    }
}
