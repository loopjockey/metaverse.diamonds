using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using Discord;
using Discord.Rest;
using System;
using Metaverse.Functions.Data;
using Metaverse.Functions.Common.Extensions;
using System.Numerics;
using Nethereum.Util;
using Nethereum.Signer;
using Metaverse.Core;
using Metaverse.Functions.Common.Configuration;
using Metaverse.Functions.Common.OpenSea;

namespace Metaverse.Functions.Http
{
    public class ApplyNftToServerFunction
    {
        private readonly ITableStorageClient _tableStorageClient;
        private readonly OpenSeaApiClient _openSeaApiClient;
        private readonly DiscordRestClient _discordClient;
        private readonly EthereumMessageSigner _ethereumMessageSigner;
        private readonly BotTokenSetting _botTokenSetting;

        public ApplyNftToServerFunction(ITableStorageClient tableStorageClient, OpenSeaApiClient openSeaApiClient, DiscordRestClient discordClient, EthereumMessageSigner ethereumMessageSigner, BotTokenSetting botTokenSetting)
        {
            _tableStorageClient = tableStorageClient;
            _openSeaApiClient = openSeaApiClient;
            _discordClient = discordClient;
            _ethereumMessageSigner = ethereumMessageSigner;
            _botTokenSetting = botTokenSetting;
        }

        [FunctionName(nameof(ApplyNftToServer))]
        public async Task<IActionResult> ApplyNftToServer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/guilds/{guildId}/rewards/{roleId}/apply/{collectionId}")] HttpRequest req,
            [Queue(ApplyRoleToUserCommand.QueueName)] ICollector<ApplyRoleToUserCommand> collector,
            ILogger log,
            string guildId,
            string roleId,
            string collectionId) 
        {
            var authToken = req.Headers.GetAuthorizationHeader();
            if (authToken == null) return new UnauthorizedResult();
            var (signature, ticks, expiryDate) = req.Headers.GetSignatureHeader();
            if (expiryDate < DateTimeOffset.UtcNow) return new UnauthorizedResult();
            if (!ulong.TryParse(guildId, out var pGuildId)) return new BadRequestObjectResult("Invalid guild ID");
            if (!ulong.TryParse(roleId, out var pRoleId)) return new BadRequestObjectResult("Invalid role ID");

            var applicableReward = _tableStorageClient
                .GetTokenRewardDefinitions(pGuildId)
                .Where(r => r.TargetRoleId == pRoleId)
                .Where(r => r.CollectionId == collectionId)
                .FirstOrDefault();
            if (applicableReward == default) return new NotFoundObjectResult("There is no applicable reward for this token.");

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
                var (hasToken, erc721TokenRef) = await _openSeaApiClient.DoesAddressOwnTokenInCollection(addressOfSigner, collectionId);
                if (!hasToken) return new NotFoundObjectResult($"This address does not currently own a token within the collection {collectionId}.");

                var getGuildTask = _discordClient.GetGuildAsync(pGuildId);
                var getGuildUserTask = _discordClient.GetGuildUserAsync(pGuildId, userId);
                await Task.WhenAll(getGuildTask, getGuildUserTask);

                if (getGuildTask.Result == null) return new NotFoundObjectResult("This server no longer exists.");
                var guildRole = getGuildTask.Result.GetRole(pRoleId);
                if (guildRole == null) return new NotFoundObjectResult("This server role no longer exists.");

                collector.Add(new ApplyRoleToUserCommand(pGuildId, pRoleId, userId));
                return new OkResult();
            }
        }
    }
}