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
    public class HashTagService : BaseService, IHashTagService
    {
        private readonly IHashTagRepository _hashTagRepository;
        private readonly SignalRService signalRService;

        public HashTagService(IHashTagRepository hashTagRepository,
                                IApplicationRepository applicationRepository) : base(applicationRepository)
        {
            _hashTagRepository = hashTagRepository;
            signalRService = new SignalRService(System.Environment.GetEnvironmentVariable(AppSettings.AzureSignalRConnectionString));
        }

        public async Task<ResponseObjectModel> Add(HashTagContract contract)
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
                var hashtag = AutoMapperConfig.MapObject<HashTagContract, HashTag>(contract);
                hashtag.Id = GeneralHelper.GenerateNewId();
                hashtag.CreatedOn = DateTime.UtcNow;
                hashtag.CreatedBy = contract.UserId;
                var result = await _hashTagRepository.Add(hashtag);
                await signalRService.SendAsync(HubNames.Comment, HubMethods.Add, result.ToString());
                return HttpHelper.ReturnObject(result);

            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }


        }

        public async Task<ResponseObjectModel> Delete(HashTagContract contract)
        {
            try
            {
                if (string.IsNullOrEmpty(contract.Id) || string.IsNullOrWhiteSpace(contract.Id))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.CommentIdRequired);
                }

                if (string.IsNullOrEmpty(contract.ApplicationId) || string.IsNullOrWhiteSpace(contract.ApplicationId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ParentIdRequired);
                }

                if (string.IsNullOrEmpty(contract.Environment) || string.IsNullOrWhiteSpace(contract.Environment))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                }

                var partitionKey = DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ApplicationId);
                var result = await _hashTagRepository.Delete(contract.Id, contract.UserId, partitionKey);
                await signalRService.SendAsync(HubNames.Comment, HubMethods.Delete, result.ToString());
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }

        }

        public async Task<ResponseObjectModel> GetAll(HashTagListContract contract)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contract.ParentId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ParentIdRequired);
                }


                var queryResult = await _hashTagRepository.GetList(
                   x => !x.IsDeleted,
                   DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ParentId),
                   contract.PageSize,
                   contract.ContinuationToken);

                var result = queryResult.GetType<HashTagListModel>();
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> GetHashTagById(string Id, string applicationId, string environment)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationId))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ParentIdRequired);
                }

                if (string.IsNullOrWhiteSpace(Id))
                {
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.HashTag.HashTagIdRequired);
                }
                var partitionKey = DocumentHelper.GetPartitionKeyByEnvironment(environment, applicationId);
                var result = await _hashTagRepository.Get(Id, partitionKey);
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> GetHashTagByTagName(string tagName, string parentId, string environment)
        {
            return await Task.Run(() =>
           {
               try
               {
                   if (string.IsNullOrWhiteSpace(parentId))
                   {
                       return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Comment.ParentIdRequired);
                   }

                   if (string.IsNullOrWhiteSpace(parentId))
                   {
                       return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.HashTag.HashTagNameRequired);
                   }

                   if (string.IsNullOrWhiteSpace(environment))
                   {
                       return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
                   }

                   var partitionKey = DocumentHelper.GetPartitionKeyByEnvironment(environment, parentId);
                   var result = _hashTagRepository.GetAll(
                                           x => !x.IsDeleted && x.ApplicationId == parentId && x.Tag.ToUpper() == tagName.ToUpper(),
                                           new FeedOptions
                                           {
                                               MaxItemCount = -1,
                                               PartitionKey = new PartitionKey(partitionKey)
                                           }).ToList();
                   return HttpHelper.ReturnObject(result);
               }
               catch (DocumentClientException ex)
               {
                   return HttpHelper.ThrowError(ex);
               }
           });
        }

        public async Task<ResponseObjectModel> update(HashTagContract contract, string userId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (contract == null)
                    {
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.ContractCannotBeNull);
                    }

                    var hashtag = AutoMapperConfig.MapObject<HashTagContract, HashTag>(contract);
                    var partitionKey = DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ApplicationId);
                    var result = _hashTagRepository.Update(contract.Id, hashtag, userId, partitionKey);
                    return HttpHelper.ReturnObject(result);
                }
                catch (DocumentClientException ex)
                {
                    return HttpHelper.ThrowError(ex);
                }
            });
        }
    }
}
