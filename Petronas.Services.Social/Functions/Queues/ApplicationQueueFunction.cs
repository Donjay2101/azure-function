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
    public static class ApplicationQueueFunction
    {
        [FunctionName("ApplicationQueueFunction")]
        public static async Task Run(
            [QueueTrigger(
                QueueNames.Applications,
                Connection = AppSettings.StorageConnectionString)]string queueMessage,
            ILogger log,
            [Inject]IApplicationService applicationService)
        {
            ApplicationContract applicationContract = new ApplicationContract();
            if (queueMessage == null)
                throw new ArgumentNullException(nameof(queueMessage));

            var queueContract = JsonConvert.DeserializeObject<ApplicationQueueContract>(queueMessage);

            if(!string.IsNullOrEmpty(queueContract.Payloads))
            {
                applicationContract = JsonConvert.DeserializeObject<ApplicationContract>(queueContract.Payloads);
            }
            
            switch (queueContract.Action)
            {
                case QueueActions.Create:
                    var addResult = await applicationService.AddApplication(applicationContract, queueContract.UserId);
                    log.LogInformation($"New application added: {JsonConvert.SerializeObject(addResult)}");
                    break;
                case QueueActions.Update:
                    var updateResult = await applicationService.UpdateApplication(queueContract.Id, applicationContract, queueContract.UserId);
                    log.LogInformation($"Application updated: {JsonConvert.SerializeObject(updateResult)}");
                    break;
                case QueueActions.Delete:
                    var deleteResult = await applicationService.DeleteApplication(queueContract.Id, queueContract.UserId);
                    log.LogInformation($"Application deleted: {JsonConvert.SerializeObject(deleteResult)}");
                    break;
                default:
                    break;
            }

            log.LogInformation($"Application Queue trigger function processed: {queueMessage}");
        }

       
    }
}
