using System;
using System.Numerics;

namespace Metaverse.Core
{
    public class GuildReferenceCommand
    {
        public const string AddRewardQueueName = "add-guild-reward";
        public const string RemoveRewardQueueName = "remove-guild-reward";

        public GuildReferenceCommand() { }

        public GuildReferenceCommand(ulong guildId, ulong rewardRoleId, string creatorAddress)
        {
            GuildId = guildId;
            RewardRoleId = rewardRoleId;
            CreatorAddress = creatorAddress;
            RewardDefinition = RewardDefinitionType.All;
        }

        public GuildReferenceCommand(ulong guildId, ulong rewardRoleId, string creatorAddress, BigInteger specificTokenId)
        {
            GuildId = guildId;
            RewardRoleId = rewardRoleId;
            CreatorAddress = creatorAddress;
            SpecificTokenId = specificTokenId;
            RewardDefinition = RewardDefinitionType.Specific;
        }

        public GuildReferenceCommand(ulong guildId, ulong rewardRoleId, string creatorAddress, BigInteger minimumTokenId, BigInteger maximumTokenId)
        {
            GuildId = guildId;
            RewardRoleId = rewardRoleId;
            CreatorAddress = creatorAddress;
            MinimumTokenId = minimumTokenId;
            MaximumTokenId = maximumTokenId;
            RewardDefinition = RewardDefinitionType.Range;
        }

        public ulong GuildId { get; set; }
        public ulong RewardRoleId { get; set; }
        public string CreatorAddress { get; set; }
        public BigInteger? SpecificTokenId { get; set; }
        public BigInteger? MinimumTokenId { get; set; }
        public BigInteger? MaximumTokenId { get; set; }
        public RewardDefinitionType RewardDefinition { get; set; }

        public enum RewardDefinitionType 
        { 
            All,
            Specific,
            Range
        }

        public static bool TryParse(string tokenReferencePart, string creatorAddress, ulong guildId, ulong rewardRoleId, out GuildReferenceCommand row)
        {
            if (tokenReferencePart == "*")
            {
                row = new GuildReferenceCommand(guildId, rewardRoleId, creatorAddress);
                return true;
            }
            if (BigInteger.TryParse(tokenReferencePart, out var tokenId))
            {
                row = new GuildReferenceCommand(guildId, rewardRoleId, creatorAddress, tokenId);
                return true;
            }
            var range = tokenReferencePart.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (range.Length == 2 &&
                BigInteger.TryParse(range[0], out var minimumTokenId) &&
                BigInteger.TryParse(range[1], out var maximumTokenId))
            {
                row = new GuildReferenceCommand(guildId, rewardRoleId, creatorAddress, minimumTokenId, maximumTokenId);
                return true;
            }

            row = null;
            return false;
        }
    }
}
