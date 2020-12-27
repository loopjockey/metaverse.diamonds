using Nethereum.StandardNonFungibleTokenERC721;
using Nethereum.StandardNonFungibleTokenERC721.ContractDefinition;
using Nethereum.Web3;
using System.Numerics;
using System.Threading.Tasks;

namespace Metaverse.Bot.Ethereum
{
    public class ERC721Client
    {
        public Web3 Web3 { get; set; }

        public async Task<string> GetAddressThatOwnsToken(string creatorAddress, BigInteger tokenId) 
        {
            var erc721 = new ERC721Service(Web3, creatorAddress);
            return await erc721.OwnerOfQueryAsync(new OwnerOfFunction
            {
                TokenId = tokenId,
            });
        } 
    }
}
