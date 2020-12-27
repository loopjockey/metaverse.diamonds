using System.Threading.Tasks;

namespace Metaverse.Bot.Data
{
    public static class TableStorageClientExtensions
    {
        public static async Task SetGuildConfigurationPair(this ITableStorageClient tables, ulong guildId, string key, string value) 
        {
            await tables.InsertOrReplaceAsync(GuildConfigurationEntity.TableName, new GuildConfigurationEntity(guildId, key, value));
        }

        public static async Task AddTokenReward(this ITableStorageClient tables, ulong guildId, TokenRewardEntity.Row row) 
        {
            await tables.InsertOrReplaceAsync(TokenRewardEntity.TableName, new TokenRewardEntity(guildId, row));
        }
    }
}
