namespace MigrateContactsFromCosmosToMs.Cosmos.Models
{
    public class AddressCosmos
    {
        [JsonProperty("msisdn")]
        public string Msisdn { get; set; }
    }
}
