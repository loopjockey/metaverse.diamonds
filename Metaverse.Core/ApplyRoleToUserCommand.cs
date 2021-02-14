namespace Metaverse.Core
{
    public class ApplyRoleToUserCommand
    {
        public const string QueueName = "apply-user-role";

        public ApplyRoleToUserCommand() { }

        public ApplyRoleToUserCommand(ulong guildId, ulong roleId, ulong userId)
        {
            GuildId = guildId;
            RoleId = roleId;
            UserId = userId;
        }

        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public ulong UserId { get; set; }
    }
}
