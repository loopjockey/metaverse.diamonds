using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Nethereum.Util;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Metaverse.Functions.Common.Extensions
{
    public static class HttpExtensions
    {
        public static string GetAuthorizationHeader(this IHeaderDictionary headers)
        {
            try
            {
                var result = headers[HeaderNames.Authorization];
                if (result.Count == 0) return null;
                var auth = AuthenticationHeaderValue.Parse(result);
                return auth.Parameter;
            }
            catch
            {
                return null;
            }
        }

        public static string GenerateVerificationMessage(ulong discordUserId, long expiryTicks)
        {
            return $"I agree to link this user {discordUserId} to my current address. Expires: {expiryTicks}";
        }

        public static (string signature, long expiryTicks, DateTimeOffset expiry) GetSignatureHeader(this IHeaderDictionary headers)
        {
            var web3AuthHeader = headers["X-Web3-Auth"];
            if (web3AuthHeader.Count == 0) return default;
            var parts = web3AuthHeader[0].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return default;
            var signaturePart = parts[0];
            if (!long.TryParse(parts[1], out var expiryTicks)) return default;
            if (!TryParseJavascriptTicks(expiryTicks, out var expiryDate)) return default;
            return (signaturePart, expiryTicks, expiryDate);
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

        public static async Task<TType> ReadBodyAsync<TType>(this HttpRequest request)
        {
            using (StreamReader streamReader = new StreamReader(request.Body))
            {
                var requestBody = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<TType>(requestBody);
            }
        }
    }
}
