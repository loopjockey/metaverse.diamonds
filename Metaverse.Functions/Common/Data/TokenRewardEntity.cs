using Microsoft.Azure.Cosmos.Table;
using System;
using System.Numerics;

namespace Metaverse.Functions.Data
{
    public class TokenRewardEntity : TableEntity
    {
        public TokenRewardEntity() { }

        public TokenRewardEntity(ulong guildId, DateTimeOffset ruleCreatedDate, Row row) : base(new PartitionKey(guildId).ToString(), row.ToString())
        {
            GuildId_S = guildId.ToString();
            TargetRoleId_S = row.TargetRoleId.ToString();
            TargetTokenId_S = row.TargetTokenId?.ToString();
            MinimumTokenId_S = row.MinimumTokenId?.ToString();
            MaximumTokenId_S = row.MaximumTokenId?.ToString();
            IsAllTokens = row.IsAllTokens;
            RuleCreatedDate = ruleCreatedDate;
            CreatorAddress = row.CreatorAddress;
            TokenReference = row.TokenReference;
        }

        public string GuildId_S { get; set; }
        [IgnoreProperty] public ulong GuildId => ulong.TryParse(GuildId_S, out var guildId) ? guildId : default;

        public string TargetRoleId_S { get; set; }
        [IgnoreProperty] public ulong TargetRoleId => ulong.TryParse(TargetRoleId_S, out var roleId) ? roleId : default;

        public string TargetTokenId_S { get; set; }
        [IgnoreProperty] public BigInteger? TargetTokenId => string.IsNullOrWhiteSpace(TargetTokenId_S) ? (BigInteger?)null : BigInteger.Parse(TargetTokenId_S);

        public string MinimumTokenId_S { get; set; }
        [IgnoreProperty] public BigInteger? MinimumTokenId => string.IsNullOrWhiteSpace(MinimumTokenId_S) ? (BigInteger?)null : BigInteger.Parse(MinimumTokenId_S);

        public string MaximumTokenId_S { get; set; }
        [IgnoreProperty] public BigInteger? MaximumTokenId => string.IsNullOrWhiteSpace(MaximumTokenId_S) ? (BigInteger?)null : BigInteger.Parse(MaximumTokenId_S);

        public bool IsAllTokens { get; set; } = false;
        public string TokenReference { get; set; }

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

            public Row(ulong targetRoleId, string creatorAddress)
            {
                TokenReference = "*";
                _rowKeyString = $"{creatorAddress}:{TokenReference}:{targetRoleId}";
                CreatorAddress = creatorAddress;
                IsAllTokens = true;
                TargetRoleId = targetRoleId;
            }

            public Row(ulong targetRoleId, string creatorAddress, BigInteger tokenId)
            {
                TokenReference = $"{tokenId}";
                _rowKeyString = $"{creatorAddress}:{TokenReference}:{targetRoleId}";
                CreatorAddress = creatorAddress;
                TargetTokenId = tokenId;
                TargetRoleId = targetRoleId;
            }

            public Row(ulong targetRoleId, string creatorAddress, BigInteger minimumTokenId, BigInteger maximumTokenId)
            {
                TokenReference = $"{minimumTokenId}-{maximumTokenId}";
                _rowKeyString = $"{creatorAddress}:{TokenReference}:{targetRoleId}";
                CreatorAddress = creatorAddress;
                MinimumTokenId = minimumTokenId;
                MaximumTokenId = maximumTokenId;
                TargetRoleId = targetRoleId;
            }

            public BigInteger? TargetTokenId { get; set; }
            public BigInteger? MinimumTokenId { get; set; }
            public BigInteger? MaximumTokenId { get; set; }
            public bool IsAllTokens { get; set; } = false;
            public ulong TargetRoleId { get; set; }
            public string CreatorAddress { get; set; }
            public string TokenReference { get; set; }

            public override string ToString()
            {
                return _rowKeyString.ToString();
            }
        }
    }
}
