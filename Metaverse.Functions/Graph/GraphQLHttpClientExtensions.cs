using GraphQL;
using GraphQL.Client.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Metaverse.Functions.Graph
{
    public static class GraphQLHttpClientExtensions
    {
        public async static Task<(string address, BigInteger tokenId)[]> GetAddressTokensAsync(this IGraphQLClient graphQLClient, string ownerAddress, IEnumerable<string> creatorAddresses) 
        {
            var allResults = await Task.WhenAll(
                from creatorAddress in creatorAddresses
                select graphQLClient.GetAddressTokensAsync(ownerAddress, creatorAddress));
            return allResults.SelectMany(r => r).ToArray();
        }

        public async static Task<(string address, BigInteger tokenId)[]> GetAddressTokensAsync(this IGraphQLClient graphQLClient, string ownerAddress, string creatorAddress) 
        {
            var personAndFilmsRequest = new GraphQLRequest
            {
                Query = 
                @"query GetAddressTokens($ownerId: String, $creatorId: String) {
                    tokens(where:{owner:$ownerId, creator:$creatorId}) {
                        id,
                        tokenID,
                        tokenURI,
                        contract {
                            id
                        }
                    }
                }",
                OperationName = "GetAddressTokens",
                Variables = new
                {
                    ownerId = ownerAddress,
                    creatorId = creatorAddress
                }
            };

            var response = await graphQLClient.SendQueryAsync<GetAddressTokensResponse>(personAndFilmsRequest);
            return (from token in response.Data.tokens
                    let parts = token.id.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                    select (parts[0], BigInteger.Parse(parts[1]))).ToArray();
        }

        public async static Task<(string address, BigInteger tokenId)> GetAddressTokenAsync(this IGraphQLClient graphQLClient, string ownerAddress, string creatorAddress, BigInteger tokenId)
        {
            var personAndFilmsRequest = new GraphQLRequest
            {
                Query =
                @"query GetAddressTokens($ownerId: String, $creatorId: String, $tokenId: String) {
                    tokens(where:{owner:$ownerId, creator:$creatorId, tokenID:$tokenId}) {
                        id,
                        tokenID,
                        tokenURI,
                        contract {
                            id
                        }
                    }
                }",
                OperationName = "GetAddressTokens",
                Variables = new
                {
                    ownerId = ownerAddress,
                    creatorId = creatorAddress,
                    tokenId = tokenId.ToString()
                }
            };

            var response = await graphQLClient.SendQueryAsync<GetAddressTokensResponse>(personAndFilmsRequest);
            return (from token in response.Data.tokens
                    let parts = token.id.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                    select (parts[0], BigInteger.Parse(parts[1]))).FirstOrDefault();
        }

        public class GetAddressTokensResponse 
        { 
            public Token[] tokens { get; set; }  
        }

        public class Token
        { 
            public string id { get; set; }
            public string tokenID { get; set; }
            public string tokenURI { get; set; }
            public string mintTime { get; set; }
            public string owner { get; set; }
            public Contract contract { get; set; }
        }

        public class Contract
        {
            public string id { get; set; }
        }
    }
}
