namespace MigrateContactsFromCosmosToMs.MsSql
{
    public class ContactContext : DbContext
    {
        public DbSet<ContactMs> Contacts { get; set; }
        public DbSet<GroupMs> Groups { get; set; }
        public DbSet<ContactXGroupMs> ContactXGroups { get; set; }
        public DbSet<ContactPropertyMs> ContactProperties { get; set; }

        public ContactContext(DbContextOptions<ContactContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ContactMs>()
                .HasKey(e => e.Id)
                .IsClustered(false);

            builder.Entity<ContactMs>()
                .HasIndex(nameof(ContactMs.Id), nameof(ContactMs.AccountId))
                .IsUnique();

            builder.Entity<ContactMs>()
                .HasIndex(nameof(ContactMs.AccountId), nameof(ContactMs.Msisdn))
                .IsUnique();

            builder.Entity<GroupMs>()
                .HasKey(e => e.Id)
                .IsClustered(false);

            builder.Entity<GroupMs>()
                .HasIndex(nameof(GroupMs.AccountId), nameof(GroupMs.Name))
                .IsUnique();

            builder.Entity<ContactXGroupMs>()
                .HasIndex(nameof(ContactXGroupMs.ContactId), nameof(ContactXGroupMs.GroupId))
                .IsUnique();

            builder.Entity<ContactPropertyMs>()
                .HasIndex(nameof(ContactPropertyMs.ContactId), nameof(ContactPropertyMs.Key))
                .IsUnique();

        }
    }
}
