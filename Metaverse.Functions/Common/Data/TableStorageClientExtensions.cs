using System.Linq;
using System.Threading.Tasks;

namespace Metaverse.Functions.Data
{
    public static class TableStorageClientExtensions
    {
        public static async Task SetGuildConfigurationPairAsync(this ITableStorageClient tables, ulong guildId, string key, string value)
        {
            await tables.InsertOrReplaceAsync(GuildConfigurationEntity.TableName, new GuildConfigurationEntity(guildId, key, value));
        }

        public static (string key, string value)[] GetGuildConfigurationPairs(this ITableStorageClient tables, ulong guildId)
        {
            var configurations = tables.GetFirstPageOfEntities<GuildConfigurationEntity>(GuildConfigurationEntity.TableName, guildId.ToString());
            return configurations.Select(c => (c.RowKey, c.Value)).ToArray();
        }

        public static string GetGuildConfiguration(this ITableStorageClient tables, ulong guildId, string key)
        {
            var configuration = tables.GetExact<GuildConfigurationEntity>(GuildConfigurationEntity.TableName, guildId.ToString(), key);
            return configuration?.Value;
        }

        public static async Task AddTokenRewardAsync(this ITableStorageClient tables, ulong guildId, TokenRewardEntity.Row row)
        {
            await tables.InsertOrReplaceAsync(TokenRewardEntity.TableName, new TokenRewardEntity(guildId, row));
        }

        public static async Task RemoveTokenRewardAsync(this ITableStorageClient tables, ulong guildId, TokenRewardEntity.Row row)
        {
            var partition = new TokenRewardEntity.PartitionKey(guildId);
            await tables.DeleteAsync(TokenRewardEntity.TableName, partition.ToString(), row.ToString());
        }

        public static TokenRewardEntity[] GetTokenRewardDefinitions(this ITableStorageClient tables, ulong guildId)
        {
            return tables.GetFirstPageOfEntities<TokenRewardEntity>(TokenRewardEntity.TableName, new TokenRewardEntity.PartitionKey(guildId).ToString());
        }

        public static string[] GetCreatorAddressesForGuild(this ITableStorageClient tables, ulong guildId) 
        { 
            return tables.GetTokenRewardDefinitions(guildId).Select(tr => tr.CreatorAddress).Distinct().ToArray();
        }
    }
}
