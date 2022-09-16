namespace MigrateContactsFromCosmosToMs.Services
{
    public class GenerateDataService
    {
        private readonly IContainerWrapper<ContactCosmos> _contactContainer;
        private readonly IContainerWrapper<GroupCosmos> _groupContainer;
        private readonly Fixture _fixture;
        private readonly ILogger<GenerateDataService> _logger;
        private readonly IConfiguration _configuration;

        public GenerateDataService(IContainerWrapper<ContactCosmos> contactContainer, IContainerWrapper<GroupCosmos> groupContainer, Fixture fixture, ILogger<GenerateDataService> logger, IConfiguration configuration)
        {
            _contactContainer = contactContainer;
            _groupContainer = groupContainer;
            _fixture = fixture;
            _logger = logger;
            _configuration = configuration;
        }

        public Task ExecuteAsync()
        {
            var section = _configuration.GetRequiredSection("GenerateData");
            int numberOfContacts = section.GetValue<int>("NumberOfContacts");
            int numberOfGroups = section.GetValue<int>("NumberOfGroups");

            _logger.LogInformation("Started generating {numberOfGroups} groups...", numberOfGroups);
            var contactMetadata = _fixture.Build<ContactMetadataCosmos>()
                .With(p => p.LastUpdatedAt, () => DateTime.UtcNow.ToString());

            var groups = _fixture.Build<GroupCosmos>()
                .With(p => p.AccountId, () => Guid.NewGuid().ToString())
                .With(p => p.Id, () => Guid.NewGuid().ToString())
                .With(p => p.ContactMetadata, () => contactMetadata.Create())
                .CreateMany(numberOfGroups);
            string accountId = string.Empty;

            var address = _fixture.Build<AddressCosmos>()
                .With(p => p.Msisdn, () => _fixture.Create<string>().Substring(0, 31));

            _logger.LogInformation("Started generating {numberOfContacts} contacts...", numberOfContacts);
            var contacts = _fixture.Build<ContactCosmos>()
                .With(p => p.Id, () => Guid.NewGuid().ToString())
                .With(p => p.AccountId, () =>
                {
                    accountId = groups.OrderBy(x => new Random().NextDouble()).First().AccountId;
                    return accountId;
                })
                .With(p => p.Address, () => address.Create())
                .With(p => p.ContactMetadata, () => contactMetadata.Create())
                .With(p => p.GroupsInformation,
                    () => groups.Where(x => x.AccountId == accountId)
                            .OrderBy(x => new Random().NextDouble())
                            .Take(PickANumber())
                            .Select(x => new GroupInfoCosmos() { GroupId = x.Id }))
                .With(p => p.Version, "v99")
                .CreateMany(numberOfContacts);

            return InsertData(groups, contacts);
        }

        private async Task InsertData(IEnumerable<GroupCosmos> groups, IEnumerable<ContactCosmos> contacts)
        {
            var batchSize = _configuration.GetRequiredSection("Settings").GetValue<int>("BatchSize");
            var tasks = new List<Task>(batchSize);
            _logger.LogInformation("Started inserting contacts into CosmosDb in batches of {batchSize}...", batchSize);
            foreach (var grouping in contacts.Select((contact, index) => new { contact = contact, index = index })
                                        .GroupBy(anonym => anonym.index / batchSize, anonym => anonym.contact))
            {
                foreach (var contact in grouping)
                {
                    tasks.Add(_contactContainer.CreateItemAsync(contact));
                }
                _logger.LogInformation("Batch number {batchNumber}", grouping.Key);
                await Task.WhenAll(tasks);
                tasks.Clear();
            }

            _logger.LogInformation("Started inserting groups into CosmosDb in batches of {batchSize}...", batchSize);
            foreach (var grouping in groups.Select((group, index) => new { group = group, index = index })
                                        .GroupBy(anonym => anonym.index / batchSize, anonym => anonym.group))
            {
                foreach (var group in grouping)
                {
                    tasks.Add(_groupContainer.CreateItemAsync(group));
                }
                _logger.LogInformation("Batch number {batchNumber}", grouping.Key);
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }

        private static int PickANumber()
        {
            var r = new Random();
            switch (r.NextSingle())
            {
                case float n when n < 0.5f:
                    return 0;
                case float n when n < 0.8f:
                    return r.Next(10);
                default:
                    return 1000;
            }
        }
    }
}
