using System;
using System.Net;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Petronas.Services.Social.Configurations;
using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Helpers;
using Petronas.Services.Social.Services;
using Petronas.Services.Social.Services.Interfaces;
using Petronas.Services.Social.Contracts.FunctionInput;
using Petronas.Services.Social.ViewModels;
using Microsoft.Extensions.Logging;

namespace Petronas.Services.Social.Functions
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class ClientFunction
    {
        [FunctionName("ClientFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                RequestMethods.Get,
                RequestMethods.Post,
                RequestMethods.Put,
                RequestMethods.Delete,
                Route = "client/{*id}")]HttpRequest request,
            string id,
            ILogger log,
            [Inject]IClientService clientService)
        {
            try
            {
                if (!GeneralHelper.IsHeaderExist(RequestHeaders.Environment, request))
                {
                    return new ResponseObjectModel(HttpStatusCode.InternalServerError, "Environment header does not exist in request.");
                }

                var clientFunctionInput = new ClientFunctionInput
                {
                    Id = id,
                    ApplicationId = request.Headers[RequestHeaders.ApplicationId].ToString(),
                    Environment = request.Headers[RequestHeaders.Environment].ToString(),
                    Request = request
                };

                if (string.IsNullOrWhiteSpace(clientFunctionInput.ApplicationId))
                    return new BadRequestObjectResult(ErrorMessages.ApplicationIdNotValid);

                switch (request.Method)
                {
                    case RequestMethods.Get:
                        return await Get(clientFunctionInput, clientService);
                    case RequestMethods.Post:
                        return await Add(clientFunctionInput);
                    case RequestMethods.Put:
                        return await Update(clientFunctionInput);
                    case RequestMethods.Delete:
                        return await Delete(clientFunctionInput);
                    default:
                        return new StatusCodeResult((int)HttpStatusCode.MethodNotAllowed);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                return new ResponseObjectModel(HttpStatusCode.InternalServerError, ex);
            }
        }

        private static async Task<IActionResult> Get(ClientFunctionInput clientFunctionInput, IClientService clientService)
        {
            if (string.IsNullOrWhiteSpace(clientFunctionInput.Id))
            {
                var getContract = GeneralHelper.GetPagedListContract<ClientListContract>(clientFunctionInput.Request);
                getContract.ApplicationId = clientFunctionInput.ApplicationId;
                getContract.Environment = clientFunctionInput.Environment;
                var clientList = await clientService.GetClientList(getContract);
                return clientList;
            }
            else
            {
                var client = await clientService.GetClient(clientFunctionInput.Id, clientFunctionInput.ApplicationId, clientFunctionInput.Environment);
                return client;
            }
        }

        private static async Task<IActionResult> Add(ClientFunctionInput clientFunctionInput)
        {
            var queueService = new QueueService(QueueNames.Clients);
            var queueContract = new ClientQueueContract
            {
                ApplicationId = clientFunctionInput.ApplicationId,
                UserId = Guid.NewGuid().ToString(), // For testing only
                Environment = clientFunctionInput.Environment
            };
            var payloads = await clientFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Create, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Update(ClientFunctionInput clientFunctionInput)
        {
            var queueService = new QueueService(QueueNames.Clients);
            var queueContract = new ClientQueueContract
            {
                Id = clientFunctionInput.Id,
                ApplicationId = clientFunctionInput.ApplicationId,
                UserId = Guid.NewGuid().ToString(), // For testing only,
                Environment = clientFunctionInput.Environment
            };
            var payloads = await clientFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Update, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Delete(ClientFunctionInput clientFunctionInput)
        {
            var queueService = new QueueService(QueueNames.Clients);
            var queueContract = new ClientQueueContract
            {
                Id = clientFunctionInput.Id,
                ApplicationId = clientFunctionInput.ApplicationId,
                UserId = clientFunctionInput.UserId,
                Environment = clientFunctionInput.Environment
            };
            var payloads = await clientFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Delete, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }
    }
}
