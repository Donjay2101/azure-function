using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Repositories.Interfaces
{
    public interface IBaseRepository<T> where T : class, new()
    {
        IQueryable<T> GetAll(FeedOptions feedOptions);
        IQueryable<T> GetAll(Expression<Func<T, bool>> predicate, FeedOptions feedOptions);
        IQueryable<T> GetAll(string sqlExpression, FeedOptions feedOptions);
        Task<PagedListModel<T>> GetList(Expression<Func<T, bool>> predicate, string partitionKey, int maxItemCount = -1, string requestContinuation = null);
        Task<T> Get(string id, string partitionKey);
        Task<Document> GetDocument(string id, string partitionKey);
        Task<T> Get(Expression<Func<T, bool>> predicate, string partitionKey);
        Task<Document> Add(T item);
        Task<Document> Update(string id, T item, string modifyBy, string partitionKey);
        Task<Document> Update(string id, T item, string partitionKey);
        Task<Document> Delete(string id, string deleteBy, string partitionKey);
        Task UpdateThroughput(int throughput);
    }
}
