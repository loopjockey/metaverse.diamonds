using Microsoft.Azure.Cosmos.Table;
using System;

namespace Metaverse.Functions.Common.Data
{
    public class TokenUsageEntity : TableEntity
    {
        public TokenUsageEntity() { }

        public TokenUsageEntity(string tokenId, ulong discordUserId, ulong effectiveGuildId, ulong effectiveRoleId, DateTimeOffset usageDate) 
            : base(new Partition(effectiveGuildId).ToString(), new Row(tokenId, usageDate, discordUserId).ToString())
        {
            TokenId = tokenId;
            UserId_S = discordUserId.ToString();
            EffectiveGuildId_S = effectiveGuildId.ToString();
            EffectiveRoleId_S = effectiveRoleId.ToString();
            UsageDate = usageDate;
        }

        public string TokenId { get; set; } // {contractAddr}:{tokenId}
        [IgnoreProperty] public ulong UserId => ulong.TryParse(UserId_S, out var guildId) ? guildId : default;
        public string UserId_S { get; set; }
        [IgnoreProperty] public ulong EffectiveGuildId => ulong.TryParse(EffectiveGuildId_S, out var guildId) ? guildId : default;
        public string EffectiveGuildId_S { get; set; }
        [IgnoreProperty] public ulong EffectiveRoleId => ulong.TryParse(EffectiveRoleId_S, out var guildId) ? guildId : default;
        public string EffectiveRoleId_S { get; set; }
        public DateTimeOffset UsageDate { get; set; }

        public class Partition
        {
            public Partition(ulong effectiveGuildId) 
            {
                EffectiveGuildId = effectiveGuildId;
            }

            public ulong EffectiveGuildId { get; }

            public override string ToString()
            {
                return $"{EffectiveGuildId}";
            }
        }

        public class Row 
        {
            public Row(string tokenId, DateTimeOffset usageDate, ulong discordUserId) 
            {
                TokenId = tokenId;
                UsageDate = usageDate;
                DiscordUserId = discordUserId;
            }

            public string TokenId { get; }
            public DateTimeOffset UsageDate { get; }
            public ulong DiscordUserId { get; }

            public override string ToString()
            {
                return $"{TokenId}:{(DateTimeOffset.MaxValue - UsageDate).Ticks}:{DiscordUserId}";
            }
        }
    }
}
