﻿using Core.Asset;
using Newtonsoft.Json;

namespace Providers.Contracts.Asset
{
    public class AssetContract
    {
        [JsonProperty("asset_ids")]
        public string[] AssetIds { get; set; }

        [JsonProperty("contract_url")]
        public string ContactUrl { get; set; }

        [JsonProperty("name_short")]
        public string NameShort { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("description_mime")]
        public string DescriptionMime { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("divisibility")]
        public int Divisibility { get; set; }

        [JsonProperty("link_to_website")]
        public bool LinkToWebsite { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        public string AssetDefinitionUrl { get; set; }
    }
}
