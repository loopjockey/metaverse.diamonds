using Discord.Rest;

namespace Metaverse.Functions.Models
{
    public class DiscordGuild
    {
        public DiscordGuild() { }

        public DiscordGuild(RestUserGuild restGuild) : this(
            restGuild.Id, 
            restGuild.Name, 
            restGuild.IconUrl, 
            restGuild.IsOwner)
        {
        }

        public DiscordGuild(ulong guildId, string guildName, string guildAvatarUrl, bool isOwner) 
        {
            Id = guildId.ToString();
            Name = guildName;
            AvatarUrl = guildAvatarUrl;
            IsOwner = isOwner;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsOwner { get; set; }
    }
}
