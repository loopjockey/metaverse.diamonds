using Microsoft.Extensions.Configuration;

namespace Metaverse.Functions.Common.Configuration
{
    public class EIP721GraphUrlSetting
    {
        public EIP721GraphUrlSetting(IConfiguration configuration)
        {
            Value = configuration.GetValue<string>("EIP721GraphUrl");
        }

        public string Value { get; set; }

        public static implicit operator string(EIP721GraphUrlSetting x)
        {
            return x.Value;
        }
    }
}
