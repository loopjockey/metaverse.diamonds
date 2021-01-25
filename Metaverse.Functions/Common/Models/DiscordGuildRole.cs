namespace Metaverse.Functions.Models
{
    public class DiscordGuildRole 
    {
        public DiscordGuildRole() { }

        public DiscordGuildRole(ulong roleId, string roleName) 
        {
            Id = roleId.ToString();
            Name = roleName;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
