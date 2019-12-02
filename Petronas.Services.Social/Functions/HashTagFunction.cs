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
    public static class HashTagFunction
    {
        [FunctionName("HashTagFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                RequestMethods.Get,
                RequestMethods.Post,
                RequestMethods.Put,
                RequestMethods.Delete,
                Route = "Hashtag/{*id}")]HttpRequest request,
            string id,
            ILogger log,
            [Inject]IHashTagService hashTagService)
        {
            try
            {
                if (!GeneralHelper.IsHeaderExist(RequestHeaders.Environment, request))
                {
                    return new ResponseObjectModel(HttpStatusCode.InternalServerError, "Environment header does not exist in request.");
                }

                var hashtagInput = new HashTagFunctionInput
                {
                    Id = id,
                    ApplicationId = request.Headers[RequestHeaders.ApplicationId].ToString(),
                    Environment = request.Headers[RequestHeaders.Environment].ToString(),
                    ClientId = request.Headers[RequestHeaders.ClientId].ToString(),
                    ParentId = request.Headers[RequestHeaders.ApplicationId].ToString(),
                    Request = request
                };

                if (string.IsNullOrWhiteSpace(hashtagInput.ApplicationId))
                    return new BadRequestObjectResult(ErrorMessages.ApplicationIdNotValid);

                switch (request.Method)
                {
                    case RequestMethods.Get:
                        return await Get(hashtagInput, hashTagService);
                    case RequestMethods.Post:
                        return await Add(hashtagInput);
                    case RequestMethods.Put:
                        return await Update(hashtagInput);
                    case RequestMethods.Delete:
                        return await Delete(hashtagInput);
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

        private static async Task<IActionResult> Get(HashTagFunctionInput hashTagFunctionInput, IHashTagService hashTagService)
        {
            if (string.IsNullOrWhiteSpace(hashTagFunctionInput.Id))
            {
                var getContract = GeneralHelper.GetPagedListContract<HashTagListContract>(hashTagFunctionInput.Request);
                var applications = await hashTagService.GetAll(getContract);
                return applications;
            }
            else
            {
                var application = await hashTagService.GetHashTagById(hashTagFunctionInput.Id, hashTagFunctionInput.ParentId, hashTagFunctionInput.Environment);
                return application;
            }
        }

        private static async Task<IActionResult> Add(HashTagFunctionInput hastagInput)
        {
            var queueService = new QueueService(QueueNames.HashTags);
            var queueContract = new HashTagQueueContract
            {
                UserId = hastagInput.UserId,
                ParentId = hastagInput.ParentId,
                ApplicationId = hastagInput.ApplicationId,
                 ClientId = hastagInput.ClientId,
                 Environment = hastagInput.Environment
            };
            var payloads = await hastagInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Create, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Update(HashTagFunctionInput hastagInput)
        {
            var queueService = new QueueService(QueueNames.HashTags);
            var queueContract = new HashTagQueueContract
            {
                Id = hastagInput.Id,
                UserId = hastagInput.UserId,
                ApplicationId = hastagInput.ApplicationId,
                ClientId = hastagInput.ClientId,
                ParentId = hastagInput.ParentId,
                Environment = hastagInput.Environment

            };
            var payloads = await hastagInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Update, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Delete(HashTagFunctionInput hashtagInput)
        {
            var queueService = new QueueService(QueueNames.HashTags);
            var queueContract = new HashTagQueueContract
            {
                Id = hashtagInput.Id,
                UserId = hashtagInput.UserId,
                ApplicationId = hashtagInput.ApplicationId,
                ClientId = hashtagInput.ClientId,
                ParentId  = hashtagInput.ParentId,
                
            };
            var payloads = await hashtagInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Delete, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }
    }
}
