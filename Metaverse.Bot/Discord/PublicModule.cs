using Discord.Commands;
using System.Threading.Tasks;

namespace Metaverse.Bot.Discord
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public Task PingAsync() => ReplyAsync("pong!");

        [Command("help")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
        public Task GuildHelpAsync() => ReplyAsync($"Hey @{Context.User.Username}! You can find everything you need at https://guilds.metaverse.diamonds/{Context.Guild.Id}");

        [Command("setup")]
        public Task GuildSetupAsync() => ReplyAsync($"Hey @{Context.User.Username}! You can find all the server setup information you need at https://setup.metaverse.diamonds");
    }
}
