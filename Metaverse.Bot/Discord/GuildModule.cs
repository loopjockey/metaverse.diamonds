using Azure.Storage.Queues;
using Discord.Commands;
using Metaverse.Bot.Common;
using Metaverse.Core;
using Nethereum.Util;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Metaverse.Bot.Discord
{
    public class GuildModule : ModuleBase<SocketCommandContext>
    {
        public Func<string, QueueClient> QueueFactory { get; set; }

        [Command("rewards")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
        [RequireOwner]
        public async Task UpdateRewards(params string[] parameters)
        {
            // !metaverse rewards add opensea {collectionID} {roleReference}
            if (parameters.Length < 4)
            {
                await ReplyAsync("You need to supply a creator address, token reference and reward role. E.g. `!metaverse rewards add 0x...123 * @Administrators`");
                return;
            }
            var action = parameters[0];
            var collectionType = parameters[1];
            var collectionId = parameters[2];

            if (action != "add" && action != "remove")
            {
                await ReplyAsync($"The action must be either 'add' or 'remove' but it was {action}");
                return;
            }

            if (collectionType != "opensea") {
                await ReplyAsync($"Only collections from opensea are currently supported. Please update your command from {collectionType} to: opensea");
                return;
            }

            if (Context.Message.MentionedRoles.Count == 0)
            {
                await ReplyAsync("Please mention the role you want to be the reward for purchasing the specified NFT group. e.g. @Administrators");
                return;
            }

            if (Context.Message.MentionedRoles.Count > 1)
            {
                await ReplyAsync("Only one role is currently supported as a reward for token ownership.");
                return;
            }

            var rewardRole = Context.Message.MentionedRoles.Single();
            var command = new GuildRewardReferenceCommand(Context.Guild.Id, rewardRole.Id, collectionId);

            if (action == "add")
            {
                await QueueFactory(GuildRewardReferenceCommand.AddRewardQueueName).SendJsonMessageAsync(command);
                await ReplyAsync($"Reward successfully added. If you want to delete this reward update this message from 'add' to 'remove'.");
            }
            else if (action == "remove") 
            {
                await QueueFactory(GuildRewardReferenceCommand.RemoveRewardQueueName).SendJsonMessageAsync(command);
                await ReplyAsync($"Reward successfully removed.");
            }
           
        }
    }
}
