namespace MigrateContactsFromCosmosToMs.Services
{
    public class MigrateDataBackgroundService
    {
        private readonly IContainerWrapper<ContactCosmos> _contactContainer;
        private readonly IContainerWrapper<GroupCosmos> _groupContainer;
        private readonly ContactContext _contactContext;
        private readonly ILogger<MigrateDataBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public MigrateDataBackgroundService(IContainerWrapper<ContactCosmos> contactContainer, IContainerWrapper<GroupCosmos> groupContainer, ContactContext contactContext, ILogger<MigrateDataBackgroundService> logger, IConfiguration configuration)
        {
            _contactContainer = contactContainer;
            _groupContainer = groupContainer;
            _contactContext = contactContext;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task ExecuteAsync()
        {
            var batchSize = _configuration.GetSection("Settings").GetValue<int>("BatchSize");
            _logger.LogInformation("Started migration from Cosmos to MsSql in batches of {batchSize}...", batchSize);
            var queryRequestOptions = new QueryRequestOptions()
            {
                MaxItemCount = batchSize
            };
            var queryDefinition = new QueryDefinition("select * from c");

            await MigrateGroups(queryRequestOptions, queryDefinition);
            await MigrateContacts(queryRequestOptions, queryDefinition);
        }

        private async Task MigrateGroups(QueryRequestOptions queryRequestOptions, QueryDefinition queryDefinition)
        {
            _logger.LogInformation("Migrating groups...");
            string token = null;
            FeedIterator<GroupCosmos> groupFeedIterator;
            do
            {
                _logger.LogInformation("Retrieving next batch of groups...");
                groupFeedIterator = _groupContainer.GetItemQueryIterator(queryDefinition, token, queryRequestOptions);
                var groupFeedResponse = await groupFeedIterator.ReadNextAsync();
                var groups = groupFeedResponse.Resource?.Select(MapGroupFromCosmosToMsSql).ToList();

                _logger.LogInformation("Inserting {count} groups...", groups?.Count ?? 0);
                if (groups != null)
                {
                    _contactContext.BulkInsertOrUpdate(groups, new BulkConfig()
                    {
                        OnConflictUpdateWhereSql = (existing, inserted) =>
                        {
                            return $"{inserted}.{nameof(GroupMs.LastUpdatedAt)} > {existing}.{nameof(GroupMs.LastUpdatedAt)}";
                        }
                    }, n => _logger.LogInformation("Groups progress {n}...", n));
                }

                token = groupFeedResponse.ContinuationToken;
            } while (groupFeedIterator.HasMoreResults);
        }

        private async Task MigrateContacts(QueryRequestOptions queryRequestOptions, QueryDefinition queryDefinition)
        {
            string token = null;
            FeedIterator<ContactCosmos> contactFeedIterator;
            _logger.LogInformation("Migrating Contacts...");
            do
            {
                _logger.LogInformation("Retrieving next batch of contacts...");
                contactFeedIterator = _contactContainer.GetItemQueryIterator(queryDefinition, token, queryRequestOptions);
                var contactFeedResponse = await MigrateContactsRecords(contactFeedIterator);
                MigrateGroupContactRelation(contactFeedResponse);
                MigrateContactProperties(contactFeedResponse);

                token = contactFeedResponse.ContinuationToken;
            } while (contactFeedIterator.HasMoreResults);
        }

        private async Task<FeedResponse<ContactCosmos>> MigrateContactsRecords(FeedIterator<ContactCosmos> contactFeedIterator)
        {
            var contactFeedResponse = await contactFeedIterator.ReadNextAsync();
            var contacts = contactFeedResponse.Resource?.Select(MapContactFromCosmosToMsSql).ToList();
            _logger.LogInformation("Inserting {count} contacts...", contacts?.Count ?? 0);
            if (contacts != null)
            {
                _contactContext.BulkInsertOrUpdate(contacts, new BulkConfig()
                {
                    OnConflictUpdateWhereSql = (existing, inserted) => $"{inserted}.{nameof(ContactMs.LastUpdatedAt)} > {existing}.{nameof(ContactMs.LastUpdatedAt)}"
                }, n => _logger.LogInformation("Contacts progress {n}...", n));
            }

            return contactFeedResponse;
        }

        private void MigrateGroupContactRelation(FeedResponse<ContactCosmos> contactFeedResponse)
        {
            var groupContactRelations = contactFeedResponse.Resource?.SelectMany(contact =>
            {
                return contact.GroupsInformation?.Select(MapContactGroupRelationFromCosmosToMsSql(contact)) ?? new List<ContactXGroupMs>();
            }).ToList();
            _logger.LogInformation("Inserting {count} group contact relations...", groupContactRelations?.Count ?? 0);
            if (groupContactRelations != null)
            {
                _contactContext.BulkInsertOrUpdate(groupContactRelations, new BulkConfig()
                {
                    UpdateByProperties = new List<string>() { "ContactId", "GroupId" },
                    OnConflictUpdateWhereSql = (_, _) => $"1 <> 1"
                }, n => _logger.LogInformation("Group contact relation progress {n}...", n));
            }
        }

        private void MigrateContactProperties(FeedResponse<ContactCosmos> contactFeedResponse)
        {
            var contactProperties = contactFeedResponse.Resource?.SelectMany(contact
                => contact.NormalisedProperties.Select(MapContactPropertiesFromCosmosToMsSql(contact))
            ).ToList();
            _logger.LogInformation("Inserting {count} contact properties...", contactProperties?.Count ?? 0);
            if (contactProperties != null)
            {
                _contactContext.BulkInsertOrUpdate(contactProperties, new BulkConfig()
                {
                    UpdateByProperties = new List<string>() { "ContactId", "Key" }
                }, n => _logger.LogInformation("Contact property progress {n}...", n));
            }
        }

        private static Func<KeyValuePair<string, string>, ContactPropertyMs> MapContactPropertiesFromCosmosToMsSql(ContactCosmos contact)
        {
            return keyValue => new ContactPropertyMs()
            {
                Id = Guid.NewGuid(),
                ContactId = Guid.Parse(contact.Id),
                Key = keyValue.Key,
                Value = keyValue.Value ?? string.Empty
            };
        }

        private static Func<GroupInfoCosmos, ContactXGroupMs> MapContactGroupRelationFromCosmosToMsSql(ContactCosmos contact)
        {
            return roupInfo => new ContactXGroupMs()
            {
                Id = Guid.NewGuid(),
                ContactId = Guid.Parse(contact.Id),
                GroupId = Guid.Parse(roupInfo.GroupId)
            };
        }

        private static ContactMs MapContactFromCosmosToMsSql(ContactCosmos contact)
        {
            return new ContactMs()
            {
                AccountId = contact.AccountId,
                ETag = Guid.Parse(contact.ETag.Replace("\\", "").Replace("\"", "")),
                Id = Guid.Parse(contact.Id),
                LastUpdatedAt = DateTime.Parse(contact.ContactMetadata.LastUpdatedAt),
                Msisdn = contact.Address.Msisdn,
                Product = contact.ContactMetadata.Product,
                User = contact.ContactMetadata.User,
                Version = contact.Version
            };
        }

        private static GroupMs MapGroupFromCosmosToMsSql(GroupCosmos group)
        {
            return new GroupMs()
            {
                Id = Guid.Parse(group.Id),
                AccountId = group.AccountId,
                ETag = Guid.Parse(group.ETag.Replace("\\", "").Replace("\"", "")),
                Name = group.GroupName,
                LastUpdatedAt = DateTime.Parse(group.ContactMetadata.LastUpdatedAt),
                Product = group.ContactMetadata.Product,
                User = group.ContactMetadata.User,
                Description = string.Empty
            };
        }
    }
}
