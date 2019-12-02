using System;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Petronas.Services.Social.Configurations;
using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Services.Interfaces;

namespace Petronas.Services.Social.Functions.Queues
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class HashTagQueueFunction
    {
        [FunctionName("HashTagQueueFunction")]
        public static async Task Run(
            [QueueTrigger(
                QueueNames.Comments,
                Connection = AppSettings.StorageConnectionString)]string queueMessage,
            ILogger log,
            [Inject]IHashTagService hashTagService)
        {
            if (queueMessage == null)
                throw new ArgumentNullException(nameof(queueMessage));

            var queueContract = JsonConvert.DeserializeObject<HashTagQueueContract>(queueMessage);
            HashTagContract hashTagContract =new HashTagContract();
            if(!string.IsNullOrEmpty(queueContract.Payloads))
            {
                hashTagContract = JsonConvert.DeserializeObject<HashTagContract>(queueContract.Payloads);
            }
            hashTagContract.ApplicationId = queueContract.ApplicationId;
            hashTagContract.ClientId = queueContract.ClientId;
            hashTagContract.ParentId = queueContract.ParentId;
            hashTagContract.Environment = queueContract.Environment;
            hashTagContract.UserId = queueContract.UserId;

            

            switch (queueContract.Action)
            {
                case QueueActions.Create:
                    var addResult = await hashTagService.Add(hashTagContract);
                    log.LogInformation($"New comment added: {JsonConvert.SerializeObject(addResult)}");
                    break;
                case QueueActions.Update:
                    var updateResult = await hashTagService.update(hashTagContract, queueContract.UserId);
                    log.LogInformation($"Comment updated: {JsonConvert.SerializeObject(updateResult)}");
                    break;
                case QueueActions.Delete:
                    var deleteResult = await hashTagService.Delete(hashTagContract);
                    log.LogInformation($"Comment deleted: {JsonConvert.SerializeObject(deleteResult)}");
                    break;
                default:
                    break;
            }

            log.LogInformation($"Comment Queue trigger function processed: {queueMessage}");
        }

     
    }
}
