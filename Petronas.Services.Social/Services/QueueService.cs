using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Contracts;

namespace Petronas.Services.Social.Services
{
    public class QueueService
    {
        private readonly string _queueName;
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudQueueClient queueClient;
        private readonly CloudQueue queue;

        public QueueService(string queueName)
        {
            _queueName = queueName;
            storageAccount = CloudStorageAccount.Parse(
                Environment.GetEnvironmentVariable(AppSettings.StorageConnectionString));
            queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        public async Task<CloudQueueMessage> AddMessage(string action, QueueMessageContract contract, string payloads)
        {
            contract.Action = action;
            contract.Payloads = payloads;
            var queueMessage = new CloudQueueMessage(contract.ToJsonString());
            await queue.AddMessageAsync(queueMessage);
            return queueMessage;
        }
    }
}
