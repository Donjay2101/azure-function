using System;
using System.Net;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Petronas.Services.Social.Configurations;
using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Contracts.FunctionInput;
using Petronas.Services.Social.Helpers;
using Petronas.Services.Social.Services;
using Petronas.Services.Social.Services.Interfaces;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Functions
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class ClientFeatureFunction
    {
        [FunctionName("ClientFeatureFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger (
                AuthorizationLevel.Anonymous,
                RequestMethods.Get,
                RequestMethods.Post,
                RequestMethods.Put,
                RequestMethods.Delete,
                Route = "client-feature/{*id}")] HttpRequest request,
            string id,
            ILogger log, [Inject] IClientFeatureService clientFeatureService)
        {
            try
            {
                if (!GeneralHelper.IsHeaderExist(RequestHeaders.Environment, request))
                {
                    return new ResponseObjectModel(HttpStatusCode.InternalServerError, "Environment header does not exist in request.");
                }

                var clientFunctionInput = new ClientFeatureFunctionInput
                {
                    Id = id,
                    ApplicationId = request.Headers[RequestHeaders.ApplicationId].ToString(),
                    ClientId = request.Headers[RequestHeaders.ClientId].ToString(),
                    Environment = request.Headers[RequestHeaders.Environment].ToString(),
                    Request = request
                };

                if (string.IsNullOrWhiteSpace(clientFunctionInput.ApplicationId))
                    return new BadRequestObjectResult(ErrorMessages.ApplicationIdNotValid);

                if (string.IsNullOrWhiteSpace(clientFunctionInput.ClientId))
                    return new BadRequestObjectResult(ErrorMessages.ClientIdNotValid);

                switch (request.Method)
                {
                    case RequestMethods.Get:
                        return await Get(clientFunctionInput, clientFeatureService);
                    case RequestMethods.Post:
                        return await Add(clientFunctionInput);
                    case RequestMethods.Put:
                        return await Update(clientFunctionInput);
                    case RequestMethods.Delete:
                        return await Delete(clientFunctionInput);
                    default:
                        return new BadRequestResult();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                return new ResponseObjectModel(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static async Task<IActionResult> Get(ClientFeatureFunctionInput clientFunctionInput, IClientFeatureService clientFeatureService)
        {
            if (string.IsNullOrWhiteSpace(clientFunctionInput.Id))
            {
                var getContract = GeneralHelper.GetPagedListContract<ClientFeatureListContract>(clientFunctionInput.Request);
                getContract.ApplicationId = clientFunctionInput.ApplicationId;
                getContract.Environment = clientFunctionInput.Environment;
                var clientList = await clientFeatureService.GetClientFeatureList(getContract);
                return clientList;
            }
            else
            {
                var client = await clientFeatureService.GetClientFeature(clientFunctionInput.Id, clientFunctionInput.ApplicationId, clientFunctionInput.Environment);
                return client;
            }
        }

        private static async Task<IActionResult> Add(ClientFeatureFunctionInput clientFunctionInput)
        {
            var queueService = new QueueService(QueueNames.ClientFeatures);
            var queueContract = new ClientFeatureQueueContract
            {
                ApplicationId = clientFunctionInput.ApplicationId,
                ClientId = clientFunctionInput.ClientId,
                UserId = clientFunctionInput.UserId,
                Environment = clientFunctionInput.Environment
            };
            var payloads = await clientFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Create, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Update(ClientFeatureFunctionInput clientFunctionInput)
        {
            var queueService = new QueueService(QueueNames.ClientFeatures);
            var queueContract = new ClientFeatureQueueContract
            {
                Id = clientFunctionInput.Id,
                ApplicationId = clientFunctionInput.ApplicationId,
                ClientId = clientFunctionInput.ClientId,
                UserId = clientFunctionInput.UserId,
                Environment = clientFunctionInput.Environment
            };
            var payloads = await clientFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Update, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Delete(ClientFeatureFunctionInput clientFunctionInput)
        {
            var queueService = new QueueService(QueueNames.ClientFeatures);
            var queueContract = new ClientFeatureQueueContract
            {
                Id = clientFunctionInput.Id,
                ApplicationId = clientFunctionInput.ApplicationId,
                ClientId = clientFunctionInput.ClientId,
                UserId = clientFunctionInput.UserId,
                Environment = clientFunctionInput.Environment
            };
            var payloads = await clientFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Delete, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }
    }
}