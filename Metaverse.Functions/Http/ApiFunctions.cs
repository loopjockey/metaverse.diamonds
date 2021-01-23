using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using Metaverse.Functions.Graph;
using Metaverse.Bot.Data;
using System.Collections.Generic;
using System.Numerics;
using GraphQL.Client.Abstractions;
using Discord;
using Metaverse.Functions.Common;
using Discord.Rest;
using Metaverse.Functions.Models;
using Metaverse.Core.Ethereum;
using System;

namespace Metaverse.Functions.Http
{
    public class ApiFunctions
    {
        private readonly ITableStorageClient _tableStorageClient;
        private readonly IGraphQLClient _eip721GraphClient;
        private readonly DiscordRestClient _discordClient;

        public ApiFunctions(ITableStorageClient tableStorageClient, IGraphQLClient eip721GraphClient, DiscordRestClient discordClient)
        {
            _tableStorageClient = tableStorageClient;
            _eip721GraphClient = eip721GraphClient;
            _discordClient = discordClient;
        }

        [FunctionName(nameof(GetServersVisibleToUser))]
        public async Task<IActionResult> GetServersVisibleToUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log) 
        {
            var (_, authToken) = req.Headers.GetAuthorizationHeader();
            await _discordClient.LoginAsync(TokenType.Bearer, authToken);
            var userId = _discordClient.CurrentUser.Id;
            return new OkObjectResult(new {
                userId,
                guilds = from g in await _discordClient.GetGuildsAsync()
                         select new DiscordGuild(g, userId)
            });
        }

        [FunctionName(nameof(GetAvailableRewardsForGuild))]
        public async Task<IActionResult> GetAvailableRewardsForGuild(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log)
        {
            var ownerAddress = req.Query.GetAddress();
            var guildId = req.Query.GetGuildId();
            var tokenRewards = _tableStorageClient.GetTokenRewardDefinitions(guildId);
            var creatorAddressses = tokenRewards.Select(tr => tr.CreatorAddress).Distinct();
            var tokens = await _eip721GraphClient.GetAddressTokensAsync(ownerAddress, creatorAddressses);
            var applicableRoles = new Dictionary<ulong, (string creatorAddress, BigInteger tokenId)>();
            foreach (var reward in tokenRewards) 
            {
                if (applicableRoles.ContainsKey(reward.TargetRoleId)) continue;
                var applicableToken = tokens.FirstOrDefault(t => reward.AppliesTo(t.address, t.tokenId));
                if (applicableToken == default) continue;
                applicableRoles[reward.TargetRoleId] = applicableToken;
            }

            var (_, authToken) = req.Headers.GetAuthorizationHeader();
            await _discordClient.LoginAsync(TokenType.Bearer, authToken);
            var currentGuildUser = await _discordClient.GetGuildUserAsync(guildId, _discordClient.CurrentUser.Id);
            return new OkObjectResult(new { 
                applicableRoles,
                roleIds = currentGuildUser.RoleIds.ToArray()
            });
        }

        [FunctionName(nameof(ApplyNftToServer))]
        public async Task<IActionResult> ApplyNftToServer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            ILogger log) 
        {
            var (_, authToken) = req.Headers.GetAuthorizationHeader();
            var verificationData = await req.ReadBodyAsync<VerificationModel>();
            if (!VerificationMessageParser.TryParse(verificationData.Message, out var model)) return new BadRequestObjectResult("Malformed message");
            if (!VerificationMessageParser.TryParsePath(model.Path, out var path)) return new BadRequestObjectResult("Malformed path");
            if (model.Timestamp > DateTimeOffset.UtcNow.AddDays(1)) return new BadRequestObjectResult("Malformed timestamp");
            if (model.Timestamp < DateTimeOffset.UtcNow.AddDays(-1)) return new BadRequestObjectResult("Expired timestamp");

            var tokenRewards = _tableStorageClient.GetTokenRewardDefinitions(model.GuildId);
            var applicableReward = tokenRewards
                .Where(r => r.TargetRoleId == model.RoleId)
                .Where(r => r.AppliesTo(path.creatorAddress, path.tokenId))
                .FirstOrDefault();
            if (applicableReward == default) return new NotFoundObjectResult("There is no applicable reward");

            await _discordClient.LoginAsync(TokenType.Bearer, authToken);
            var userId = _discordClient.CurrentUser.Id;
            if (model.UserId != userId) return new UnauthorizedResult();
            await _discordClient.LogoutAsync();

            var ethereumAddress = SignatureValidator.GetAddressOfSignature(verificationData.Signature, verificationData.Message);
            var token = await _eip721GraphClient.GetAddressTokenAsync(ethereumAddress, path.creatorAddress, path.tokenId);
            if (token == default) return new NotFoundObjectResult("You do not own this NFT");

            await _discordClient.LoginAsync(TokenType.Bot, "NzkwNTU5MTIwNjAzODczMzIw.X-CXjg.RxQemVh7O3Y4-tiVVZ_twsmyei0");
            var guild = await _discordClient.GetGuildAsync(model.GuildId);
            var guildRole = guild.GetRole(model.RoleId);
            var guildUser = await _discordClient.GetGuildUserAsync(model.GuildId, userId);
            await guildUser.AddRoleAsync(guildRole);

            return new OkResult();            
        }
    }
}
