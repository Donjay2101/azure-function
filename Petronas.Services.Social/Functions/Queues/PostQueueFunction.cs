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
    public static class PostQueueFunction
    {
        [FunctionName("PostQueueFunction")]
        public static async Task Run(
            [QueueTrigger(
                QueueNames.Posts,
                Connection = AppSettings.StorageConnectionString)]string queueMessage,
            ILogger log,
            [Inject]IPostService postService)
        {
            PostContract postContract  = new PostContract ();
            if (queueMessage == null)
                throw new ArgumentNullException(nameof(queueMessage));

            var queueContract = JsonConvert.DeserializeObject<PostQueueContract>(queueMessage);
            if(!string.IsNullOrEmpty(queueContract.Payloads))
            {
                postContract = JsonConvert.DeserializeObject<PostContract>(queueContract.Payloads);
            }
            postContract.ApplicationId = queueContract.ApplicationId;
            postContract.ClientId = queueContract.ClientId;
            postContract.Environment = queueContract.Environment;

            switch (queueContract.Action)
            {
                case QueueActions.Create:
                    var addResult = await postService.AddPost(postContract, queueContract.UserId);
                    log.LogInformation($"New post added: {JsonConvert.SerializeObject(addResult)}");
                    break;
                case QueueActions.Update:
                    var updateResult = await postService.UpdatePost(queueContract.Id, postContract, queueContract.UserId);
                    log.LogInformation($"Post updated: {JsonConvert.SerializeObject(updateResult)}");
                    break;
                case QueueActions.UpdatePartial:
                    log.LogInformation($"Post to update(Partially): {JsonConvert.SerializeObject(postContract)}");
                    await postService.UpdatePartial(queueContract.Id, postContract, queueContract.UserId);
                    break;
                case QueueActions.Delete:
                    var deleteResult = await postService.DeletePost(queueContract.Id, queueContract.ApplicationId, queueContract.UserId, queueContract.Environment);
                    log.LogInformation($"Post deleted: {JsonConvert.SerializeObject(deleteResult)}");
                    break;
                default:
                    break;
            }
            log.LogInformation($"Post Queue trigger function processed: {queueMessage}");
        }
    }
}
