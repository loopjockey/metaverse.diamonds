using Nethereum.Signer;

namespace Metaverse.Bot.Ethereum
{
    public static class SignatureValidator
    {
        public static string GetAddressOfSignature(string ethereumSignature, string sourceMessage)
        {
            var messageSigner = new EthereumMessageSigner();
            var signatureAddress = messageSigner.EncodeUTF8AndEcRecover(sourceMessage, ethereumSignature);
            return signatureAddress;
        }
    }
}
