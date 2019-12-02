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
    public static class ClientQueueFunction
    {
        [FunctionName("ClientQueueFunction")]
        public static async Task Run(
            [QueueTrigger(
                QueueNames.Clients,
                Connection = AppSettings.StorageConnectionString)]string queueMessage,
            ILogger log,
            [Inject]IClientService clientService)
        {
            if (queueMessage == null)
                throw new ArgumentNullException(nameof(queueMessage));

            var queueContract = JsonConvert.DeserializeObject<ClientQueueContract>(queueMessage);
            ClientContract clientContract = new ClientContract ();
            if(!string.IsNullOrEmpty(queueContract.Payloads))
            {
                clientContract = JsonConvert.DeserializeObject<ClientContract>(queueContract.Payloads);
            }
            clientContract.ApplicationId = queueContract.ApplicationId;
            clientContract.Environment = queueContract.Environment;

            switch (queueContract.Action)
            {
                case QueueActions.Create:
                    var addResult = await clientService.AddClient(clientContract, queueContract.UserId);
                    log.LogInformation($"New client added: {JsonConvert.SerializeObject(addResult)}");
                    break;
                case QueueActions.Update:
                    var updateResult = await clientService.UpdateClient(queueContract.Id, clientContract, queueContract.UserId);
                    log.LogInformation($"Client updated: {JsonConvert.SerializeObject(updateResult)}");
                    break;
                case QueueActions.Delete:
                    var deleteResult = await clientService.DeleteClient(queueContract.Id, queueContract.ApplicationId, queueContract.UserId, queueContract.Environment);
                    log.LogInformation($"Client deleted: {JsonConvert.SerializeObject(deleteResult)}");
                    break;
                default:
                    break;
            }

            log.LogInformation($"Client Queue trigger function processed: {queueMessage}");
        }
    }
}
