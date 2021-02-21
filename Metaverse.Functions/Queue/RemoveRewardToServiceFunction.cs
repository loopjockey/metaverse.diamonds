using Metaverse.Core;
using Metaverse.Functions.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
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
            [QueueTrigger(GuildRewardReferenceCommand.RemoveRewardQueueName)] GuildRewardReferenceCommand model,
            ILogger log)
        {
            await _tableStorageClient.RemoveTokenRewardAsync(model.GuildId, model.RewardRoleId, model.CollectionId);
        }
    }
}
