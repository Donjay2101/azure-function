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
using Petronas.Services.Social.Helpers;
using Petronas.Services.Social.Services;
using Petronas.Services.Social.Services.Interfaces;
using Petronas.Services.Social.Contracts.FunctionInput;
using Petronas.Services.Social.ViewModels;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Petronas.Services.Social.Functions
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class ApplicationFunction
    {
        [FunctionName("ApplicationFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                RequestMethods.Get,
                RequestMethods.Post,
                RequestMethods.Put,
                RequestMethods.Delete,
                Route = "application/{*id}")]HttpRequest request,
            string id,
            ILogger log,
            [Inject]IApplicationService applicationService)
        {
            try
            {
                var applicationInput = new ApplicationFunctionInput
                {
                    Id = id,
                    Request = request
                };

                switch (request.Method)
                {
                    case RequestMethods.Get:
                        return await Get(applicationInput, applicationService);
                    case RequestMethods.Post:
                        return await Add(applicationInput);
                    case RequestMethods.Put:
                        return await Update(applicationInput);
                    case RequestMethods.Delete:
                        return await Delete(applicationInput);
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

        private static async Task<IActionResult> Get(ApplicationFunctionInput applicationInput, IApplicationService applicationService)
        {
            if (string.IsNullOrWhiteSpace(applicationInput.Id))
            {
                var getContract = GeneralHelper.GetPagedListContract<PagedListContract>(applicationInput.Request);
                var applications = await applicationService.GetApplicationList(getContract);
                return applications;
            }
            else
            {
                var application = await applicationService.GetApplication(applicationInput.Id);
                return application;
            }
        }

        private static async Task<IActionResult> Add(ApplicationFunctionInput applicationInput)
        {
            var queueService = new QueueService(QueueNames.Applications);
            var queueContract = new ApplicationQueueContract
            {
                UserId = applicationInput.UserId
            };
            var payloads = await applicationInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Create, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Update(ApplicationFunctionInput applicationInput)
        {
            var queueService = new QueueService(QueueNames.Applications);
            var queueContract = new QueueMessageContract
            {
                Id = applicationInput.Id,
                UserId = applicationInput.UserId

            };
            var payloads = await applicationInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Update, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Delete(ApplicationFunctionInput applicationInput)
        {
            var queueService = new QueueService(QueueNames.Applications);
            var queueContract = new QueueMessageContract
            {
                Id = applicationInput.Id,
                UserId = applicationInput.UserId
            };
            var payloads = await applicationInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Delete, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }
    }
}
