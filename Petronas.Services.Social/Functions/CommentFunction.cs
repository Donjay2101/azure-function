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
using Microsoft.Extensions.Logging;
using System;
using Petronas.Services.Social.ViewModels;
using System.Net;

namespace Petronas.Services.Social.Functions
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class CommentFunction
    {
        [FunctionName("CommentFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                RequestMethods.Get,
                RequestMethods.Post,
                RequestMethods.Put,
                RequestMethods.Delete,
                Route = "comment/{*id}")]HttpRequest request,
            string id,
            ILogger log,
            [Inject]ICommentService commentService,
            [Inject]IClientFeatureService clientFeatureService,
            [Inject]IPostService postService)
        {
            try
            {
                if (!GeneralHelper.IsHeaderExist(RequestHeaders.Environment, request))
                {
                    return new ResponseObjectModel(HttpStatusCode.InternalServerError, "Environment header does not exist in request.");
                }

                var commentFunctionInput = new CommentFunctionInput
                {
                    Id = id,
                    ApplicationId = request.Headers[RequestHeaders.ApplicationId].ToString(),
                    ClientId = request.Headers[RequestHeaders.ClientId].ToString(),
                    PostId = request.Headers[RequestHeaders.PostId].ToString(),
                    ParentId = request.Headers[RequestHeaders.ParentId].ToString(),
                    Environment = request.Headers[RequestHeaders.Environment].ToString(),
                    Request = request
                };

                if (string.IsNullOrWhiteSpace(commentFunctionInput.ApplicationId))
                    return new BadRequestObjectResult(ErrorMessages.ApplicationIdNotValid);

                if (string.IsNullOrWhiteSpace(commentFunctionInput.ClientId))
                    return new BadRequestObjectResult(ErrorMessages.ClientIdNotValid);

                if (string.IsNullOrWhiteSpace(commentFunctionInput.ParentId))
                    return new BadRequestObjectResult(ErrorMessages.TypeIdNotValid);

                switch (request.Method)
                {
                    case RequestMethods.Get:
                        return await Get(commentFunctionInput, commentService);
                    case RequestMethods.Post:
                        return await Add(commentFunctionInput, clientFeatureService, postService);
                    case RequestMethods.Put:
                        return await Update(commentFunctionInput);
                    case RequestMethods.Delete:
                        return await Delete(commentFunctionInput, clientFeatureService, postService);
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

        private static async Task<IActionResult> Get(CommentFunctionInput commentFunctionInput, ICommentService commentService)
        {
            CommentListContract getContract = null;
            if (string.IsNullOrWhiteSpace(commentFunctionInput.Id))
            {
                getContract = GeneralHelper.GetPagedListContract<CommentListContract>(commentFunctionInput.Request);
                getContract.ParentId = commentFunctionInput.ParentId;
                getContract.Environment = commentFunctionInput.Environment;
                var commentList = await commentService.GetCommentList(getContract);
                return commentList;
            }
            else
            {
                var post = await commentService.GetComment(commentFunctionInput.Id, commentFunctionInput.ParentId, commentFunctionInput.Environment);
                return post;
            }
        }

        private static async Task<IActionResult> Add(CommentFunctionInput commentFunctionInput, IClientFeatureService clientFeatureService, IPostService postService)
        {
            var queueService = new QueueService(QueueNames.Comments);
            var queueContract = new CommentQueueContract
            {
                ApplicationId = commentFunctionInput.ApplicationId,
                ClientId = commentFunctionInput.ClientId,
                PostId = commentFunctionInput.PostId,
                ParentId = commentFunctionInput.ParentId,
                UserId = commentFunctionInput.UserId,
                Environment = commentFunctionInput.Environment
            };
            var payloads = await commentFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Create, queueContract, payloads);

            /*
             increase comment count and points in post collection.
             */
            commentFunctionInput.ClientFeatureService = clientFeatureService;
            await QueueHelper.IncreaseCountInPost(commentFunctionInput, "CommentCount", Constants.Enums.UpdateAction.Increment, Constants.Enums.ClientFeature.Comment, true);

            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Update(CommentFunctionInput commentFunctionInput)
        {
            var queueService = new QueueService(QueueNames.Comments);
            var queueContract = new CommentQueueContract
            {
                Id = commentFunctionInput.Id,
                ApplicationId = commentFunctionInput.ApplicationId,
                ClientId = commentFunctionInput.ClientId,
                PostId = commentFunctionInput.PostId,
                ParentId = commentFunctionInput.ParentId,
                UserId = commentFunctionInput.UserId,
                Environment = commentFunctionInput.Environment
            };
            var payloads = await commentFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Update, queueContract, payloads);
            return new OkObjectResult(queueResult);
        }

        private static async Task<IActionResult> Delete(CommentFunctionInput commentFunctionInput, IClientFeatureService clientFeatureService, IPostService postService)
        {
            var queueService = new QueueService(QueueNames.Comments);
            var queueContract = new CommentQueueContract
            {
                Id = commentFunctionInput.Id,
                ApplicationId = commentFunctionInput.ApplicationId,
                ClientId = commentFunctionInput.ClientId,
                PostId = commentFunctionInput.PostId,
                ParentId = commentFunctionInput.ParentId,
                UserId = commentFunctionInput.UserId,
                Environment = commentFunctionInput.Environment
            };
            var payloads = await commentFunctionInput.Request.ReadAsStringAsync();
            var queueResult = await queueService.AddMessage(QueueActions.Delete, queueContract, payloads);

            /*
            decrease comment count and points in post collection.
            */
            commentFunctionInput.ClientFeatureService = clientFeatureService;
            await QueueHelper.IncreaseCountInPost(commentFunctionInput, "CommentCount", Constants.Enums.UpdateAction.Decrement, Constants.Enums.ClientFeature.Comment, true);

            return new OkObjectResult(queueResult);
        }
    }
}
