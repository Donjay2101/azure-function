using System;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Petronas.Services.Social.Configurations;
using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Services;
using Petronas.Services.Social.Services.Interfaces;
using Petronas.Services.Social.ViewModels;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Petronas.Services.Social.Functions
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class EnvironmentFunction
    {
        [FunctionName("EnvironmentFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                RequestMethods.Get,
                RequestMethods.Post,
                RequestMethods.Put,
                RequestMethods.Delete,
                Route = "environment")]HttpRequest request,
            ILogger log,
            [Inject]IApplicationService applicationService)
        {
            try
            {
                var functionInput = new FunctionInputBase
                {
                    Request = request
                };

                switch (request.Method)
                {
                    case RequestMethods.Post:
                        return await Add(functionInput);
                    case RequestMethods.Delete:
                        return await Delete(functionInput);
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

        private static async Task<IActionResult> Add(FunctionInputBase functionInput)
        {
            var queueService = new QueueService(QueueNames.Environments);
            var queueContract = new EnvironmentQueueContract
            {
                UserId = functionInput.UserId
            };
            var payloads = await functionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Create, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Delete(FunctionInputBase functionInput)
        {
            var queueService = new QueueService(QueueNames.Environments);
            var queueContract = new EnvironmentQueueContract
            {
                UserId = functionInput.UserId
            };
            var payloads = await functionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Delete, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }
    }
}
