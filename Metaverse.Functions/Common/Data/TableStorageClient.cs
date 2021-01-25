using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using System.Threading.Tasks;

namespace Metaverse.Functions.Data
{
    public interface ITableStorageClient
    {
        Task DeleteAsync(string tableName, string partitionKey, string rowKey);
        TEntity GetExact<TEntity>(string tableName, string partitionKey, string rowKey) where TEntity : TableEntity, new();
        TEntity GetFirstOrDefault<TEntity>(string tableName, string partitionKey) where TEntity : TableEntity, new();
        TEntity[] GetFirstPageOfEntities<TEntity>(string tableName, string partitionKey) where TEntity : TableEntity, new();
        Task InsertOrReplaceAsync(string tableName, ITableEntity tableEntity);
    }

    public class TableStorageClient : ITableStorageClient
    {
        private readonly CloudTableClient _cloudTableClient;

        public TableStorageClient(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            _cloudTableClient = storageAccount.CreateCloudTableClient();
        }

        public TEntity GetExact<TEntity>(string tableName, string partitionKey, string rowKey) where TEntity : TableEntity, new()
        {
            var table = _cloudTableClient.GetTableReference(tableName);
            try
            {
                var query = new TableQuery<TEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey)))
                    .Take(1);
                return table.ExecuteQuery(query).FirstOrDefault();
            }
            catch (StorageException)
            {
                return null;
            }
        }

        public TEntity GetFirstOrDefault<TEntity>(string tableName, string partitionKey) where TEntity : TableEntity, new()
        {
            var table = _cloudTableClient.GetTableReference(tableName);
            try
            {
                var query = new TableQuery<TEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey))
                    .Take(1);
                return table.ExecuteQuery(query).FirstOrDefault();
            }
            catch (StorageException)
            {
                return null;
            }
        }

        public TEntity[] GetFirstPageOfEntities<TEntity>(string tableName, string partitionKey) where TEntity : TableEntity, new()
        {
            var table = _cloudTableClient.GetTableReference(tableName);
            try
            {
                var query = new TableQuery<TEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
                return table.ExecuteQuery(query).ToArray();
            }
            catch (StorageException)
            {
                return new TEntity[] { };
            }
        }

        public async Task DeleteAsync(string tableName, string partitionKey, string rowKey)
        {
            var table = _cloudTableClient.GetTableReference(tableName);
            try
            {
                await table.ExecuteAsync(TableOperation.Delete(new TableEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    ETag = "*"
                }));
            }
            catch (StorageException)
            {
                return;
            }
        }
        public async Task InsertOrReplaceAsync(string tableName, ITableEntity tableEntity)
        {
            var table = _cloudTableClient.GetTableReference(tableName);
            try
            {
                await table.ExecuteAsync(TableOperation.InsertOrReplace(tableEntity));
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 404)
                {
                    await table.CreateIfNotExistsAsync();
                    await table.ExecuteAsync(TableOperation.Insert(tableEntity));
                }
                else throw;
            }
        }
    }

    public interface IStorageClient
    {
        Task DeleteAsync(string tableName, string partitionKey, string rowKey);
        TEntity GetFirstOrDefault<TEntity>(string tableName, string partitionKey) where TEntity : TableEntity, new();
        TEntity[] GetFirstPageOfEntities<TEntity>(string tableName, string partitionKey) where TEntity : TableEntity, new();
        Task InsertOrReplaceAsync(string tableName, ITableEntity tableEntity);
    }
}
