using System;
using System.Numerics;

namespace Metaverse.Core
{
    public class GuildRewardReferenceCommand
    {
        public const string AddRewardQueueName = "add-guild-reward";
        public const string RemoveRewardQueueName = "remove-guild-reward";

        public GuildRewardReferenceCommand() { }

        public GuildRewardReferenceCommand(ulong guildId, ulong rewardRoleId, string collectionId)
        {
            GuildId = guildId;
            RewardRoleId = rewardRoleId;
            CollectionId = collectionId;
        }

        public ulong GuildId { get; set; }
        public ulong RewardRoleId { get; set; }
        public string CollectionId { get; set; }
    }
}
