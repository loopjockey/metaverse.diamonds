using Discord;
using Discord.Rest;
using Metaverse.Functions.Common.Configuration;
using Metaverse.Functions.Common.Extensions;
using Metaverse.Functions.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Metaverse.Functions.Queue
{
    public class ApplyRoleToUserFunction
    {
        private readonly ITableStorageClient _tableStorageClient;
        private readonly DiscordRestClient _discordClient;
        private readonly BotTokenSetting _botTokenSetting;

        public const string QueueName = "apply-user-role";

        public ApplyRoleToUserFunction(ITableStorageClient tableStorageClient, DiscordRestClient discordClient, BotTokenSetting botTokenSetting)
        {
            _tableStorageClient = tableStorageClient;
            _discordClient = discordClient;
            _botTokenSetting = botTokenSetting;
        }

        [FunctionName(nameof(ApplyRoleToUser))]
        public async Task ApplyRoleToUser(
            [QueueTrigger(QueueName)] BusCommand model,
            ILogger log) 
        {
            var session = await _discordClient.ScopedLoginAsync(TokenType.Bot, _botTokenSetting);
            await using (session)
            {
                var guild = await _discordClient.GetGuildAsync(model.GuildId);
                if (guild == null) return;
                var guildRole = guild.GetRole(model.RoleId);
                if (guildRole == null) return;

                var guildUser = await _discordClient.GetGuildUserAsync(model.GuildId, model.UserId);
                await guildUser.AddRoleAsync(guildRole);
            }
        }

        public class BusCommand 
        {
            public BusCommand() { }

            public BusCommand(ulong guildId, ulong roleId, ulong userId) 
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
}
