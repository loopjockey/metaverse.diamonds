using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Metaverse.Functions.Common
{
    public static class HttpQueryStringNames 
    {
        public const string Address = "address";
        public const string Guild = "guild";
    }

    public static class HttpExtensions
    {
        public static (string scheme, string parameter) GetAuthorizationHeader(this IHeaderDictionary headers)
        {
            var auth = AuthenticationHeaderValue.Parse(headers[HeaderNames.Authorization]);
            return (auth.Scheme, auth.Parameter);
        }

        public static async Task<TType> ReadBodyAsync<TType>(this HttpRequest request) 
        {
            using (StreamReader streamReader = new StreamReader(request.Body))
            {
                var requestBody = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<TType>(requestBody);
            }
        }

        public static ulong GetGuildId(this IQueryCollection query) 
        { 
            return ulong.Parse(query[HttpQueryStringNames.Guild].First());
        }

        public static string GetAddress(this IQueryCollection query)
        {
            return query[HttpQueryStringNames.Address].First();
        }
    }
}
