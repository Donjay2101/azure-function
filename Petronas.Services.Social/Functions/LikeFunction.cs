using System;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Petronas.Services.Social.Contracts.FunctionInput;
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
using Microsoft.Extensions.Logging;
using Petronas.Services.Social.ViewModels;
using System.Net;

namespace Petronas.Services.Social.Functions
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class LikeFunction
    {
        [FunctionName("LikeFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                RequestMethods.Get,
                RequestMethods.Post,
                RequestMethods.Put,
                RequestMethods.Delete,
                Route = "like/{*id}")]HttpRequest request,
            string id,
            ILogger log,
            [Inject]ILikeService likeService,
            [Inject]IClientFeatureService clientFeatureService,
            [Inject]IPostService postService)
        {
            try
            {
                if (!GeneralHelper.IsHeaderExist(RequestHeaders.Environment, request))
                {
                    return new ResponseObjectModel(HttpStatusCode.InternalServerError, "Environment header does not exist in request.");
                }

                var likeFunctionInput = new LikeFunctionInput
                {
                    Id = id,
                    ApplicationId = request.Headers[RequestHeaders.ApplicationId].ToString(),
                    ClientId = request.Headers[RequestHeaders.ClientId].ToString(),
                    PostId = request.Headers[RequestHeaders.PostId].ToString(),
                    LikeType = (Constants.Enums.LikeTypes)int.Parse(request.Headers[RequestHeaders.Type].ToString()),
                    TypeId = request.Headers[RequestHeaders.TypeId].ToString(),
                    Request = request,
                    Environment = request.Headers[RequestHeaders.Environment].ToString()
                };

                if (string.IsNullOrWhiteSpace(likeFunctionInput.ApplicationId))
                    return new BadRequestObjectResult(ErrorMessages.ApplicationIdNotValid);

                if (string.IsNullOrWhiteSpace(likeFunctionInput.ClientId))
                    return new BadRequestObjectResult(ErrorMessages.ClientIdNotValid);

                if (string.IsNullOrWhiteSpace(likeFunctionInput.TypeId))
                    return new BadRequestObjectResult(ErrorMessages.TypeIdNotValid);

                switch (request.Method)
                {
                    case RequestMethods.Get:
                        return await Get(likeFunctionInput, likeService);
                    case RequestMethods.Post:
                        return await Add(likeFunctionInput, postService, clientFeatureService);
                    case RequestMethods.Put:
                        return await Update(likeFunctionInput);
                    case RequestMethods.Delete:
                        return await Delete(likeFunctionInput, postService, clientFeatureService);
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

        private static async Task<IActionResult> Get(LikeFunctionInput likeFunctionInput, ILikeService likeService)
        {
            if (string.IsNullOrWhiteSpace(likeFunctionInput.Id))
            {
                var getContract = GeneralHelper.GetPagedListContract<LikeListContract>(likeFunctionInput.Request);
                getContract.TypeId = likeFunctionInput.TypeId;
                getContract.Environment = likeFunctionInput.Environment;
                var likes = await likeService.GetLikes(getContract);
                return likes;
            }
            else
            {
                var like = await likeService.GetLikeDetail(likeFunctionInput.Id, likeFunctionInput.TypeId, likeFunctionInput.Environment);
                return like;
            }
        }

        private static async Task<IActionResult> Add(LikeFunctionInput likeFunctionInput, IPostService postService, IClientFeatureService clientFeatureService)
        {
            var queueService = new QueueService(QueueNames.Likes);
            var queueContract = ConvertToContract(likeFunctionInput);
            var payloads = await likeFunctionInput.Request.ReadAsStringAsync();

            var queueResult = await queueService.AddMessage(QueueActions.Create, queueContract, payloads);

            /*
               * increase like count and toalpoint post collection.
             */

            likeFunctionInput.ParentId = likeFunctionInput.TypeId;
            likeFunctionInput.ClientFeatureService = clientFeatureService;
            await QueueHelper.IncreaseCountInPost(likeFunctionInput, "LikeCount", Constants.Enums.UpdateAction.Increment, Constants.Enums.ClientFeature.Like,true);

            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Update(LikeFunctionInput likeFunctionInput)
        {
            var queueService = new QueueService(QueueNames.Likes);
            var queueContract = ConvertToContract(likeFunctionInput);
            var payloads = await likeFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Update, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Delete(LikeFunctionInput likeFunctionInput, IPostService postService, IClientFeatureService clientFeatureService)
        {
            var queueService = new QueueService(QueueNames.Likes);
            var queueContract = ConvertToContract(likeFunctionInput);
            var payloads = await likeFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Delete, queueContract, payloads);

            /*
               * decrease like count and toalpoint post collection.
             */
            likeFunctionInput.ParentId = likeFunctionInput.TypeId;
            likeFunctionInput.ClientFeatureService = clientFeatureService;
            await QueueHelper.IncreaseCountInPost(likeFunctionInput, "LikeCount", Constants.Enums.UpdateAction.Decrement, Constants.Enums.ClientFeature.Like, true);
            //await GeneralHelper.IncreaseCountInPost(typeId, clientId, applicationId, "LikeCount", Enums.ClientFeature.Like, postService, clientFeatureService, Enums.UpdateAction.Decrement);

            return new OkObjectResult(queueResult);
        }

        private static LikeQueueContract ConvertToContract(LikeFunctionInput input)
        {
            return new LikeQueueContract
            {
                Id = input.Id,
                ApplicationId = input.ApplicationId,
                ClientId = input.ClientId,
                PostId = input.PostId,
                Type = input.LikeType,
                TypeId = input.TypeId,
                UserId = input.UserId,
                Environment = input.Environment
            };
        }
    }
}
