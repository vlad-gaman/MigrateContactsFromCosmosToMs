namespace MigrateContactsFromCosmosToMs.MsSql.Models
{
    public class GroupMs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [StringLength(128)]
        public string AccountId { get; set; }

        [StringLength(150)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(128)]
        public string User { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        [StringLength(128)]
        public string Product { get; set; }

        public Guid ETag { get; set; }

        public List<ContactXGroupMs> ContactXGroups { get; set; }
    }
}
