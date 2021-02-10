namespace Metaverse.Functions.Models
{
    public class DiscordGuildRole 
    {
        public DiscordGuildRole() { }

        public DiscordGuildRole(ulong roleId, string roleName, string colour, bool isAdministrator)
        {
            Id = roleId.ToString();
            Name = roleName;
            Colour = colour;
            IsAdministrator = isAdministrator;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Colour { get; set; }
        public bool IsAdministrator { get; set; }
    }
}
