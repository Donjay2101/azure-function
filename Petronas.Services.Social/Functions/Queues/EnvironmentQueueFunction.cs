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
    public static class EnvironmentQueueFunction
    {
        [FunctionName("EnvironmentQueueFunction")]
        public static async Task Run(
            [QueueTrigger(
                QueueNames.Environments,
                Connection = AppSettings.StorageConnectionString)]string queueMessage,
            ILogger log,
            [Inject]IApplicationService applicationService)
        {
            if (queueMessage == null)
                throw new ArgumentNullException(nameof(queueMessage));

            var queueContract = JsonConvert.DeserializeObject<EnvironmentQueueContract>(queueMessage);
            var environmentContract = JsonConvert.DeserializeObject<EnvironmentContract>(queueContract.Payloads);
            switch (queueContract.Action)
            {
                case QueueActions.Create:
                    var addResult = await applicationService.AddAllowedEnvironments(environmentContract, queueContract.UserId);
                    log.LogInformation($"New Environment added to application: {JsonConvert.SerializeObject(addResult)}");
                    break;
                case QueueActions.Delete:
                    var deleteResult = await applicationService.DeleteAllowedEnvironments(environmentContract, queueContract.UserId);;
                    log.LogInformation($"Environment deleted from application: {JsonConvert.SerializeObject(deleteResult)}");
                    break;
                default:
                    break;
            }

            log.LogInformation($"Environment Queue trigger function processed: {queueMessage}");
        }
    }
}
