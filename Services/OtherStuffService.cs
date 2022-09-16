namespace MigrateContactsFromCosmosToMs.Services
{
    public class OtherStuffService
    {
        private readonly IContainerWrapper<ContactCosmos> _contactContainer;
        private readonly IContainerWrapper<GroupCosmos> _groupContainer;
        private readonly ContactContext _contactContext;
        private readonly ILogger<OtherStuffService> _logger;
        private readonly IConfiguration _configuration;

        public OtherStuffService(IContainerWrapper<ContactCosmos> contactContainer, IContainerWrapper<GroupCosmos> groupContainer, ContactContext contactContext, ILogger<OtherStuffService> logger, IConfiguration configuration)
        {
            _contactContainer = contactContainer;
            _groupContainer = groupContainer;
            _contactContext = contactContext;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task DeleteDuplicateGroups()
        {
            _logger.LogInformation("Started deleting duplicate groups by groupName and accountId");
            CreateQueryRequestAndListOfTasks(out var queryRequestOptions, out var tasks);
            string customToken = null;
            FeedIterator<CustomGroupWithAccount> customFeedIterator;
            do
            {
                customFeedIterator = _groupContainer.GetItemQueryIterator<CustomGroupWithAccount>(
                    new QueryDefinition($"select * from (SELECT c.groupName, c.accountId, count(1) as cnt FROM c group by c.groupName, c.accountId) as b where b.cnt > 1")
                    , customToken, queryRequestOptions);
                var customFeedResponse = await customFeedIterator.ReadNextAsync();
                customToken = customFeedResponse.ContinuationToken;

                foreach (var customGroupWithAccount in customFeedResponse.Resource)
                {
                    await DeleteGroups($"select * from c where c.accountId = '{customGroupWithAccount.AccountId}' and c.groupName = '{customGroupWithAccount.GroupName}'", queryRequestOptions, tasks);
                }
            } while (customFeedIterator.HasMoreResults);
        }

        public Task DeleteContactsWithMsisdnLongerThen32()
        {
            _logger.LogInformation("Started deleting contacts with length of msisdn longer than 32");
            return DeleteContacts("select * from c where length(c.address.msisdn) > 32");
        }

        public Task DeleteContactsWithVersionLike()
        {
            _logger.LogInformation("Started deleting contacts with version like 'Version%'");
            return DeleteContacts("SELECT * FROM c where c.version like 'Version%'");
        }

        public Task DeleteGroupsWithGroupNameLike()
        {
            _logger.LogInformation("Started deleting groups with groupName like 'GroupName%'");
            CreateQueryRequestAndListOfTasks(out var queryRequestOptions, out var tasks);
            return DeleteGroups("SELECT * FROM c where c.groupName like 'GroupName%'", queryRequestOptions, tasks);
        }

        private Task DeleteContacts(string select)
        {
            return Delete(_contactContainer, select, contact => (contact.Id, contact.AccountId));
        }

        private void CreateQueryRequestAndListOfTasks(out QueryRequestOptions queryRequestOptions, out List<Task> tasks)
        {
            var batchSize = _configuration.GetRequiredSection("Settings").GetValue<int>("BatchSize");
            queryRequestOptions = new QueryRequestOptions()
            {
                MaxItemCount = batchSize
            };
            tasks = new List<Task>(batchSize);
        }

        private Task DeleteGroups(string select, QueryRequestOptions queryRequestOptions, List<Task> tasks)
        {
            return Delete(_groupContainer, select, group => (group.Id, group.AccountId), queryRequestOptions, tasks);
        }

        private Task Delete<T>(IContainerWrapper<T> container, string select, Func<T, (string id, string partitionKey)> getIdAndPartitionKey) where T : class
        {
            CreateQueryRequestAndListOfTasks(out var queryRequestOptions, out var tasks);
            return Delete(container, select, getIdAndPartitionKey, queryRequestOptions, tasks);
        }

        private async Task Delete<T>(IContainerWrapper<T> container, string select, Func<T, (string id, string partitionKey)> getIdAndPartitionKey, QueryRequestOptions queryRequestOptions, List<Task> tasks) where T : class
        {
            _logger.LogInformation("Started deleting in batches of {batchSize} ...", queryRequestOptions.MaxItemCount);
            string token = null;
            FeedIterator<T> feedIterator;
            do
            {
                feedIterator = container.GetItemQueryIterator(new QueryDefinition(select), token, queryRequestOptions);
                var feedResponse = await feedIterator.ReadNextAsync();
                token = feedResponse.ContinuationToken;

                foreach (var item in feedResponse.Resource)
                {
                    var tuple = getIdAndPartitionKey(item);
                    tasks.Add(container.DeleteItemAsync(tuple.id, new PartitionKey(tuple.partitionKey)));
                }
                _logger.LogInformation("Deleting {count} items", feedResponse.Count);
                await Task.WhenAll(tasks);
                tasks.Clear();
            } while (feedIterator.HasMoreResults);
        }
    }

    public class CustomGroupWithAccount
    {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }
    }
}
