using Microsoft.Extensions.Configuration;

namespace Metaverse.Functions.Common.Configuration
{
    public class BotTokenSetting
    {
        public BotTokenSetting(IConfiguration configuration) 
        {
            Value = configuration.GetValue<string>("BotToken");
        }

        public string Value { get; set; }

        public static implicit operator string(BotTokenSetting x)
        {
            return x.Value;
        }
    }
}
