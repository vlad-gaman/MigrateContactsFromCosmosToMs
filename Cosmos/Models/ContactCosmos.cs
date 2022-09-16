namespace MigrateContactsFromCosmosToMs.Cosmos.Models
{
    public class ContactCosmos : CosmosDocument
    {
        [JsonProperty("address")]
        public AddressCosmos Address { get; set; }

        [JsonProperty("metadata")]
        public ContactMetadataCosmos ContactMetadata { get; set; }

        /*
         * Storing the properties as array allows us to join the document on the properties and filter without
         * needing to know the key
         */
        [JsonProperty("properties")]
        public List<ContactPropertyCosmos> Properties { get; set; }

        /*
         * Storing the properties as an object allows us to perform contains logic on the value to perform
         * fuzzy search on values whilst being specific about the key
         */
        [JsonProperty("normalisedProperties")]
        public Dictionary<string, string> NormalisedProperties { get; set; }

        [JsonProperty("groupsInformation")]
        public IEnumerable<GroupInfoCosmos> GroupsInformation { get; set; }
    }
}
