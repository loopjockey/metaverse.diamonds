using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metaverse.Functions.Models
{
    public class DiscordGuild
    {
        public DiscordGuild() { }

        public DiscordGuild(RestGuild restGuild, ulong currentUserId) : this(
            restGuild.Id, 
            restGuild.Name, 
            restGuild.IconUrl, 
            currentUserId == restGuild.OwnerId, 
            from r in restGuild.Roles
            select new DiscordGuildRole(r.Id, r.Name))
        {
        }

        public DiscordGuild(ulong guildId, string guildName, string guildAvatarUrl, bool isOwner, IEnumerable<DiscordGuildRole> roles) 
        {
            Id = guildId;
            Name = guildName;
            AvatarUrl = guildAvatarUrl;
            IsOwner = isOwner;
            Roles = roles.ToArray();
        }

        public ulong Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsOwner { get; set; }
        public DiscordGuildRole[] Roles { get; set; } = Array.Empty<DiscordGuildRole>();
    }
}
