﻿using Microsoft.Azure.Cosmos.Table;

namespace Metaverse.Functions.Data
{
    public class GuildConfigurationEntity : TableEntity
    {
        public GuildConfigurationEntity() { }

        public GuildConfigurationEntity(ulong guildId, string key, string value) : base(new Partition(guildId).ToString(), key)
        {
            Value = value;
        }

        public string Value { get; set; }

        public const string TableName = "GuildConfigurations";

        public class KnownConfigurationKeys
        {
            public const string ShopUrl = "shop_url";
        }

        public class Partition
        {
            public Partition(ulong guildId)
            {
                GuildId = guildId;
            }

            public ulong GuildId { get; }

            public override string ToString()
            {
                return GuildId.ToString();
            }
        }
    }
}
