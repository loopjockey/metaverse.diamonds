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

namespace Metaverse.Functions.Http
{
    public class GetAvailableRewardsForGuildFunction
    {
        private readonly ITableStorageClient _tableStorageClient;
        private readonly IGraphQLClient _eip721GraphClient;
        private readonly DiscordRestClient _discordClient;
        private readonly EthereumMessageSigner _ethereumMessageSigner;
        private readonly BotTokenSetting _botTokenSetting;

        public GetAvailableRewardsForGuildFunction(ITableStorageClient tableStorageClient, IGraphQLClient eip721GraphClient, DiscordRestClient discordClient, EthereumMessageSigner ethereumMessageSigner, BotTokenSetting botTokenSetting)
        {
            _tableStorageClient = tableStorageClient;
            _eip721GraphClient = eip721GraphClient;
            _discordClient = discordClient;
            _ethereumMessageSigner = ethereumMessageSigner;
            _botTokenSetting = botTokenSetting;
        }

        [FunctionName(nameof(GetAvailableRewardsForGuild))]
        public async Task<IActionResult> GetAvailableRewardsForGuild(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/guilds/{guildId}/rewards")] HttpRequest req,
            ILogger log,
            string guildId)
        {
            var authToken = req.Headers.GetAuthorizationHeader();
            if (authToken == null) return new UnauthorizedResult();
            var (signature, ticks, expiryDate) = req.Headers.GetSignatureHeader();
            if (expiryDate < DateTimeOffset.UtcNow) return new UnauthorizedResult();
            if (!ulong.TryParse(guildId, out var pGuildId)) return new BadRequestObjectResult("Invalid guild ID");

            ulong userId;
            var userDiscordSession = await _discordClient.ScopedLoginAsync(TokenType.Bearer, authToken);
            await using (userDiscordSession) userId = _discordClient.CurrentUser.Id;

            var serverDiscordSession = await _discordClient.ScopedLoginAsync(TokenType.Bot, _botTokenSetting);
            await using (serverDiscordSession)
            {
                var message = HttpExtensions.GenerateVerificationMessage(userId, ticks);
                var addressOfSigner = _ethereumMessageSigner.EncodeUTF8AndEcRecover(message, signature);
                var tokenRewards = _tableStorageClient.GetTokenRewardDefinitions(pGuildId);
                var uniqueCreatorAddresses = tokenRewards.Select(tr => tr.CreatorAddress).Distinct().ToArray();
                var getGuildTask = _discordClient.GetGuildAsync(pGuildId);
                var getAddressTokensTask = _eip721GraphClient.GetAddressTokensAsync(addressOfSigner);
                var getGuildUserTask = _discordClient.GetGuildUserAsync(pGuildId, userId);
                
                await Task.WhenAll(getGuildTask, getGuildUserTask, getAddressTokensTask);

                var allRoles = getGuildTask.Result.Roles.Select(r => new DiscordGuildRole(r.Id, r.Name)).ToArray();
                var applicableRoles = new Dictionary<ulong, (string creatorAddress, BigInteger tokenId)>();
                foreach (var reward in tokenRewards)
                {
                    if (applicableRoles.ContainsKey(reward.TargetRoleId)) continue;
                    var applicableToken = getAddressTokensTask.Result.FirstOrDefault(t => reward.AppliesTo(t.address, t.tokenId));
                    if (applicableToken == default) continue;
                    applicableRoles[reward.TargetRoleId] = applicableToken;
                }
                var currentRoleIds = getGuildUserTask.Result.RoleIds.ToArray();
                return new OkObjectResult(new { applicableRoles, allRoles, currentRoleIds });
            }
            
        }
    }
}
