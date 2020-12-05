using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace AzureStorageTest.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        string ConnectionString { get; set; }
        Queue<Task<Action>> RollbackActions { get; set; }

        void CommitTransaction();
        IRepository<T> Repository<T>() where T : TableEntity;
    }
}
