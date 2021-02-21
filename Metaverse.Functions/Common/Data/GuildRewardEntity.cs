using Microsoft.Azure.Cosmos.Table;
using System;

namespace Metaverse.Functions.Data
{
    public class GuildRewardEntity : TableEntity
    {
        public GuildRewardEntity() { }

        public GuildRewardEntity(ulong guildId, DateTimeOffset ruleCreatedDate, ulong targetRoleId, string collectionId) :
            base(new Partition(guildId).ToString(), new Row(targetRoleId, collectionId).ToString())
        {
            GuildId_S = guildId.ToString();
            TargetRoleId_S = targetRoleId.ToString();
            RuleCreatedDate = ruleCreatedDate;
            CollectionId = collectionId;
            CollectionSource = "opensea";
        }

        public string GuildId_S { get; set; }
        [IgnoreProperty] public ulong GuildId => ulong.TryParse(GuildId_S, out var guildId) ? guildId : default;
        public string TargetRoleId_S { get; set; }
        [IgnoreProperty] public ulong TargetRoleId => ulong.TryParse(TargetRoleId_S, out var roleId) ? roleId : default;
        public string CollectionId { get; set; }
        public string CollectionSource { get; set; }
        public DateTimeOffset RuleCreatedDate { get; set; }
        public const string TableName = "GuildRewards";

        public class Partition
        {
            public Partition(ulong guildId)
            {
                GuildId = guildId;
            }

            public ulong GuildId { get; }

            public override string ToString()
            {
                return GuildId.ToString();
            }
        }

        public class Row
        {
            private readonly string _rowKeyString;

            public Row(ulong targetRoleId, string collectionId)
            {
                _rowKeyString = $"{targetRoleId}:{collectionId}";
                CollectionId = collectionId;
                TargetRoleId = targetRoleId;
            }

            public ulong TargetRoleId { get; }
            public string CollectionId { get; }

            public override string ToString()
            {
                return _rowKeyString.ToString();
            }
        }
    }
}
