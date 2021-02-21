using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metaverse.Functions.Common.OpenSea
{
    /// <summary>
    /// https://docs.opensea.io/reference#getting-assets
    /// </summary>
    public class OpenSeaApiClient
    {
        public async Task<GetAssetsResponse> GetTokensForAddress(GetAssetsRequest request) 
        {
            var restClient = new RestClient("https://api.opensea.io");
            var restRequest = new RestRequest("/api/v1/assets");

            if (request.owner != null)
            {
                restRequest.AddQueryParameter("owner", request.owner);
            }

            if (request.asset_contract_address != null)
            {
                restRequest.AddQueryParameter("asset_contract_address", request.asset_contract_address);
            }

            if (request.order_by != null)
            {
                restRequest.AddQueryParameter("order_by", request.order_by);
            }

            if (request.order_direction != null)
            {
                restRequest.AddQueryParameter("order_direction", request.order_direction);
            }

            if (request.offset != null)
            {
                restRequest.AddQueryParameter("offset", $"{request.offset}");
            }

            if (request.limit != null)
            {
                restRequest.AddQueryParameter("limit", $"{request.limit}");
            }

            if (request.collection != null)
            {
                restRequest.AddQueryParameter("collection", $"{request.collection}");
            }

            if (request.token_ids != null)
            {
                foreach (var id in request.token_ids)
                {
                    restRequest.AddQueryParameter("token_ids", id);
                }
            }

            if (request.asset_contract_addresses != null)
            {
                foreach (var id in request.asset_contract_addresses)
                {
                    restRequest.AddQueryParameter("asset_contract_addresses", id);
                }
            }

            return await restClient.GetAsync<GetAssetsResponse>(restRequest);
        }

        public class GetAssetsRequest {
            public string owner { get; set; } = null;
            public string[] token_ids { get; set; } = null;
            public string asset_contract_address { get; set; } = null;
            public string[] asset_contract_addresses { get; set; } = null;
            public string order_by { get; set; } = null;
            public string order_direction { get; set; } = null;
            public int? offset { get; set; } = null;
            public int? limit { get; set; } = null;
            public string collection { get; set; }
        }

        public class GetAssetsResponse { 
            public List<Asset> assets { get; set; }
        }

        public class Asset { 
            public AssetContract asset_contract { get; set; }
            public string token_id { get; set; }
        }

        public class AssetContract { 
            public string address { get; set; }
        }
    }
}
