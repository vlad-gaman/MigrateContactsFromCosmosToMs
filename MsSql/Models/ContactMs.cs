namespace MigrateContactsFromCosmosToMs.MsSql.Models
{
    public class ContactMs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [StringLength(128)]
        public string AccountId { get; set; }

        [StringLength(32)]
        public string Msisdn { get; set; }

        [StringLength(128)]
        public string User { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        [StringLength(128)]
        public string Product { get; set; }

        [StringLength(8)]
        public string Version { get; set; }

        public Guid ETag { get; set; }

        public List<ContactXGroupMs> ContactXGroups { get; set; }
        public List<ContactPropertyMs> ContactProperties { get; set; }
    }
}
