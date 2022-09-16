namespace MigrateContactsFromCosmosToMs.MsSql.Models
{
    public class ContactXGroupMs
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Contact")]
        public Guid ContactId { get; set; }

        [ForeignKey("Group")]
        public Guid GroupId { get; set; }


        public ContactMs Contact { get; set; }
        public GroupMs Group { get; set; }
    }
}
