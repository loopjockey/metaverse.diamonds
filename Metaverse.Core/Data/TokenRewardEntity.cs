using Microsoft.Azure.Cosmos.Table;
using System;
using System.Numerics;

namespace Metaverse.Bot.Data
{
    public class TokenRewardEntity : TableEntity
    {
        public TokenRewardEntity() { }

        public TokenRewardEntity(ulong guildId, Row row) : base(new PartitionKey(guildId).ToString(), row.ToString())
        {
            GuildId = guildId;
            TargetRoleId = row.TargetRoleId;
            TargetTokenId_S = row.TargetTokenId?.ToString();
            MinimumTokenId_S = row.MinimumTokenId?.ToString();
            MaximumTokenId_S = row.MaximumTokenId?.ToString();
            IsAllTokens = row.IsAllTokens;
            RuleCreatedDate = row.RuleCreatedDate;
            CreatorAddress = row.CreatorAddress;
        }

        public ulong GuildId { get; set; }
        public ulong TargetRoleId { get; set; }
        public string TargetTokenId_S { get; set; }
        [IgnoreProperty] public BigInteger? TargetTokenId => string.IsNullOrWhiteSpace(TargetTokenId_S) ? (BigInteger?)null : BigInteger.Parse(TargetTokenId_S);

        public string MinimumTokenId_S { get; set; }
        [IgnoreProperty] public BigInteger? MinimumTokenId => string.IsNullOrWhiteSpace(MinimumTokenId_S) ? (BigInteger?)null : BigInteger.Parse(MinimumTokenId_S);

        public string MaximumTokenId_S { get; set; }
        [IgnoreProperty] public BigInteger? MaximumTokenId => string.IsNullOrWhiteSpace(MaximumTokenId_S) ? (BigInteger?)null : BigInteger.Parse(MaximumTokenId_S);

        public bool IsAllTokens { get; set; } = false;
        
        public DateTimeOffset RuleCreatedDate { get; set; }
        public string CreatorAddress { get; set; }

        public const string TableName = "TokenRewards";

        public bool AppliesTo(string creatorAddress, BigInteger tokenId) 
        {
            if (creatorAddress != CreatorAddress) return false;
            if (IsAllTokens) return true;
            if (TargetTokenId == tokenId) return true;
            if (!MinimumTokenId.HasValue) return false;
            if (!MaximumTokenId.HasValue) return false;
            return tokenId <= MaximumTokenId &&
                   tokenId >= MinimumTokenId;
        }

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

            public Row(string creatorAddress, BigInteger tokenId, DateTimeOffset ruleCreatedDate)
            {
                _rowKeyString = $"{ruleCreatedDate.Ticks}:{creatorAddress}:{tokenId}";
                CreatorAddress = creatorAddress;
                RuleCreatedDate = ruleCreatedDate;
                TargetTokenId = tokenId;
            }

            public Row(string creatorAddress, BigInteger minimumTokenId, BigInteger maximumTokenId, DateTimeOffset ruleCreatedDate)
            {
                _rowKeyString = $"{ruleCreatedDate.Ticks}:{creatorAddress}:{minimumTokenId}-{maximumTokenId}";
                CreatorAddress = creatorAddress;
                RuleCreatedDate = ruleCreatedDate;
                MinimumTokenId = minimumTokenId;
                MaximumTokenId = maximumTokenId;
            }

            public BigInteger? TargetTokenId { get; set; }
            public BigInteger? MinimumTokenId { get; set; }
            public BigInteger? MaximumTokenId { get; set; }
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
                if (BigInteger.TryParse(tokenReferencePart, out var tokenId)) 
                {
                    row = new Row(creatorAddress, tokenId, ruleCreatedDate);
                    return true;
                }
                var range = tokenReferencePart.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (range.Length == 2 &&
                    BigInteger.TryParse(range[0], out var minimumTokenId) &&
                    BigInteger.TryParse(range[1], out var maximumTokenId))
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
