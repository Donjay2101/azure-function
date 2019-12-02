using System;
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
    public class LikeService : BaseService, ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly SignalRService signalRService;

        public LikeService(
            ILikeRepository likeRepository,
            IApplicationRepository applicationRepository) : base(applicationRepository)
        {
            _likeRepository = likeRepository;
            signalRService = new SignalRService(System.Environment.GetEnvironmentVariable(AppSettings.AzureSignalRConnectionString));
        }

        public ResponseObjectModel GetLikesByType(Constants.Enums.LikeTypes type, string typeId)
        {
            try
            {
                if (string.IsNullOrEmpty(typeId) || string.IsNullOrWhiteSpace(typeId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Like.TypeIdRequired);
                }

                var post = _likeRepository.GetAll(
                                x => !x.IsDeleted && x.Type == type && x.TypeId == typeId,
                                new FeedOptions
                                {
                                    MaxItemCount = -1,
                                    EnableCrossPartitionQuery = true
                                });
                return HttpHelper.ReturnObject(post);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public ResponseObjectModel GetLikesByUser(string resourceId)
        {
            try
            {
                if (string.IsNullOrEmpty(resourceId) || string.IsNullOrWhiteSpace(resourceId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Like.ResourceIdRequired);
                }

                var post = _likeRepository.GetAll(
                x => !x.IsDeleted && x.ResourceId == resourceId,
                new FeedOptions
                {
                    MaxItemCount = -1,
                    EnableCrossPartitionQuery = true
                });
                return HttpHelper.ReturnObject(post);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }

        }

        public async Task<ResponseObjectModel> GetLikes(LikeListContract contract)
        {
            try
            {

                if (contract == null)
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.ContractCannotBeNull);
                }

                if (string.IsNullOrEmpty(contract.TypeId) || string.IsNullOrWhiteSpace(contract.TypeId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Like.TypeIdRequired);
                }

                if (string.IsNullOrEmpty(contract.Environment) || string.IsNullOrWhiteSpace(contract.Environment))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                }

                var queryResult = await _likeRepository.GetList(
                    x => !x.IsDeleted,
                    DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.TypeId),
                    contract.PageSize,
                    contract.ContinuationToken);
                var result = queryResult.GetType<LikeListModel>();

                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }

        }

        public async Task<ResponseObjectModel> GetLikeDetail(string id, string typeId, string environment)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Like.LikeIdRequired);
                }

                if (string.IsNullOrEmpty(typeId) || string.IsNullOrWhiteSpace(typeId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Like.TypeIdRequired);
                }

                if (string.IsNullOrEmpty(environment) || string.IsNullOrWhiteSpace(environment))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                }

                var partitionKey = DocumentHelper.GetPartitionKeyByEnvironment(environment, typeId);
                var like = await _likeRepository.Get(id, partitionKey);
                return HttpHelper.ReturnObject(like);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }

        }

        public async Task<ResponseObjectModel> Like(LikeContract contract, string userId)
        {
            try
            {
                if (contract == null)
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.ContractCannotBeNull);
                }

                if (string.IsNullOrEmpty(contract.ApplicationId) || string.IsNullOrWhiteSpace(contract.ApplicationId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);
                }

                if (string.IsNullOrEmpty(contract.ClientId) || string.IsNullOrWhiteSpace(contract.ClientId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);
                }

                if (string.IsNullOrEmpty(contract.TypeId) || string.IsNullOrWhiteSpace(contract.TypeId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Like.TypeIdRequired);
                }

                if (string.IsNullOrEmpty(contract.Environment) || string.IsNullOrWhiteSpace(contract.Environment))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                }
                var isEnvironmentNotAllowed = await IsEnvironmentNotAllowed(contract.ApplicationId, contract.Environment);

                if(isEnvironmentNotAllowed){
                    return HttpHelper.ThrowError(HttpStatusCode.MethodNotAllowed, ErrorMessages.Application.ApplicationNotFoundOrNotAllowed);
                }

                var like = AutoMapperConfig.MapObject<LikeContract, Like>(contract);
                like.Id = GeneralHelper.GenerateNewId();
                like.CreatedOn = DateTime.UtcNow;
                like.CreatedBy = userId;
                var result = await _likeRepository.Add(like);
                signalRService.SendAsync(HubNames.Like, HubMethods.Add, result.ToString());
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }

        }

        public async Task<ResponseObjectModel> Unlike(LikeContract contract, string userId)
        {
            try
            {

                if (contract == null)
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.ContractCannotBeNull);
                }

                if (string.IsNullOrEmpty(contract.ResourceId) || string.IsNullOrWhiteSpace(contract.ResourceId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Like.ResourceIdRequired);
                }

                if (string.IsNullOrEmpty(contract.TypeId) || string.IsNullOrWhiteSpace(contract.TypeId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Like.TypeIdRequired);
                }

                if (string.IsNullOrEmpty(contract.Environment) || string.IsNullOrWhiteSpace(contract.Environment))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                }

                var partitionKey = DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.TypeId);
                var like = await _likeRepository.Get(
                                x => !x.IsDeleted
                                    && x.Type == contract.Type
                                    && x.TypeId == contract.TypeId
                                    && x.ResourceId == contract.ResourceId,
                                    partitionKey
                                );
                like.IsDeleted = true;
                like.DeletedOn = DateTime.UtcNow;
                like.DeletedBy = userId;
                var result = await _likeRepository.Delete(like.Id.ToString(), userId, partitionKey);
                signalRService.SendAsync(HubNames.Like, HubMethods.Delete, result.ToString());
                return null;
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }

        }

        public async Task<bool> IsLiked(string typeId, string resourceId)
        {
            var result = await _likeRepository.Get(x => !x.IsDeleted && x.ResourceId == resourceId, typeId);
            return result != null ? true : false;
        }
    }
}
