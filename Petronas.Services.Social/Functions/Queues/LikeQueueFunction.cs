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
    public static class LikeQueueFunction
    {
        [FunctionName("LikeQueueFunction")]
        public static async Task Run(
            [QueueTrigger(
                QueueNames.Likes,
                Connection = AppSettings.StorageConnectionString)]string queueMessage,
            ILogger log,
            [Inject]ILikeService likeService)
        {
            LikeContract likeContract =new LikeContract ();
            if (queueMessage == null)
                throw new ArgumentNullException(nameof(queueMessage));

            var queueContract = JsonConvert.DeserializeObject<LikeQueueContract>(queueMessage);

            if(!string.IsNullOrEmpty(queueContract.Payloads))
            {
                likeContract = JsonConvert.DeserializeObject<LikeContract>(queueContract.Payloads);
            }
            likeContract.ApplicationId = queueContract.ApplicationId;
            likeContract.ClientId = queueContract.ClientId;
            likeContract.PostId = queueContract.PostId;
            likeContract.Type = queueContract.Type;
            likeContract.TypeId = queueContract.TypeId;
            likeContract.Environment = queueContract.Environment;

            switch (queueContract.Action)
            {
                case QueueActions.Create:
                    var addResult = await likeService.Like(likeContract, queueContract.UserId);
                    log.LogInformation($"New like added: {JsonConvert.SerializeObject(addResult)}");
                    break;
                case QueueActions.Delete:
                    var deleteResult = await likeService.Unlike(likeContract, queueContract.UserId);
                    log.LogInformation($"Like deleted: {JsonConvert.SerializeObject(deleteResult)}");
                    break;
                default:
                    break;
            }

            log.LogInformation($"Like Queue trigger function processed: {queueMessage}");
        }
    }
}
