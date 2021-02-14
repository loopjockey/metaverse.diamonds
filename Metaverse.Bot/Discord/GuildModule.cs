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

        [Command("config")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
        [RequireOwner]
        public async Task GuildSetupAsync(params string[] parameters)
        {
            if (parameters.Length < 2)
            {
                await ReplyAsync("You need to supply a configuration key and value. E.g. `!metaverse config shop_url https://app.rarible.com/myshopname/collectibles`");
                return;
            }
            var key = parameters[0];
            if (key != "shop_url")
            {
                await ReplyAsync($"Unsupported configuration key '{key}'. Supported keys: shop_url");
                return;
            }
            var value = parameters[1];
            if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            {
                await ReplyAsync($"The supplied shop_url is not a valid URL. Please make sure it's a fully qualified valid URI (e.g. https://app.rarible.com/myshopname/collectibles)");
                return;
            }

            await QueueFactory(UpdateGuildConfigurationCommand.QueueName)
                .SendJsonMessageAsync(new UpdateGuildConfigurationCommand(Context.Guild.Id, key, value));
            await ReplyAsync($"Configuration updated! When you are fully setup please refer your server to https://server.metaverse.diamonds/{Context.Guild.Id} or ask them to enter `!metaverse help` for information on how to use their NFTs.");
        }

        [Command("rewards")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
        [RequireOwner]
        public async Task UpdateRewards(params string[] parameters)
        {
            var action = parameters[0];
            if (action != "add" && action != "remove")
            {
                await ReplyAsync($"The action must be either 'add' or 'remove' but it was {action}");
            }

            if (parameters.Length < 4)
            {
                await ReplyAsync("You need to supply a creator address, token reference and reward role. E.g. `!metaverse rewards add 0x...123 * @Administrators`");
                return;
            }
            var creatorAddress = parameters[1];
            if (!AddressUtil.Current.IsValidEthereumAddressHexFormat(creatorAddress))
            {
                await ReplyAsync($"The creator address supplied '{creatorAddress}' was not a valid ethereum address. Ethereum addresses should start with 0x amongst other conditions (We do not support ENS addresses yet).");
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

            var tokenReference = parameters[2];
            if (!GuildReferenceCommand.TryParse(tokenReference, creatorAddress, Context.Guild.Id, rewardRole.Id, out var command))
            {
                await ReplyAsync($"The supplied token reference '{tokenReference}' is not valid. Token references can either be '*' denoting all tokens for a creator bear the reward. Alternatively they can be a specific token id or a range, e.g. '123-456' meaning all tokens between ids 123 and 456 bear the reward. This is all pretty complicated, in most scenarios you will really just need: *.");
                return;
            }

            if (action == "add")
            {
                await QueueFactory(GuildReferenceCommand.AddRewardQueueName).SendJsonMessageAsync(command);
                await ReplyAsync($"Reward successfully added. If you want to delete this reward update this message from 'add' to 'remove'.");
            }
            else if (action == "remove") 
            {
                await QueueFactory(GuildReferenceCommand.RemoveRewardQueueName).SendJsonMessageAsync(command);
                await ReplyAsync($"Reward successfully removed.");
            }
           
        }
    }
}
