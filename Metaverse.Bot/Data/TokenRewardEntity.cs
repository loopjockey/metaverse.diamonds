using Microsoft.Azure.Cosmos.Table;
using System;

namespace Metaverse.Bot.Data
{
    public class TokenRewardEntity : TableEntity
    {
        public TokenRewardEntity() { }

        public TokenRewardEntity(ulong guildId, Row row) : base(new PartitionKey(guildId).ToString(), row.ToString())
        {
            GuildId = guildId;
            TargetRoleId = row.TargetRoleId;
            TargetTokenId = row.TargetTokenId;
            MinimumTokenId = row.MinimumTokenId;
            MaximumTokenId = row.MaximumTokenId;
            IsAllTokens = row.IsAllTokens;
            RuleCreatedDate = row.RuleCreatedDate;
            CreatorAddress = row.CreatorAddress;
        }

        public ulong GuildId { get; set; }
        public ulong TargetRoleId { get; set; }
        public ulong? TargetTokenId { get; set; }
        public ulong? MinimumTokenId { get; set; }
        public ulong? MaximumTokenId { get; set; }
        public bool IsAllTokens { get; set; } = false;
        
        public DateTimeOffset RuleCreatedDate { get; set; }
        public string CreatorAddress { get; set; }

        public const string TableName = "TokenRewards";

        public class PartitionKey
        {
            public PartitionKey(ulong guildId)
            {
                GuildId = guildId;
            }

            public ulong GuildId { get; }

            public override string ToString()
            {
                return GuildId.ToString();
            }
        }

        /// <summary>
        /// Rules can be in the form...
        /// 0x...123:*
        /// 0x...123:456
        /// 0x...123:456-789
        /// </summary>
        public class Row
        {
            private readonly string _rowKeyString;

            public Row(string creatorAddress, DateTimeOffset ruleCreatedDate)
            {
                _rowKeyString = $"{ruleCreatedDate.Ticks}:{creatorAddress}:*";
                CreatorAddress = creatorAddress;
                RuleCreatedDate = ruleCreatedDate;
                IsAllTokens = true;
            }

            public Row(string creatorAddress, ulong tokenId, DateTimeOffset ruleCreatedDate)
            {
                _rowKeyString = $"{ruleCreatedDate.Ticks}:{creatorAddress}:{tokenId}";
                CreatorAddress = creatorAddress;
                RuleCreatedDate = ruleCreatedDate;
                TargetTokenId = tokenId;
            }

            public Row(string creatorAddress, ulong minimumTokenId, ulong maximumTokenId, DateTimeOffset ruleCreatedDate)
            {
                _rowKeyString = $"{ruleCreatedDate.Ticks}:{creatorAddress}:{minimumTokenId}-{maximumTokenId}";
                CreatorAddress = creatorAddress;
                RuleCreatedDate = ruleCreatedDate;
                MinimumTokenId = minimumTokenId;
                MaximumTokenId = maximumTokenId;
            }

            public ulong? TargetTokenId { get; set; }
            public ulong? MinimumTokenId { get; set; }
            public ulong? MaximumTokenId { get; set; }
            public bool IsAllTokens { get; set; } = false;
            public ulong TargetRoleId { get; set; }
            public DateTimeOffset RuleCreatedDate { get; set; }
            public string CreatorAddress { get; set; }

            public override string ToString()
            {
                return _rowKeyString.ToString();
            }

            public static bool TryParse(string tokenReferencePart, string creatorAddress, DateTimeOffset ruleCreatedDate, out Row row) 
            {
                if (tokenReferencePart == "*") 
                {
                    row = new Row(creatorAddress, ruleCreatedDate);
                    return true;
                }
                if (ulong.TryParse(tokenReferencePart, out var tokenId)) 
                {
                    row = new Row(creatorAddress, tokenId, ruleCreatedDate);
                    return true;
                }
                var range = tokenReferencePart.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (range.Length == 2 &&
                    ulong.TryParse(range[0], out var minimumTokenId) &&
                    ulong.TryParse(range[1], out var maximumTokenId))
                {
                    row = new Row(creatorAddress, minimumTokenId, maximumTokenId, ruleCreatedDate);
                    return true;
                }

                row = null;
                return false;
            }
        }
    }
}
