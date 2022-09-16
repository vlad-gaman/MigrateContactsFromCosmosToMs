namespace MigrateContactsFromCosmosToMs
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddCosmosDatabase(this IServiceCollection services)
        {
            return services.AddSingleton(sp => new CosmosClientBuilder(sp.GetRequiredService<IConfiguration>().GetConnectionString("CosmosDb"))
                                                .WithApplicationName("Contacts")
                                                .WithApplicationRegion("UK South")
                                                .WithConnectionModeDirect()
                                                .WithThrottlingRetryOptions(TimeSpan.FromSeconds(10), 30)
                                                .Build())
                            .AddSingleton(s =>
                            {
                                var client = s.GetRequiredService<CosmosClient>();

                                var database = client.CreateDatabaseIfNotExistsAsync("Contacts", ThroughputProperties.CreateAutoscaleThroughput(4000))
                                                    .GetAwaiter().GetResult().Database;
                                return database;
                            });
        }

        public static IServiceCollection AddContactContainer(this IServiceCollection services)
        {
            return services.AddSingleton<IContainerWrapper<ContactCosmos>>(s =>
            {
                var database = s.GetRequiredService<Database>();

                var container = database.DefineContainer("Contacts", "/accountId")
                        .WithUniqueKey().Path("/address/msisdn")
                        .Attach()
                        .WithIndexingPolicy().WithIncludedPaths().Path("/groupsInformation/[]/groupId/?")
                        .Path("/")
                        .Attach().Attach()
                        .CreateIfNotExistsAsync().GetAwaiter().GetResult().Container;

                return new ContainerWrapper<ContactCosmos>(container);
            });
        }

        public static IServiceCollection AddGroupContainer(this IServiceCollection services)
        {
            return services.AddSingleton<IContainerWrapper<GroupCosmos>>(s =>
            {
                var database = s.GetRequiredService<Database>();

                var container = database.DefineContainer("Groups", "/accountId")
                        .CreateIfNotExistsAsync().GetAwaiter().GetResult().Container;

                return new ContainerWrapper<GroupCosmos>(container);
            });
        }

        public static IServiceCollection AddMsSqlDatabase(this IServiceCollection services)
        {

            services.AddDbContext<ContactContext>((sp, options) =>
            {
                options.UseSqlServer(sp.GetRequiredService<IConfiguration>().GetConnectionString("MsSqlDb"));
            });
            return services;
        }
    }
}
