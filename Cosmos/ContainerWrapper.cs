namespace MigrateContactsFromCosmosToMs.Cosmos
{
    public class ContainerWrapper<T> : IContainerWrapper<T> where T : class
    {
        public Container Container { get; private set; }
        public ContainerWrapper(Container container)
        {
            Container = container;
        }

        public Task<ItemResponse<T>> ReadItemAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Container.ReadItemAsync<T>(id, partitionKey, requestOptions, cancellationToken);
        }

        public IOrderedQueryable<T> GetItemLinqQueryable(bool allowSynchronousQueryExecution = false, string continuationToken = null, QueryRequestOptions requestOptions = null, CosmosLinqSerializerOptions linqSerializerOptions = null)
        {
            return Container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution, continuationToken, requestOptions, linqSerializerOptions);
        }

        public FeedIterator<T> GetItemQueryIterator(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null)
        {
            return Container.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);
        }

        public FeedIterator<T1> GetItemQueryIterator<T1>(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null)
        {
            return Container.GetItemQueryIterator<T1>(queryDefinition, continuationToken, requestOptions);
        }

        public FeedIterator<T> GetItemQueryIterator(IQueryable<T> query, string continuationToken = null, QueryRequestOptions requestOptions = null)
        {
            return GetItemQueryIterator(query.ToQueryDefinition(), continuationToken, requestOptions);
        }

        public Task<ItemResponse<T>> CreateItemAsync(T item, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Container.CreateItemAsync(item, partitionKey, requestOptions, cancellationToken);
        }

        public Task<ItemResponse<T>> UpsertItemAsync(T item, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Container.UpsertItemAsync(item, partitionKey, requestOptions, cancellationToken);
        }

        public Task<ItemResponse<T>> DeleteItemAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Container.DeleteItemAsync<T>(id, partitionKey, requestOptions, cancellationToken);
        }
    }
}
