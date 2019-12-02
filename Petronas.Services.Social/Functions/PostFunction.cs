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
using Petronas.Services.Social.Models;
using Petronas.Services.Social.Services;
using Petronas.Services.Social.Services.Interfaces;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Functions
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class PostFunction
    {
        [FunctionName("PostFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger (
                AuthorizationLevel.Anonymous,
                RequestMethods.Get,
                RequestMethods.Post,
                RequestMethods.Put,
                RequestMethods.Delete,
                Route = "post/{*id}")] HttpRequest request,
            string id,
            ILogger log, [Inject] IPostService postService)
        {
            try
            {
                if (!GeneralHelper.IsHeaderExist(RequestHeaders.Environment, request))
                {
                    return new ResponseObjectModel(HttpStatusCode.InternalServerError, "Environment header does not exist in request.");
                }

                var postFunctionInput = new PostFunctionInput
                {
                    Id = id,
                    ApplicationId = request.Headers[RequestHeaders.ApplicationId].ToString(),
                    ClientId = request.Headers[RequestHeaders.ClientId].ToString(),
                    Request = request,
                    Environment = request.Headers[RequestHeaders.Environment].ToString()
                };

                if (string.IsNullOrWhiteSpace(postFunctionInput.ApplicationId))
                    return new BadRequestObjectResult(ErrorMessages.ApplicationIdNotValid);

                if (string.IsNullOrWhiteSpace(postFunctionInput.ClientId))
                    return new BadRequestObjectResult(ErrorMessages.ClientIdNotValid);

                switch (request.Method)
                {
                    case RequestMethods.Get:
                        return await Get(postFunctionInput, postService);
                    case RequestMethods.Post:
                        return await Add(postFunctionInput);
                    case RequestMethods.Put:
                        return await Update(postFunctionInput);
                    case RequestMethods.Delete:
                        return await Delete(postFunctionInput);
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

        private static async Task<IActionResult> Get(PostFunctionInput postFunctionInput, IPostService postService)
        {
            if (string.IsNullOrWhiteSpace(postFunctionInput.Id))
            {
                var getContract = GeneralHelper.GetPagedListContract<PostListContract>(postFunctionInput.Request);
                getContract.ClientId = postFunctionInput.ClientId;
                getContract.Environment = postFunctionInput.Environment;
                var postList = await postService.GetPostList(getContract);
                return postList;
            }
            else
            {
                var postResponseModel = await postService.GetPost(postFunctionInput.Id, postFunctionInput.ClientId, postFunctionInput.Environment);
                Post post = new Post();
                
                if (postResponseModel != null)
                {
                    post = postResponseModel.Value as Post;
                }

                post.ViewCount = ++post.ViewCount;

                /*
                 increase comment count and points in post collection.
                */
                await QueueHelper.IncreaseCountInPost(postFunctionInput, "ViewCount", Constants.Enums.UpdateAction.Increment, Constants.Enums.ClientFeature.Post, false);
                return new OkObjectResult(post);
            }
        }

        private static async Task<IActionResult> Add(PostFunctionInput postFunctionInput)
        {
            var queueService = new QueueService(QueueNames.Posts);
            var queueContract = ConvertToContract(postFunctionInput);
            var payloads = await postFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Create, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Update(PostFunctionInput postFunctionInput)
        {
            var queueService = new QueueService(QueueNames.Posts);
            var queueContract = ConvertToContract(postFunctionInput);
            var payloads = await postFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Update, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static QueueService NewMethod()
        {
            return new QueueService(QueueNames.Posts);
        }

        private static async Task<IActionResult> Delete(PostFunctionInput postFunctionInput)
        {
            var queueService = new QueueService(QueueNames.Posts);
            var queueContract = ConvertToContract(postFunctionInput);
            var payloads = await postFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Delete, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static PostQueueContract ConvertToContract(PostFunctionInput input)
        {
            return new PostQueueContract
            {
                Id = input.Id,
                ApplicationId = input.ApplicationId,
                ClientId = input.ClientId,
                UserId = input.UserId,
                Environment = input.Environment
            };
        }
    }
}
