namespace MigrateContactsFromCosmosToMs.Cosmos.Models
{
    public class GroupCosmos : CosmosDocument
    {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("metadata")]
        public ContactMetadataCosmos ContactMetadata { get; set; }
    }
}
