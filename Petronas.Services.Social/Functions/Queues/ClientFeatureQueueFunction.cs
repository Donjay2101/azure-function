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
    public static class ClientFeatureQueueFunction
    {
        [FunctionName("ClientFeatureQueueFunction")]
        public static async Task Run(
            [QueueTrigger(
                QueueNames.ClientFeatures,
                Connection = AppSettings.StorageConnectionString)]string queueMessage,
            ILogger log,
            [Inject]IClientFeatureService clientFeatureService)
        {
            ClientFeatureContract clientFeatureContract = new ClientFeatureContract();
            if (queueMessage == null)
                throw new ArgumentNullException(nameof(queueMessage));

            var queueContract = JsonConvert.DeserializeObject<ClientFeatureQueueContract>(queueMessage);
            if(!string.IsNullOrEmpty(queueContract.Payloads))
            {
                clientFeatureContract = JsonConvert.DeserializeObject<ClientFeatureContract>(queueContract.Payloads);
            }
            clientFeatureContract.ApplicationId = queueContract.ApplicationId;
            clientFeatureContract.ClientId = queueContract.ClientId;
            clientFeatureContract.Environment = queueContract.Environment;

            switch (queueContract.Action)
            {
                case QueueActions.Create:
                    var addResult = await clientFeatureService.AddClientFeature(clientFeatureContract, queueContract.UserId);
                    log.LogInformation($"New client feature added: {JsonConvert.SerializeObject(addResult)}");
                    break;
                case QueueActions.Update:
                    var updateResult = await clientFeatureService.UpdateClientFeature(queueContract.Id, clientFeatureContract, queueContract.UserId);
                    log.LogInformation($"Client feature updated: {JsonConvert.SerializeObject(updateResult)}");
                    break;
                case QueueActions.Delete:
                    var deleteResult = await clientFeatureService.DeleteClientFeature(queueContract.Id, queueContract.ApplicationId, queueContract.UserId, queueContract.Environment);
                    log.LogInformation($"Client feature deleted: {JsonConvert.SerializeObject(deleteResult)}");
                    break;
                default:
                    break;
            }

            log.LogInformation($"Client Feature Queue trigger function processed: {queueMessage}");
        }

       
    }
}
