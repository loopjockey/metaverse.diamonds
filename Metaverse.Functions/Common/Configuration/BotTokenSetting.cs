namespace Metaverse.Functions.Common.Configuration
{
    public class BotTokenSetting
    {
        public BotTokenSetting(string value) 
        {
            Value = value;
        }

        public string Value { get; set; }

        public static implicit operator string(BotTokenSetting x)
        {
            return x.Value;
        }
    }
}
