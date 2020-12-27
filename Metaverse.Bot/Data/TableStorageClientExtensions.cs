using System.Threading.Tasks;

namespace Metaverse.Bot.Data
{
    public static class TableStorageClientExtensions
    {
        public static async Task SetGuildConfigurationPairAsync(this ITableStorageClient tables, ulong guildId, string key, string value) 
        {
            await tables.InsertOrReplaceAsync(GuildConfigurationEntity.TableName, new GuildConfigurationEntity(guildId, key, value));
        }

        public static async Task AddTokenRewardAsync(this ITableStorageClient tables, ulong guildId, TokenRewardEntity.Row row) 
        {
            await tables.InsertOrReplaceAsync(TokenRewardEntity.TableName, new TokenRewardEntity(guildId, row));
        }

        public static TokenRewardEntity[] GetTokenRewardDefinitions(this ITableStorageClient tables, ulong guildId)
        {
            return tables.GetFirstPageOfEntities<TokenRewardEntity>(TokenRewardEntity.TableName, new TokenRewardEntity.PartitionKey(guildId).ToString());
        }
    }
}
