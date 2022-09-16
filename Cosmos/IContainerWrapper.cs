namespace MigrateContactsFromCosmosToMs.Cosmos
{
    public interface IContainerWrapper<T> where T : class
    {
        Container Container { get; }

        Task<ItemResponse<T>> ReadItemAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken));
        IOrderedQueryable<T> GetItemLinqQueryable(bool allowSynchronousQueryExecution = false, string continuationToken = null, QueryRequestOptions requestOptions = null, CosmosLinqSerializerOptions linqSerializerOptions = null);
        FeedIterator<T> GetItemQueryIterator(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null);
        FeedIterator<T> GetItemQueryIterator(IQueryable<T> query, string continuationToken = null, QueryRequestOptions requestOptions = null);
        Task<ItemResponse<T>> CreateItemAsync(T item, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<ItemResponse<T>> DeleteItemAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<ItemResponse<T>> UpsertItemAsync(T item, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken));
        FeedIterator<T1> GetItemQueryIterator<T1>(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null);
    }
}
