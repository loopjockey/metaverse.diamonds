using Microsoft.Extensions.Configuration;

namespace Metaverse.Functions.Common.Configuration
{
    public class StorageConnectionStringSetting
    {
        public StorageConnectionStringSetting(IConfiguration configuration)
        {
            Value = configuration.GetValue<string>("AzureWebJobsStorage");
        }

        public string Value { get; set; }

        public static implicit operator string(StorageConnectionStringSetting x)
        {
            return x.Value;
        }
    }
}
