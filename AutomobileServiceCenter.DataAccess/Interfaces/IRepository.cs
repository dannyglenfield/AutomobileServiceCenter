using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace AutomobileServiceCenter.DataAccess.Interfaces
{
    public interface IRepository<T> where T : TableEntity
    {
        Task<T> AddAsync(T entity);
        Task CreateTableAsync();
        Task DeleteAsync(T entity);
        Task<IEnumerable<T>> FindAllAsync();
        Task<IEnumerable<T>> FindAllByPartitionKeyAsync(string partitionKey);
        Task<T> FindAsync(string partitionKey, string rowKey);
        Task<T> UpdateAsync(T entity);
    }
}
