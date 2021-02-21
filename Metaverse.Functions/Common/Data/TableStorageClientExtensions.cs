using System;
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

        public static async Task AddTokenRewardAsync(this ITableStorageClient tables, ulong guildId, ulong targetRoleId, string collectionId)
        {
            await tables.InsertOrReplaceAsync(GuildRewardEntity.TableName, new GuildRewardEntity(guildId, DateTimeOffset.UtcNow, targetRoleId, collectionId));
        }

        public static async Task RemoveTokenRewardAsync(this ITableStorageClient tables, ulong guildId, ulong targetRoleId, string collectionId)
        {
            var partition = new GuildRewardEntity.Partition(guildId);
            var rowKey = new GuildRewardEntity.Row(targetRoleId, collectionId);
            await tables.DeleteAsync(GuildRewardEntity.TableName, partition.ToString(), rowKey.ToString());
        }

        public static GuildRewardEntity[] GetTokenRewardDefinitions(this ITableStorageClient tables, ulong guildId)
        {
            return tables.GetFirstPageOfEntities<GuildRewardEntity>(GuildRewardEntity.TableName, new GuildRewardEntity.Partition(guildId).ToString());
        }

        public static string[] GetCreatorAddressesForGuild(this ITableStorageClient tables, ulong guildId) 
        { 
            return tables.GetTokenRewardDefinitions(guildId).Select(tr => tr.CollectionId).Distinct().ToArray();
        }
    }
}
