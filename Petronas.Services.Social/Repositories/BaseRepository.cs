using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Helpers;
using Petronas.Services.Social.Migrations;
using Petronas.Services.Social.Repositories.Interfaces;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class, new()
    {
        private readonly Lazy<DocumentClient> _lazyClient = new Lazy<DocumentClient>(InitializeDocumentClient);
        private DocumentClient _client => _lazyClient.Value;
        private Uri _collectionUri;
        private string _databaseName;
        private string _collectionName;

        public BaseRepository(string collectionName)
        {
            _databaseName = Environment.GetEnvironmentVariable(AppSettings.DbName);
            _collectionName = collectionName;
            _collectionUri = UriFactory.CreateDocumentCollectionUri(_databaseName, collectionName);
            var createDbResult = _client.CreateDatabaseIfNotExistsAsync(
                new Database { Id = _databaseName }).GetAwaiter().GetResult();

            // If database is newly created, run migrations
            if (createDbResult.StatusCode == HttpStatusCode.Created)
            {
                MigrateCollection.Run(_client);
                MigrateMasterData.Run(_client);
            }
        }

        public IQueryable<T> GetAll(FeedOptions feedOptions)
        {
            var result = _client.CreateDocumentQuery<T>(_collectionUri, feedOptions);
            return result;
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate, FeedOptions feedOptions)
        {
            var result = _client.CreateDocumentQuery<T>(_collectionUri, feedOptions).Where(predicate);
            return result;
        }

        public IQueryable<T> GetAll(string sqlExpression, FeedOptions feedOptions)
        {
            var result = _client.CreateDocumentQuery<T>(_collectionUri, sqlExpression, feedOptions);
            return result;
        }

        public async Task<PagedListModel<T>> GetList(Expression<Func<T, bool>> predicate, string partitionKey = null, int maxItemCount = -1, string requestContinuation = null)
        {
            var feedOptions = new FeedOptions
            {
                PartitionKey = GeneratePartitionKey(partitionKey),
                EnableCrossPartitionQuery = string.IsNullOrEmpty(partitionKey) ? true : false,
                EnableScanInQuery = true,
                MaxItemCount = maxItemCount,
                RequestContinuation = requestContinuation
            };
            var query = _client.CreateDocumentQuery<T>(_collectionUri, feedOptions)
                .Where(predicate).AsDocumentQuery();
            var queryResult = await query.ExecuteNextAsync<T>();

            var result = new PagedListModel<T>
            {
                ContinuationToken = GeneralHelper.EncodeString(queryResult.ResponseContinuation),
                TotalCount = queryResult.Count(),
                Data = queryResult.AsEnumerable()
            };

            return result;
        }

        public async Task<T> Get(string id, string partitionKey)
        {
            var docUri = UriFactory.CreateDocumentUri(_databaseName, _collectionName, id);
            var result = await _client.ReadDocumentAsync<T>(
                docUri,
                new RequestOptions { PartitionKey = GeneratePartitionKey(partitionKey) });
            return result;
        }

        public async Task<Document> GetDocument(string id, string partitionKey)
        {
            var docUri = UriFactory.CreateDocumentUri(_databaseName, _collectionName, id);
            var result = await _client.ReadDocumentAsync(
                docUri,
                new RequestOptions { PartitionKey = GeneratePartitionKey(partitionKey) });
            return result;
        }

        public async Task<T> Get(Expression<Func<T, bool>> predicate, string partitionKey)
        {
            var feedOptions = new FeedOptions
            {
                PartitionKey = GeneratePartitionKey(partitionKey),
                EnableCrossPartitionQuery = string.IsNullOrEmpty(partitionKey) ? true : false,
                EnableScanInQuery = true
            };

            var query = _client.CreateDocumentQuery<T>(_collectionUri, feedOptions)
                .Where(predicate).AsDocumentQuery();

            var result = await query.ExecuteNextAsync<T>();

            return result.AsEnumerable().FirstOrDefault();
        }

        public async Task<Document> Add(T item)
        {
            var result = await _client.CreateDocumentAsync(_collectionUri, item, null, false);
            return result.Resource;
        }

        public async Task<Document> Update(string id, T item, string modifyBy, string partitionKey)
        {
            var doc = await GetDocument(id, partitionKey);
            DocumentHelper.SetModificationProperties(doc, modifyBy, item);
            var docUri = UriFactory.CreateDocumentUri(_databaseName, _collectionName, id);
            var result = await _client.ReplaceDocumentAsync(
                docUri,
                doc,
                new RequestOptions { PartitionKey = GeneratePartitionKey(partitionKey) });
            return result.Resource;
        }

        public async Task<Document> Update(string id, T item, string partitionKey)
        {
            var docUri = UriFactory.CreateDocumentUri(_databaseName, _collectionName, id);
            var result = await _client.ReplaceDocumentAsync(
                docUri,
                item,
                new RequestOptions { PartitionKey = GeneratePartitionKey(partitionKey) });
            return result.Resource;
        }

        public async Task<Document> Delete(string id, string deleteBy, string partitionKey)
        {
            var doc = await GetDocument(id, partitionKey);
            DocumentHelper.SetDeletionProperties(doc, deleteBy);
            var docUri = UriFactory.CreateDocumentUri(_databaseName, _collectionName, id);
            var result = await _client.ReplaceDocumentAsync(
                docUri,
                doc,
                new RequestOptions {
                    PartitionKey = GeneratePartitionKey(partitionKey)
                });
            return result.Resource;
        }

        public async Task UpdateThroughput(int throughput)
        {
            var colection = await _client.ReadDocumentCollectionAsync(_collectionUri);
            Offer offer = _client.CreateOfferQuery()
                .Where(r => r.ResourceLink == colection.Resource.SelfLink)
                .AsEnumerable()
                .SingleOrDefault();

            offer = new OfferV2(offer, throughput);
            await _client.ReplaceOfferAsync(offer);
        }

        private static DocumentClient InitializeDocumentClient()
        {
            var uri = new Uri(Environment.GetEnvironmentVariable(AppSettings.DbUri));
            var authKey = Environment.GetEnvironmentVariable(AppSettings.DbAuthKey);
            var client = new DocumentClient(uri, authKey);
            client.OpenAsync().GetAwaiter().GetResult();
            return client;
        }

        private PartitionKey GeneratePartitionKey(string partitionKey)
        {
            return string.IsNullOrEmpty(partitionKey) ? null : new PartitionKey(partitionKey);
        }
    }
}
