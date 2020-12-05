using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorageTest.Interfaces;
using Microsoft.Azure.Cosmos.Table;

namespace AzureStorageTest
{
    public class UnitOfWork : IUnitOfWork
    {
        public string ConnectionString { get; set; }
        public Queue<Task<Action>> RollbackActions { get; set; }

        private bool complete;
        private bool disposed;
        private Dictionary<string, object> repositories;

        public UnitOfWork(string connectionString)
        {
            ConnectionString = connectionString;
            RollbackActions = new Queue<Task<Action>>();
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }

        public void CommitTransaction()
        {
            complete = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IRepository<T> Repository<T>() where T : TableEntity
        {
            if (repositories == null)
                repositories = new Dictionary<string, object>();

            var type = typeof(T).Name;

            if (repositories.ContainsKey(type)) return (IRepository<T>)repositories[type];

            var repositoryType = typeof(Repository<>);

            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), this);

            repositories.Add(type, repositoryInstance);

            return (IRepository<T>)repositories[type];
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (!complete) RollbackTransaction();
                }
                finally
                {
                    RollbackActions.Clear();
                }
            }
            complete = false;
        }

        private void RollbackTransaction()
        {
            while (RollbackActions.Count > 0)
            {
                var undoAction = RollbackActions.Dequeue();
                undoAction.Result();
            }
        }
    }
}
