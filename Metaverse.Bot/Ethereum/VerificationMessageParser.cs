using System;
using System.Linq;

namespace Metaverse.Bot.Ethereum
{
    public class VerificationMessageParser
    {
        private static readonly string[] SupportedDomains = new[] { "metaverse.diamonds" };
        private static readonly string[] SupportedVersions = new[] { "v1" };
        private static readonly string[] SupportedActions = new[] { "use_nft" };

        public bool TryParse(string verificationMessage, out VerificationMessageModel messageModel) 
        {
            var messageParts = verificationMessage.Split('|');
            var domain = messageParts[0];
            var version = messageParts[1];
            var action = messageParts[2];
            var path = messageParts[3];
            var ticks = messageParts[4];

            if (messageParts.Length != 5 ||
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
                Timestamp = signatureDate
            };
            return true;
        }

        public static bool TryParseJavascriptTicks(long jsTicks, out DateTimeOffset dateTime)
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
        public DateTimeOffset Timestamp { get; set; }
    }
}
