using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Petronas.Services.Social.Constants;

namespace Petronas.Services.Social.Migrations
{
    public static class MigrateCollection
    {
        public static void Run(DocumentClient client)
        {
            var dbName = Environment.GetEnvironmentVariable(AppSettings.DbName);
            var collections = typeof(CollectionNames).GetFields();

            foreach (var c in collections)
            {
                var collection = new DocumentCollection();
                collection.Id = c.GetValue(null).ToString();
                CreatePartitionKey(collection);
                client.CreateDocumentCollectionAsync(
                    UriFactory.CreateDatabaseUri(dbName),
                    collection
                ).GetAwaiter().GetResult();
            }
        }

        private static void CreatePartitionKey(DocumentCollection collection)
        {
            collection.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });
            collection.IndexingPolicy.IncludedPaths.Clear();
            collection.PartitionKey.Paths.Add(PartitionPaths.PartitionKey);
            var path = new IncludedPath();

            switch (collection.Id)
            {
                case CollectionNames.Applications:
                    break;
                case CollectionNames.Clients:
                case CollectionNames.ClientFeatures:
                    path.Path = PartitionPaths.ApplicationId + PartitionPaths.Value;
                    path.Indexes.Add(new RangeIndex(DataType.String) { Precision = -1 });
                    collection.IndexingPolicy.IncludedPaths.Add(path);
                    break;
                case CollectionNames.Posts:
                    path.Path = PartitionPaths.ClientId + PartitionPaths.Value;
                    path.Indexes.Add(new RangeIndex(DataType.String) { Precision = -1 });
                    collection.IndexingPolicy.IncludedPaths.Add(path);

                    path = new IncludedPath();
                    path.Path = PartitionPaths.ApplicationId + PartitionPaths.Value;
                    path.Indexes.Add(new RangeIndex(DataType.String) { Precision = -1 });
                    collection.IndexingPolicy.IncludedPaths.Add(path);

                    path = new IncludedPath();
                    path.Path = PartitionPaths.AuthorId + PartitionPaths.Value;
                    path.Indexes.Add(new RangeIndex(DataType.String) { Precision = -1 });
                    collection.IndexingPolicy.IncludedPaths.Add(path);

                    path = new IncludedPath();
                    path.Path = PartitionPaths.IsPublished + PartitionPaths.Value;
                    path.Indexes.Add(new RangeIndex(DataType.String) { Precision = -1 });
                    collection.IndexingPolicy.IncludedPaths.Add(path);
                    break;
                case CollectionNames.Likes:
                    path.Path = PartitionPaths.TypeId + PartitionPaths.Value;
                    path.Indexes.Add(new RangeIndex(DataType.String) { Precision = -1 });
                    collection.IndexingPolicy.IncludedPaths.Add(path);

                    path = new IncludedPath();
                    path.Path = PartitionPaths.ResourceId + PartitionPaths.Value;
                    path.Indexes.Add(new RangeIndex(DataType.String) { Precision = -1 });
                    collection.IndexingPolicy.IncludedPaths.Add(path);
                    break;
                case CollectionNames.Comments:
                    path.Path = PartitionPaths.ParentId + PartitionPaths.Value;
                    path.Indexes.Add(new RangeIndex(DataType.String) { Precision = -1 });
                    collection.IndexingPolicy.IncludedPaths.Add(path);

                    path = new IncludedPath();
                    path.Path = PartitionPaths.ResourceId + PartitionPaths.Value;
                    path.Indexes.Add(new RangeIndex(DataType.String) { Precision = -1 });
                    collection.IndexingPolicy.IncludedPaths.Add(path);
                    break;
                case CollectionNames.UserLogs:
                    path.Path = PartitionPaths.ClientId + PartitionPaths.Value;
                    path.Indexes.Add(new RangeIndex(DataType.String) { Precision = -1 });
                    collection.IndexingPolicy.IncludedPaths.Add(path);
                    break;
                default:
                    break;
            }

            collection.IndexingPolicy.ExcludedPaths.Clear();
            collection.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = PartitionPaths.All });
        }
    }
}
