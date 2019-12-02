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
    public class PostService : BaseService, IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly SignalRService signalRService;
        private readonly IApplicationService _applicationService;
        private readonly IClientService _clientService;
        private readonly IClientFeatureService _clientFeatureService;

        public PostService(
            IPostRepository postRepository,
            IApplicationService applicationService,
            IClientService clientService,
            IClientFeatureService clientFeatureService,
            IApplicationRepository applicationRepository) : base(applicationRepository)
        {
            _postRepository = postRepository;
            signalRService = new SignalRService(System.Environment.GetEnvironmentVariable(AppSettings.AzureSignalRConnectionString));
            _applicationService = applicationService;
            _clientService = clientService;
            _clientFeatureService = clientFeatureService;
        }

        public ResponseObjectModel GetAllPosts()
        {
            try
            {
                var result = _postRepository.GetAll(new FeedOptions
                {
                    MaxItemCount = -1,
                    EnableCrossPartitionQuery = true
                }).ToList();
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> GetPostList(PostListContract contract)
        {
            try
            {
                if (contract == null)
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.ContractIsNull);
                }

                if (string.IsNullOrEmpty(contract.ClientId) || string.IsNullOrWhiteSpace(contract.ClientId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);
                }

                if (string.IsNullOrWhiteSpace(contract.Environment)|| string.IsNullOrEmpty(contract.Environment))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                }

                var queryResult = await _postRepository.GetList(
                    x => !x.IsDeleted,
                    DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ClientId),
                    contract.PageSize,
                    contract.ContinuationToken);
                var result = queryResult.GetType<PostListModel>();

                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> GetPost(string id, string clientId,string environment)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Post.PostIdRequired);
                }

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrWhiteSpace(clientId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);
                }

                
                if (string.IsNullOrEmpty(environment) || string.IsNullOrWhiteSpace(environment))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                }

                var result = await _postRepository.Get(id,DocumentHelper.GetPartitionKeyByEnvironment(environment,clientId));
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> AddPost(PostContract contract, string userId)
        {
            try
            {
                if (contract == null)
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.ContractIsNull);
                }

                if (string.IsNullOrEmpty(contract.ApplicationId) || string.IsNullOrWhiteSpace(contract.ApplicationId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);
                }

                if (string.IsNullOrEmpty(contract.ClientId) || string.IsNullOrWhiteSpace(contract.ClientId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);
                }

                if (string.IsNullOrEmpty(contract.Title) || string.IsNullOrWhiteSpace(contract.Title))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Post.TitleRequired);
                }

                if (string.IsNullOrEmpty(contract.Content) || string.IsNullOrWhiteSpace(contract.Content))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Post.ContentRequired);
                }


                ClientFeature feature = null;
                //var isApplicationExist = await _applicationService.IsExisting(contract.ApplicationId);
                var isEnvironmentnotAllowed = await IsEnvironmentNotAllowed(contract.ApplicationId,contract.Environment);

                if (isEnvironmentnotAllowed)
                    return HttpHelper.ThrowError(HttpStatusCode.MethodNotAllowed, ErrorMessages.Application.ApplicationNotFoundOrNotAllowed);

                var isClientExist = await _clientService.IsExisting(contract.ClientId, contract.ApplicationId, contract.Environment);

                if (!isClientExist)
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Client.ClientNotFound);

                var featureResponseModel = await _clientFeatureService.GetClientFeature(contract.ApplicationId, contract.ClientId, Constants.Enums.ClientFeature.Post, contract.Environment);
                if (featureResponseModel != null && featureResponseModel.Value != null)
                {
                    feature = featureResponseModel.Value as ClientFeature;
                }
                var post = AutoMapperConfig.MapObject<PostContract, Post>(contract);
                post.Id = GeneralHelper.GenerateNewId();
                post.Point = feature == null ? 0 : feature.Point;
                post.AuthorId = userId;
                post.IsDraft = true;
                post.IsPublished = false;
                post.CreatedOn = DateTime.UtcNow;
                post.CreatedBy = userId;

                var result = await _postRepository.Add(post);
                signalRService.SendAsync(HubNames.Post, HubMethods.Add, result.ToString());
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> UpdatePost(string id, PostContract contract, string userId)
        {
            try
            {

                if (contract == null)
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.ContractIsNull);
                }

                if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Post.PostIdRequired);
                }

                if (string.IsNullOrEmpty(contract.ApplicationId) || string.IsNullOrWhiteSpace(contract.ApplicationId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);
                }

                if (string.IsNullOrEmpty(contract.ClientId) || string.IsNullOrWhiteSpace(contract.ClientId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);
                }


                if (string.IsNullOrEmpty(contract.Content) || string.IsNullOrWhiteSpace(contract.Content))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Post.ContentRequired);
                }

                if (string.IsNullOrEmpty(contract.Title) || string.IsNullOrWhiteSpace(contract.Title))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Post.TitleRequired);
                }

                
                if (string.IsNullOrEmpty(contract.Environment) || string.IsNullOrWhiteSpace(contract.Environment))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                }

                var post = AutoMapperConfig.MapObject<PostContract, Post>(contract);
                var result = await _postRepository.Update(id, post, userId,DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ClientId));
                signalRService.SendAsync(HubNames.Post, HubMethods.Update, result.ToString());
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> UpdatePost(string id, string userId, Post post)
        {
            try
            {
                if (post == null)
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.ContractIsNull);
                }


                if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Post.PostIdRequired);
                }

                if (string.IsNullOrEmpty(post.Environment) || string.IsNullOrWhiteSpace(post.Environment))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                }
                var partitionKey = DocumentHelper.GetPartitionKeyByEnvironment(post.Environment, post.ClientId);
                var result = await _postRepository.Update(id, post, userId, partitionKey);
                signalRService.SendAsync(HubNames.Post, HubMethods.Update, result.ToString());
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> PublishPost(string id, string clientId, string userId,string environment)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Post.PostIdRequired);
                }

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrWhiteSpace(clientId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);
                }

                if (string.IsNullOrEmpty(environment) || string.IsNullOrWhiteSpace(environment))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                }

                var partitionKey = DocumentHelper.GetPartitionKeyByEnvironment(environment, clientId);
                var post = await _postRepository.Get(id, partitionKey);
                post.IsPublished = true;
                post.PublishedDate = DateTime.UtcNow;
                var result = await _postRepository.Update(id, post, userId, partitionKey);
                signalRService.SendAsync(HubNames.Post, HubMethods.Publish, result.ToString());
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> DeletePost(string id, string clientId, string userId, string environment)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Post.PostIdRequired);
                }

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrWhiteSpace(clientId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);
                }

                if (string.IsNullOrEmpty(environment) || string.IsNullOrWhiteSpace(environment))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                }

                var result = await _postRepository.Delete(id, userId, DocumentHelper.GetPartitionKeyByEnvironment(environment, clientId));
                signalRService.SendAsync(HubNames.Post, HubMethods.Delete, result.ToString());
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<bool> IsExisting(string id, string clientId)
        {
            var result = await _postRepository.Get(id, clientId);
            return result != null ? true : false;
        }

        public async Task<ResponseObjectModel> UpdatePartial(string id, PostContract postContract, string userId)
        {
           var postResponseModel = await GetPost(id, postContract.ClientId, postContract.Environment);
                    Post post = new Post();
                    if(postResponseModel != null)
                    {
                            post = postResponseModel.Value as Post;
                    }
                    //get the property from post which needs to increment
                    if(post == null){
                        return HttpHelper.ThrowError(HttpStatusCode.NotFound,ErrorMessages.Application.ApplicationNotFound);
                    }
                    
                    var property = post.GetType().GetProperty(postContract.PropertyName);
                    int val, udpateValue = 0;
                    object resultValue = null;

                    if (property != null)
                    {
                        //perform action
                        switch (postContract.Action)
                        {
                            case Constants.Enums.UpdateAction.Increment:
                                //convert value from object to int
                                int.TryParse(property.GetValue(post).ToString(), out val);
                                if (postContract.UpdateValue != null)
                                {
                                    int.TryParse(postContract.UpdateValue.ToString(), out udpateValue);
                                    val = val + udpateValue;
                                    resultValue = val;
                                }
                                break;
                            case Constants.Enums.UpdateAction.Decrement:
                                //convert value from object to int
                                int.TryParse(property.GetValue(post).ToString(), out val);
                                if (postContract.UpdateValue != null)
                                {
                                    int.TryParse(postContract.UpdateValue.ToString(), out udpateValue);
                                    val = val - udpateValue;
                                    resultValue = val;
                                }
                                break;
                            case Constants.Enums.UpdateAction.Value:
                                resultValue = Convert.ChangeType(postContract.UpdateValue, property.GetType());
                                break;
                        }

                        property.SetValue(post, resultValue);
                        var result = await UpdatePost(id, userId, post);
                         return HttpHelper.ReturnObject(result);
                    }
                    else
                    {
                        ErrorMessages.genericValue = postContract.PropertyName;
                        return HttpHelper.ThrowError(HttpStatusCode.MethodNotAllowed, ErrorMessages.Application.PropertyNotDefined);
                    }
        }
    }
}
