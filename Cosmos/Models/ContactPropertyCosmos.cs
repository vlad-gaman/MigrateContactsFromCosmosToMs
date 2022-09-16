namespace MigrateContactsFromCosmosToMs.Cosmos.Models
{
    public class ContactPropertyCosmos
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public ContactPropertyCosmos(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
