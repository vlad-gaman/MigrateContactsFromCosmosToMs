namespace MigrateContactsFromCosmosToMs.Cosmos.Models
{
    public class GroupInfoCosmos
    {
        [JsonProperty("groupId")]
        public string GroupId { get; set; }
    }
}
