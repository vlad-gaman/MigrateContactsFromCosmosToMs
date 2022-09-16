namespace MigrateContactsFromCosmosToMs.Cosmos.Models
{
    public class CosmosDocument
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("_etag")]
        public string ETag { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }
    }
}
