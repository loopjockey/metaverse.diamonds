using Metaverse.Core;
using Metaverse.Functions.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Metaverse.Functions.Queue
{
    public class RemoveRewardToServiceFunction
    {
        private readonly ITableStorageClient _tableStorageClient;

        public RemoveRewardToServiceFunction(ITableStorageClient tableStorageClient)
        {
            _tableStorageClient = tableStorageClient;
        }

        [FunctionName(nameof(RemoveRewardToGuild))]
        public async Task RemoveRewardToGuild(
            [QueueTrigger(GuildReferenceCommand.RemoveRewardQueueName)] GuildReferenceCommand model,
            ILogger log)
        {
            await _tableStorageClient.RemoveTokenRewardAsync(model.GuildId, Convert(model));
        }

        private static TokenRewardEntity.Row Convert(GuildReferenceCommand model) 
        {
            switch (model.RewardDefinition)
            {
                case GuildReferenceCommand.RewardDefinitionType.All:
                default:
                    return new TokenRewardEntity.Row(model.CreatorAddress, DateTimeOffset.UtcNow);
                case GuildReferenceCommand.RewardDefinitionType.Specific:
                    return new TokenRewardEntity.Row(model.CreatorAddress, model.SpecificTokenId.Value, DateTimeOffset.UtcNow);
                case GuildReferenceCommand.RewardDefinitionType.Range:
                    return new TokenRewardEntity.Row(model.CreatorAddress, model.MinimumTokenId.Value, model.MaximumTokenId.Value, DateTimeOffset.UtcNow);
            }
        }
    }
}
