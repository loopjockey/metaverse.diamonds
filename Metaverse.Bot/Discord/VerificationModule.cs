using Discord.Commands;
using Metaverse.Bot.Data;
using System.Threading.Tasks;
using Metaverse.Bot.Ethereum;
using System.Linq;

namespace Metaverse.Bot.Discord
{
    public class VerificationModule : ModuleBase<SocketCommandContext>
    {
        public TableStorageClient Tables { get; set; }
        public ERC721Client ERC721Client { get; set; }

        /// <summary>
        /// https://danfinlay.github.io/js-eth-personal-sign-examples/
        /// </summary>
        [Command("verify")]
        public async Task VerifyOwnership(params string[] parameters)
        {
            if (parameters.Length != 2)
            {
                await ReplyAsync("There are not enough parameters in this verification message. These should not be constructed manually. If you have been directed here by some other means outside of https://metaverse.diamonds please turn back.");
                return;
            }

            var signature = parameters[0];
            var message = parameters[1];
            if (!VerificationMessageParser.TryParse(message, out var messageModel))
            {
                await ReplyAsync("The message provided is not valid. These should not be constructed manually. If you have been directed here by some other means outside of https://metaverse.diamonds please turn back.");
                return;
            }

            var guild = Context.Client.Guilds.FirstOrDefault(g => g.Id == messageModel.GuildId);
            if (guild == default) return; // This bot isn't connected to the guild requested.

            var user = guild.GetUser(Context.User.Id);
            if (user == default) return; // The user making this request isn't in the guild.

            if (!VerificationMessageParser.TryParsePath(messageModel.Path, out var erc721Token))
            {
                await ReplyAsync("The message provided is not valid. These should not be constructed manually. If you have been directed here by some other means outside of https://metaverse.diamonds please turn back.");
                return;
            }

            var messageString = $"I wish to use the ERC721 token {messageModel.Path} for the purposes of receiving elevated permissions in the discord server #{messageModel.GuildId}. This signature is valid as of {messageModel.OriginalTicks}.";
            var usersAddress = SignatureValidator.GetAddressOfSignature(signature, message);
            var ownershipAddress = await ERC721Client.GetAddressThatOwnsToken(erc721Token.creatorAddress, erc721Token.tokenId);
            if (usersAddress != ownershipAddress)
            {
                await ReplyAsync("The address that signed this message does not own the token described.");
                return;
            }

            var tokenRewards = Tables.GetTokenRewardDefinitions(messageModel.GuildId);
            var lastApplicableRule = tokenRewards.Where(r => r.AppliesTo(erc721Token.creatorAddress, erc721Token.tokenId)).LastOrDefault();
            if (lastApplicableRule == default)
            {
                await ReplyAsync($"There are no rules that apply to the token {erc721Token.creatorAddress}:{erc721Token.tokenId}. Please contact the administrator of this server");
                return;
            }

            var applicableRole = guild.GetRole(lastApplicableRule.TargetRoleId);
            if (applicableRole == default) return;
            await user.AddRoleAsync(applicableRole);
            await ReplyAsync($"Success! You've been added to the role {applicableRole.Name} in the guild {guild.Name}.");
        }
    }
}
