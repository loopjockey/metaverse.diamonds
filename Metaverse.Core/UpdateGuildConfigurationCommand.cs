namespace Metaverse.Core
{
    public class UpdateGuildConfigurationCommand
    {
        public const string QueueName = "update-guild-configuration";

        public UpdateGuildConfigurationCommand() { }

        public UpdateGuildConfigurationCommand(ulong guildId, string key, string value)
        {
            GuildId = guildId;
            Key = key;
            Value = value;
        }

        public ulong GuildId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
