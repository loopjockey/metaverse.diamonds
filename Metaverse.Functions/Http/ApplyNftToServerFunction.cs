using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using GraphQL.Client.Abstractions;
using Discord;
using Discord.Rest;
using System;
using Metaverse.Functions.Data;
using Metaverse.Functions.Queue;
using Metaverse.Functions.Common.Extensions;
using System.Numerics;
using Nethereum.Util;
using Nethereum.Signer;

namespace Metaverse.Functions.Http
{
    public class ApplyNftToServerFunction
    {
        private readonly ITableStorageClient _tableStorageClient;
        private readonly IGraphQLClient _eip721GraphClient;
        private readonly DiscordRestClient _discordClient;
        private readonly EthereumMessageSigner _ethereumMessageSigner;

        public ApplyNftToServerFunction(ITableStorageClient tableStorageClient, IGraphQLClient eip721GraphClient, DiscordRestClient discordClient, EthereumMessageSigner ethereumMessageSigner)
        {
            _tableStorageClient = tableStorageClient;
            _eip721GraphClient = eip721GraphClient;
            _discordClient = discordClient;
            _ethereumMessageSigner = ethereumMessageSigner;
        }

        [FunctionName(nameof(ApplyNftToServer))]
        public async Task<IActionResult> ApplyNftToServer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/guilds/{guildId}/rewards/{roleId}/apply/{creatorAddress}/{tokenId}")] HttpRequest req,
            [Queue(ApplyRoleToUserFunction.QueueName)] ICollector<ApplyRoleToUserFunction.BusCommand> collector,
            ILogger log,
            string guildId,
            string roleId,
            string creatorAddress,
            string tokenId) 
        {
            var authToken = req.Headers.GetAuthorizationHeader();
            if (authToken == null) return new UnauthorizedResult();
            var (signature, ticks, expiryDate) = req.Headers.GetSignatureHeader();
            if (expiryDate < DateTimeOffset.UtcNow) return new UnauthorizedResult();
            if (!ulong.TryParse(guildId, out var pGuildId)) return new BadRequestObjectResult("Invalid guild ID");
            if (!ulong.TryParse(roleId, out var pRoleId)) return new BadRequestObjectResult("Invalid role ID");
            if (!BigInteger.TryParse(tokenId, out var pTokenId)) return new BadRequestObjectResult("Invalid token ID");
            if (!AddressUtil.Current.IsValidEthereumAddressHexFormat(creatorAddress)) return new BadRequestObjectResult("Invalid creator address."); 

            var applicableReward = _tableStorageClient
                .GetTokenRewardDefinitions(pGuildId)
                .Where(r => r.TargetRoleId == pRoleId)
                .Where(r => r.AppliesTo(creatorAddress, pTokenId))
                .FirstOrDefault();
            if (applicableReward == default) return new NotFoundObjectResult("There is no applicable reward for this token.");

            var discordSession = await _discordClient.ScopedLoginAsync(TokenType.Bearer, authToken);
            await using (discordSession)
            {
                var userId = _discordClient.CurrentUser.Id;
                var message = HttpExtensions.GenerateVerificationMessage(userId, ticks);
                var addressOfSigner = _ethereumMessageSigner.EncodeUTF8AndEcRecover(message, signature);
                var token = await _eip721GraphClient.GetAddressTokenAsync(addressOfSigner, creatorAddress, pTokenId);
                if (token == default) return new NotFoundObjectResult($"This address does not currently own the token {creatorAddress}:{pTokenId}.");

                var guild = await _discordClient.GetGuildAsync(pGuildId);
                if (guild == null) return new NotFoundObjectResult("This server no longer exists.");
                var guildRole = guild.GetRole(pRoleId);
                if (guildRole == null) return new NotFoundObjectResult("This server role no longer exists.");

                collector.Add(new ApplyRoleToUserFunction.BusCommand(pGuildId, pRoleId, userId));
                return new OkResult();
            }
        }
    }
}
