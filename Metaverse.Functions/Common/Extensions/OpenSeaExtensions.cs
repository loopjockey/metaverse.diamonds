using Metaverse.Functions.Common.OpenSea;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Metaverse.Functions.Common.Extensions
{
    public static class OpenSeaExtensions
    {
        public static async Task<(bool hasTokenInCollection, string erc721TokenRef)> DoesAddressOwnTokenInCollection(this OpenSeaApiClient openSeaApiClient, string ownerAddress, string collectionId)
        {
            var response = await openSeaApiClient.GetTokensForAddress(new OpenSeaApiClient.GetAssetsRequest
            {
                owner = ownerAddress,
                collection = collectionId,
                limit = 1
            });
            var applicableToken = response.assets.FirstOrDefault();
            if (applicableToken == default) return (false, null);
            return (true, $"{applicableToken.asset_contract}:{applicableToken.token_id}");
        }

        public static async Task<Dictionary<string, bool>> WhichCollectionsAreOwnedByAddress(this OpenSeaApiClient openSeaApiClient, string ownerAddress, HashSet<string> uniqueCollectionIds)
        {
            var results = await Task.WhenAll(
                from collectionId in uniqueCollectionIds
                select openSeaApiClient.DoesAddressOwnTokenInCollection(ownerAddress, collectionId));

            return results.Zip(uniqueCollectionIds).ToDictionary(a => a.Second, a => a.First.hasTokenInCollection);
        }
    }
}
