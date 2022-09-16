namespace MigrateContactsFromCosmosToMs.MsSql.Models
{
    public class ContactPropertyMs
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Contact")]
        public Guid ContactId { get; set; }

        [StringLength(256)]
        public string Key { get; set; }

        [StringLength(256)]
        public string Value { get; set; }

        public ContactMs Contact { get; set; }
    }
}
