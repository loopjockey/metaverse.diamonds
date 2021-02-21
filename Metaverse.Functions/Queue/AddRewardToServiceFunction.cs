using Metaverse.Core;
using Metaverse.Functions.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Metaverse.Functions.Queue
{
    public class AddRewardToServiceFunction
    {
        private readonly ITableStorageClient _tableStorageClient;

        public AddRewardToServiceFunction(ITableStorageClient tableStorageClient)
        {
            _tableStorageClient = tableStorageClient;
        }

        [FunctionName(nameof(AddRewardToGuild))]
        public async Task AddRewardToGuild(
            [QueueTrigger(GuildRewardReferenceCommand.AddRewardQueueName)] GuildRewardReferenceCommand model,
            ILogger log)
        {
            await _tableStorageClient.AddTokenRewardAsync(model.GuildId, model.RewardRoleId, model.CollectionId);
        }
    }
}
