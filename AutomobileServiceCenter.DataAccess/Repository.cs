using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutomobileServiceCenter.DataAccess.Interfaces;
using AutomobileServiceCenter.Models.BaseTypes;
using AutomobileServiceCenter.Utilities;
using Microsoft.Azure.Cosmos.Table;

namespace AutomobileServiceCenter.DataAccess
{
    public class Repository<T> : IRepository<T> where T : TableEntity, new()
    {
        public IUnitOfWork Scope { get; set; }

        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudTable _table;
        private readonly CloudTableClient _tableClient;

        public Repository(IUnitOfWork scope)
        {
            _storageAccount = CloudStorageAccount.Parse(scope.ConnectionString);

            _tableClient = _storageAccount.CreateCloudTableClient();
            _table = _tableClient.GetTableReference(typeof(T).Name);
            Scope = scope;
        }

        public async Task<T> AddAsync(T entity)
        {
            var entityToInsert = entity as BaseEntity;
            entityToInsert.CreatedOn = DateTime.UtcNow;
            entityToInsert.UpdatedOn = DateTime.UtcNow;

            TableOperation insertOperation = TableOperation.Insert(entity);
            var result = await ExecuteAsync(insertOperation);
            return result.Result as T;
        }

        public async Task CreateTableAsync()
        {
            CloudTable table = _tableClient.GetTableReference(typeof(T).Name);
            await table.CreateIfNotExistsAsync();

            if (typeof(IAuditTracker).IsAssignableFrom(typeof(T)))
            {
                var auditTable = _tableClient.GetTableReference($"{typeof(T).Name}Audit");
                await auditTable.CreateIfNotExistsAsync();
            }
        }

        public async Task DeleteAsync(T entity)
        {
            var entityToDelete = entity as BaseEntity;
            entityToDelete.UpdatedOn = DateTime.UtcNow;
            entityToDelete.IsDeleted = true;

            TableOperation deleteOperation = TableOperation.Replace(entityToDelete);
            await ExecuteAsync(deleteOperation);
        }

        public async Task<IEnumerable<T>> FindAllAsync()
        {
            TableQuery<T> query = new TableQuery<T>();
            TableContinuationToken token = null;
            var result = await _table.ExecuteQuerySegmentedAsync(query, token);
            return result.Results as IEnumerable<T>;
        }

        public async Task<IEnumerable<T>> FindAllByPartitionKeyAsync(string partitionKey)
        {
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            TableContinuationToken token = null;
            var result = await _table.ExecuteQuerySegmentedAsync(query, token);
            return result.Results as IEnumerable<T>;
        }

        public async Task<T> FindAsync(string partitionKey, string rowKey)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await _table.ExecuteAsync(retrieveOperation);
            return result.Result as T;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            var entityToUpdate = entity as BaseEntity;
            entityToUpdate.UpdatedOn = DateTime.UtcNow;

            TableOperation updateOperation = TableOperation.Replace(entity);
            var result = await ExecuteAsync(updateOperation);
            return result.Result as T;
        }

        private async Task<Action> CreateRollbackAction(TableOperation operation, bool isAuditOperation = false)
        {
            if (operation.OperationType == TableOperationType.Retrieve) return null;

            var entity = operation.Entity;
            var cloudTable = !isAuditOperation ? _table : _tableClient.GetTableReference($"{typeof(T).Name}Audit");
            switch (operation.OperationType)
            {
                case TableOperationType.Insert:
                    return async () => await UndoInsertOperationAsync(cloudTable, entity);
                case TableOperationType.Delete:
                    return async () => await UndoDeleteOperation(cloudTable, entity);
                case TableOperationType.Replace:
                    var retrieveResult = await cloudTable.ExecuteAsync(TableOperation.Retrieve(entity.PartitionKey, entity.RowKey));
                    return async () => await UndoReplaceOperation(retrieveResult.Result as DynamicTableEntity, entity);
                default:
                    throw new InvalidOperationException("The storage operation cannot be identified.");
            }
        }

        private async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            var rollbackAction = CreateRollbackAction(operation);
            var result = await _table.ExecuteAsync(operation);
            Scope.RollbackActions.Enqueue(rollbackAction);

            // Audit Implementation
            if (operation.Entity is IAuditTracker)
            {
                // Make sure we do not use same RowKey and PartitionKey
                var auditEntity = ObjectExtensions.CopyObject<T>(operation.Entity);
                auditEntity.PartitionKey = $"{auditEntity.PartitionKey}-{auditEntity.RowKey}";
                auditEntity.RowKey = $"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff")}";

                var auditOperation = TableOperation.Insert(auditEntity);
                var auditRollbackAction = CreateRollbackAction(auditOperation, true);

                var auditTable = _tableClient.GetTableReference($"{typeof(T).Name}Audit");
                await auditTable.ExecuteAsync(auditOperation);

                Scope.RollbackActions.Enqueue(auditRollbackAction);
            }

            return result;
        }

        private async Task UndoDeleteOperation(CloudTable table, ITableEntity entity)
        {
            var entityToRestore = entity as BaseEntity;
            entityToRestore.IsDeleted = false;

            var insertOperation = TableOperation.Replace(entity);
            await table.ExecuteAsync(insertOperation);
        }

        private async Task UndoInsertOperationAsync(CloudTable table, ITableEntity entity)
        {
            var deleteOperation = TableOperation.Delete(entity);
            await table.ExecuteAsync(deleteOperation);
        }

        private async Task UndoReplaceOperation(ITableEntity originalEntity, ITableEntity newEntity)
        {
            if (originalEntity is not null)
            {
                if (!String.IsNullOrEmpty(newEntity.ETag)) originalEntity.ETag = newEntity.ETag;

                var replaceOperation = TableOperation.Replace(originalEntity);
                await _table.ExecuteAsync(replaceOperation);
            }
        }
    }
}
