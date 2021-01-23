using System;
using System.Linq;
using System.Numerics;

namespace Metaverse.Core.Ethereum
{
    public static class VerificationMessageParser
    {
        private static readonly string[] SupportedDomains = new[] { "metaverse.diamonds" };
        private static readonly string[] SupportedVersions = new[] { "v1" };
        private static readonly string[] SupportedActions = new[] { "use_nft" };

        public static bool TryParse(string verificationMessage, out VerificationMessageModel messageModel)
        {
            var messageParts = verificationMessage.Split('|');
            if (messageParts.Length != 8)
            {
                messageModel = null;
                return false;
            }

            var domain = messageParts[0];
            var version = messageParts[1];
            var action = messageParts[2];
            var path = messageParts[3];
            var guildId = messageParts[4];
            var userId = messageParts[5];
            var roleId = messageParts[6];
            var ticks = messageParts[7];

            if (!ulong.TryParse(guildId, out var gGuildId) ||
                !ulong.TryParse(userId, out var gUserId) ||
                !ulong.TryParse(roleId, out var gRoleId) ||
                !SupportedDomains.Contains(domain) ||
                !SupportedVersions.Contains(version) ||
                !SupportedActions.Contains(action) ||
                !long.TryParse(ticks, out var jsTicks) ||
                !TryParseJavascriptTicks(jsTicks, out var signatureDate))
            {
                messageModel = null;
                return false;
            }

            messageModel = new VerificationMessageModel
            {
                Domain = domain,
                Version = version,
                Action = action,
                Path = path,
                GuildId = gGuildId,
                UserId = gUserId,
                RoleId = gRoleId,
                OriginalTicks = jsTicks,
                Timestamp = signatureDate
            };
            return true;
        }

        public static bool TryParsePath(string path, out (string creatorAddress, BigInteger tokenId) erc721Token)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                erc721Token = default;
                return false;
            }
            var parts = path.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                erc721Token = default;
                return false;
            }
            if (!BigInteger.TryParse(parts[1], out var tokenId))
            {
                erc721Token = default;
                return false;
            }

            erc721Token = (parts[0], tokenId);
            return true;
        }

        private static bool TryParseJavascriptTicks(long jsTicks, out DateTimeOffset dateTime)
        {
            try
            {
                dateTime = new DateTimeOffset(jsTicks * 10000 + 621355968000000000, TimeSpan.Zero);
                return true;
            }
            catch (Exception)
            {
                dateTime = DateTimeOffset.MinValue;
                return false;
            }
        }
    }

    public class VerificationMessageModel
    {
        public string Domain { get; set; }
        public string Version { get; set; }
        public string Action { get; set; }
        public string Path { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong RoleId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public long OriginalTicks { get; set; }
    }
}
