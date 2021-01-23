namespace Metaverse.Functions.Models
{
    public class DiscordGuildRole 
    {
        public DiscordGuildRole() { }

        public DiscordGuildRole(ulong roleId, string roleName) 
        {
            Id = roleId;
            Name = roleName;
        }

        public ulong Id { get; set; }
        public string Name { get; set; }
    }
}
