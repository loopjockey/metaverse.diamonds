using Metaverse.Core;
using Metaverse.Functions.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Metaverse.Functions.Queue
{
    public class UpdateGuildConfigurationFunction
    {
        private readonly ITableStorageClient _tableStorageClient;

        public UpdateGuildConfigurationFunction(ITableStorageClient tableStorageClient)
        {
            _tableStorageClient = tableStorageClient;
        }

        [FunctionName(nameof(UpdateGuildConfiguration))]
        public async Task UpdateGuildConfiguration(
            [QueueTrigger(UpdateGuildConfigurationCommand.QueueName)] UpdateGuildConfigurationCommand model,
            ILogger log)
        {
            await _tableStorageClient.SetGuildConfigurationPairAsync(model.GuildId, model.Key, model.Value);
        }
    }
}
