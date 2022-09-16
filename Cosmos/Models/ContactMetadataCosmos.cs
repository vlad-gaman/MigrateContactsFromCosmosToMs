namespace MigrateContactsFromCosmosToMs.Cosmos.Models
{
    public class ContactMetadataCosmos
    {
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonConverter(typeof(DateFormatConverter))]
        [JsonProperty("lastUpdatedAt")]
        public string LastUpdatedAt { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }
    }
}
