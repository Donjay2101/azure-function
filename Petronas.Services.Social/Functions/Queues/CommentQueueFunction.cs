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
    public static class CommentQueueFunction
    {
        [FunctionName("CommentQueueFunction")]
        public static async Task Run(
            [QueueTrigger(
                QueueNames.Comments,
                Connection = AppSettings.StorageConnectionString)]string queueMessage,
            ILogger log,
            [Inject]ICommentService commentService)
        {
            if (queueMessage == null)
                throw new ArgumentNullException(nameof(queueMessage));

            var queueContract = JsonConvert.DeserializeObject<CommentQueueContract>(queueMessage);
            CommentContract commentContract =new CommentContract();
            if(!string.IsNullOrEmpty(queueContract.Payloads))
            {
                commentContract = JsonConvert.DeserializeObject<CommentContract>(queueContract.Payloads);
            }
            commentContract.ApplicationId = queueContract.ApplicationId;
            commentContract.ClientId = queueContract.ClientId;
            commentContract.PostId = queueContract.PostId;
            commentContract.ParentId = queueContract.ParentId;
            commentContract.Environment = queueContract.Environment;
            

            switch (queueContract.Action)
            {
                case QueueActions.Create:
                    var addResult = await commentService.AddComment(commentContract, queueContract.UserId);
                    log.LogInformation($"New comment added: {JsonConvert.SerializeObject(addResult)}");
                    break;
                case QueueActions.Update:
                    var updateResult = await commentService.UpdateComment(queueContract.Id, commentContract, queueContract.UserId);
                    log.LogInformation($"Comment updated: {JsonConvert.SerializeObject(updateResult)}");
                    break;
                case QueueActions.Delete:
                    var deleteResult = await commentService.DeleteComment(queueContract.Id, queueContract.ApplicationId, queueContract.UserId,queueContract.Environment);
                    log.LogInformation($"Comment deleted: {JsonConvert.SerializeObject(deleteResult)}");
                    break;
                default:
                    break;
            }

            log.LogInformation($"Comment Queue trigger function processed: {queueMessage}");
        }

     
    }
}
