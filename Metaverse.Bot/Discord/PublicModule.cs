using Discord;
using Discord.Commands;
using Metaverse.Bot.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using Nethereum.Util;

namespace Metaverse.Bot.Discord
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        public TableStorageClient TableStorage { get; set; }

        [Command("ping")]
        public Task PingAsync() => ReplyAsync("pong!");

        [Command("help")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
        public Task GuildHelpAsync() => ReplyAsync($"Hey @{Context.User.Username}! You can find all the information you need at https://server.metaverse.diamonds/{Context.Guild.Id}");

        [Command("setup")]
        public Task GuildSetupAsync() => ReplyAsync($"Hey @{Context.User.Username}! You can find all the server setup information you need at https://setup.metaverse.diamonds");

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
            if (key != GuildConfigurationEntity.KnownConfigurationKeys.ShopUrl) 
            {
                await ReplyAsync($"Unsupported configuration key '{key}'. Supported keys: {GuildConfigurationEntity.KnownConfigurationKeys.ShopUrl}");
                return;
            }
            var value = parameters[1];
            if (!Uri.TryCreate(value, UriKind.Absolute, out _)) 
            {
                await ReplyAsync($"The supplied {GuildConfigurationEntity.KnownConfigurationKeys.ShopUrl} is not a valid URL. Please make sure it's a fully qualified valid URI (e.g. https://app.rarible.com/myshopname/collectibles)");
                return;
            }

            await TableStorage.SetGuildConfigurationPair(Context.Guild.Id, key, value);
            await ReplyAsync("Configuration updated! When you are fully setup please refer your server to https://server.metaverse.diamonds/{Context.Guild.Id} or ask them to enter `!metaverse help` for information on how to use their NFTs.");
        }

        [Command("rewards add")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
        [RequireOwner]
        public async Task AddRewards(params string[] parameters) 
        {
            if (parameters.Length < 3) 
            {
                await ReplyAsync("You need to supply a creator address, token reference and reward role. E.g. `!metaverse rewards add 0x...123 * @Administrators`");
                return;
            }
            var creatorAddress = parameters[0];
            if (!AddressUtil.Current.IsValidEthereumAddressHexFormat(creatorAddress))
            {
                await ReplyAsync($"The creator address supplied '{creatorAddress}' was not a valid ethereum address. Ethereum addresses should start with 0x amongst other conditions (We do not support ENS addresses yet).");
                return;
            }

            var tokenReference = parameters[1];
            if (!TokenRewardEntity.Row.TryParse(tokenReference, creatorAddress, DateTimeOffset.UtcNow, out var row)) 
            {
                await ReplyAsync($"The supplied token reference '{tokenReference}' is not valid. Token references can either be '*' denoting all tokens for a creator bear the reward. Alternatively they can be a specific token id or a range, e.g. '123-456' meaning all tokens between ids 123 and 456 bear the reward. This is all pretty complicated, in most scenarios you will really just need: *.");
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

            await TableStorage.AddTokenReward(Context.Guild.Id, row);
            await ReplyAsync("Configuration updated! When you are fully setup please refer your server to https://server.metaverse.diamonds/{Context.Guild.Id} or ask them to enter `!metaverse help` for information on how to use their NFTs.");
        }

        [Command("verify")]
        public async Task VerifyOwnership(params string[] parameters) 
        { 
        
        }
    }
}
