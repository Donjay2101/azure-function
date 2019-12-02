    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Petronas.Services.Social.Configurations;
    using Petronas.Services.Social.Constants;
    using Petronas.Services.Social.Contracts;
    using Petronas.Services.Social.Helpers;
    using Petronas.Services.Social.Models;
    using Petronas.Services.Social.Repositories.Interfaces;
    using Petronas.Services.Social.Services.Interfaces;
    using Petronas.Services.Social.ViewModels;

    namespace Petronas.Services.Social.Services
    {
        public class CommentService : BaseService, ICommentService
        {
            private readonly ICommentRepository _commentRepository;
            private readonly SignalRService signalRService;

            public CommentService(
                ICommentRepository commentRepository,
                IApplicationRepository applicationRepository) : base(applicationRepository)
            {
                _commentRepository = commentRepository;
                signalRService = new SignalRService(System.Environment.GetEnvironmentVariable(AppSettings.AzureSignalRConnectionString));
            }

            public ResponseObjectModel GetComments(string parentId)
            {

                try
                {

                    if (string.IsNullOrEmpty(parentId) || string.IsNullOrWhiteSpace(parentId))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ParentIdRequired);
                    }

                    var result = _commentRepository.GetAll(
                                    x => !x.IsDeleted && x.ParentId == parentId,
                                    new FeedOptions
                                    {
                                        MaxItemCount = -1,
                                        EnableCrossPartitionQuery = true
                                    }).ToList();
                    return new ResponseObjectModel(HttpStatusCode.OK, result);
                }
                catch (DocumentClientException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Comment.CommentNotFound);
                    }
                    else
                    {
                        throw;
                    }
                }

            }

            public ResponseObjectModel GetUserComments(string resourceId)
            {
                try
                {
                    if (string.IsNullOrEmpty(resourceId) || string.IsNullOrWhiteSpace(resourceId))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ResourceIdRequired);
                    }
                    var result = _commentRepository.GetAll(
                    x => !x.IsDeleted && x.ResourceId == resourceId,
                    new FeedOptions
                    {
                        MaxItemCount = -1,
                        EnableCrossPartitionQuery = true
                    }).ToList();
                    return new ResponseObjectModel(HttpStatusCode.OK, result);

                }
                catch (DocumentClientException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Comment.CommentNotFound);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            public async Task<ResponseObjectModel> GetCommentList(CommentListContract contract)
            {
                try
                {
                    if (contract == null)
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.ContractCannotBeNull);
                    }

                    if (string.IsNullOrEmpty(contract.ParentId) || string.IsNullOrWhiteSpace(contract.ParentId))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ParentIdRequired);
                    }

                    if (string.IsNullOrEmpty(contract.Environment) || string.IsNullOrWhiteSpace(contract.Environment))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                    }


                    var queryResult = await _commentRepository.GetList(
                        x => !x.IsDeleted,
                        DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ParentId),
                        contract.PageSize,
                        contract.ContinuationToken);
                    var result = queryResult.GetType<CommentListModel>();

                    return HttpHelper.ReturnObject(result);
                }
                catch (DocumentClientException ex)
                {
                    return HttpHelper.ThrowError(ex);
                }

            }

            public async Task<ResponseObjectModel> GetComment(string id, string parentId, string environment)
            {
                try
                {
                    if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.CommentIdRequired);
                    }

                    if (string.IsNullOrEmpty(parentId) || string.IsNullOrWhiteSpace(parentId))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ParentIdRequired);
                    }

                    if (string.IsNullOrEmpty(environment) || string.IsNullOrWhiteSpace(environment))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                    }


                    var result = await _commentRepository.Get(id, DocumentHelper.GetPartitionKeyByEnvironment(environment, parentId));
                    return HttpHelper.ReturnObject(result);
                }
                catch (DocumentClientException ex)
                {
                    return HttpHelper.ThrowError(ex);
                }

            }

            public async Task<ResponseObjectModel> AddComment(CommentContract contract, string userId)
            {
                try
                {
                    if (contract == null)
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.ContractCannotBeNull);
                    }

                    if (string.IsNullOrEmpty(contract.Content) || string.IsNullOrWhiteSpace(contract.Content))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ContentRequired);
                    }

                    if (string.IsNullOrEmpty(contract.ParentId) || string.IsNullOrWhiteSpace(contract.ParentId))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ParentIdRequired);
                    }

                    if (string.IsNullOrEmpty(contract.ClientId) || string.IsNullOrWhiteSpace(contract.ClientId))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);
                    }

                    if (string.IsNullOrEmpty(contract.ApplicationId) || string.IsNullOrWhiteSpace(contract.ApplicationId))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);
                    }

                    if (string.IsNullOrEmpty(contract.Environment) || string.IsNullOrWhiteSpace(contract.Environment))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                    }

                    bool isEnvironmentNotAllowed = await IsEnvironmentNotAllowed(contract.ApplicationId, contract.Environment);

                    if(isEnvironmentNotAllowed){
                        return HttpHelper.ThrowError(HttpStatusCode.MethodNotAllowed, ErrorMessages.Application.ApplicationNotFoundOrNotAllowed);
                    }

                    var comment = AutoMapperConfig.MapObject<CommentContract, Comment>(contract);
                    comment.Id = GeneralHelper.GenerateNewId();
                    comment.CreatedOn = DateTime.UtcNow;
                    comment.CreatedBy = userId;
                    var result = await _commentRepository.Add(comment);
                    signalRService.SendAsync(HubNames.Comment, HubMethods.Add, result.ToString());
                    return HttpHelper.ReturnObject(result);
                }
                catch (DocumentClientException ex)
                {
                    return HttpHelper.ThrowError(ex);
                }

            }

            public async Task<ResponseObjectModel> UpdateComment(string id, CommentContract contract, string userId)
            {
                try
                {
                    if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.CommentIdRequired);
                    }

                    if (contract == null)
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.ContractCannotBeNull);
                    }


                    if (string.IsNullOrEmpty(contract.ParentId) || string.IsNullOrWhiteSpace(contract.ParentId))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ParentIdRequired);
                    }

                    if (string.IsNullOrEmpty(contract.ClientId) || string.IsNullOrWhiteSpace(contract.ClientId))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);
                    }

                    if (string.IsNullOrEmpty(contract.ApplicationId) || string.IsNullOrWhiteSpace(contract.ApplicationId))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);
                    }

                    if (string.IsNullOrEmpty(contract.Content) || string.IsNullOrWhiteSpace(contract.Content))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ContentRequired);
                    }

                    if (string.IsNullOrEmpty(contract.Environment) || string.IsNullOrWhiteSpace(contract.Environment))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                    }



                    var comment = AutoMapperConfig.MapObject<CommentContract, Comment>(contract);
                    var partitionKey = DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ParentId);
                    var result = await _commentRepository.Update(id, comment, userId, partitionKey);
                    signalRService.SendAsync(HubNames.Comment, HubMethods.Update, result.ToString());
                    return HttpHelper.ReturnObject(result);
                }
                catch (DocumentClientException ex)
                {
                    return HttpHelper.ThrowError(ex);
                }

            }

            public async Task<ResponseObjectModel> DeleteComment(string id, string parentId, string userId,string environment)
            {
                try
                {

                    if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.CommentIdRequired);
                    }

                    if (string.IsNullOrEmpty(parentId) || string.IsNullOrWhiteSpace(parentId))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ParentIdRequired);
                    }

                    if (string.IsNullOrEmpty(environment) || string.IsNullOrWhiteSpace(environment))
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                    }

                    var partitionKey = DocumentHelper.GetPartitionKeyByEnvironment(environment, parentId);
                    var result = await _commentRepository.Delete(id, userId, partitionKey);
                    signalRService.SendAsync(HubNames.Comment, HubMethods.Delete, result.ToString());
                    return HttpHelper.ReturnObject(result);
                }
                catch (DocumentClientException ex)
                {
                    return HttpHelper.ThrowError(ex);
                }

            }
        }
    }
