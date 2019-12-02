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
    public class ClientFeatureService : BaseService, IClientFeatureService
    {
        private readonly IClientFeatureRepository _clientFeatureRepository;
        private readonly IApplicationService _applicationService;
        private readonly IClientService _clientService;

        public ClientFeatureService(
            IClientFeatureRepository clientFeatureRepository,
            IApplicationService applicationService,
            IClientService clientService,
            IApplicationRepository applicationRepository) : base(applicationRepository)
        {
            _clientFeatureRepository = clientFeatureRepository;
            _applicationService = applicationService;
            _clientService = clientService;
        }

        public ResponseObjectModel GetAllClientFeatures()
        {
            try
            {
                var result = _clientFeatureRepository.GetAll(new FeedOptions
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

        public async Task<ResponseObjectModel> GetClientFeatureList(ClientFeatureListContract contract)
        {
            // Validate contract
            if (contract == null)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.ContractCannotBeNull);

            // Validate application id
            if (string.IsNullOrWhiteSpace(contract.ApplicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate page size
            if (contract.PageSize == 0 || contract.PageSize < -1)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.PageSizeIsInvalid);

            if (string.IsNullOrEmpty(contract.Environment) || string.IsNullOrWhiteSpace(contract.Environment))
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.EnvironmentRequired);

            try
            {
                var queryResult = await _clientFeatureRepository.GetList(
                    x => !x.IsDeleted,
                    DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ApplicationId),
                    contract.PageSize,
                    contract.ContinuationToken);
                var result = queryResult.GetType<ClientFeatureListModel>();

                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> GetClientFeature(string id, string applicationId, string environment)
        {
            // Validate client feature id
            if (string.IsNullOrWhiteSpace(id))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.ClientFeature.ClientFeatureIdRequired);

            // Validate application id
            if (string.IsNullOrWhiteSpace(applicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            if (string.IsNullOrWhiteSpace(environment) || string.IsNullOrEmpty(environment))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);

            try
            {
                var result = await _clientFeatureRepository.Get(id, DocumentHelper.GetPartitionKeyByEnvironment(environment, applicationId));
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> GetClientFeature(string applicationId, string clientId, Constants.Enums.ClientFeature feature, string environment)
        {
            // Validate application id
            if (string.IsNullOrWhiteSpace(applicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate client id
            if (string.IsNullOrWhiteSpace(clientId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);

            if (string.IsNullOrWhiteSpace(environment) || string.IsNullOrEmpty(environment))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);

            try
            {
                var result = await _clientFeatureRepository.Get(
                    x => x.ClientId == clientId && x.Feature == feature && !x.IsDeleted,
                    DocumentHelper.GetPartitionKeyByEnvironment(environment, applicationId));
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> AddClientFeature(ClientFeatureContract contract, string userId)
        {
            // Validate contract
            if (contract == null)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.ContractCannotBeNull);

            // Validate application id
            if (string.IsNullOrWhiteSpace(contract.ApplicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate client id
            if (string.IsNullOrWhiteSpace(contract.ClientId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);

            // Validate point
            if (contract.Point < 0)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.PointNotValid);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);

            if (string.IsNullOrWhiteSpace(contract.Environment) || string.IsNullOrEmpty(contract.Environment))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);

            try
            {

                if (await IsEnvironmentNotAllowed(contract.ApplicationId, contract.Environment))
                    return HttpHelper.ThrowError(HttpStatusCode.MethodNotAllowed, ErrorMessages.Application.ApplicationNotFoundOrNotAllowed);

                if (await IsDuplicate(contract.Feature, contract.ApplicationId, contract.Environment,contract.ClientId))
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.ClientFeature.ClientFeatureDuplicated);

                var isClientExist = await _clientService.IsExisting(contract.ClientId, contract.ApplicationId, contract.Environment);

                if (!isClientExist)
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Client.ClientNotFound);

                var clientFeature = AutoMapperConfig.MapObject<ClientFeatureContract, ClientFeature>(contract);
                clientFeature.Id = GeneralHelper.GenerateNewId();
                clientFeature.FeatureName = contract.Feature.ToString();
                clientFeature.CreatedOn = DateTime.UtcNow;
                clientFeature.CreatedBy = userId;
                var result = await _clientFeatureRepository.Add(clientFeature);
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> UpdateClientFeature(string id, ClientFeatureContract contract, string userId)
        {
            // Validate client feature id
            if (string.IsNullOrWhiteSpace(id))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.ClientFeature.ClientFeatureIdRequired);

            // Validate contract
            if (contract == null)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.ContractCannotBeNull);

            // Validate application id
            if (string.IsNullOrWhiteSpace(contract.ApplicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            if (string.IsNullOrWhiteSpace(contract.Environment) || string.IsNullOrEmpty(contract.Environment))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);

            // Validate point
            if (contract.Point < 0)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.PointNotValid);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);

            try
            {
                var feature = await _clientFeatureRepository.Get(
                    id, DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ApplicationId));

                if (feature == null)
                    HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.ClientFeature.ClientFeatureNotFound);

                if (feature.Feature != contract.Feature && await IsDuplicate(contract.Feature, contract.ApplicationId, contract.Environment, contract.ClientId))
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationDuplicated);

                feature.Feature = contract.Feature;
                feature.FeatureName = contract.Feature.ToString();

                var result = await _clientFeatureRepository.Update(id, feature, userId,
                    DocumentHelper.GetPartitionKeyByEnvironment(feature.Environment, feature.ApplicationId));
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> DeleteClientFeature(string id, string applicationId, string userId, string environment)
        {
            // Validate client feature id
            if (string.IsNullOrWhiteSpace(id))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.ClientFeature.ClientFeatureIdRequired);

            // Validate application id
            if (string.IsNullOrWhiteSpace(applicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);

            if (string.IsNullOrWhiteSpace(environment) || string.IsNullOrEmpty(environment))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);

            try
            {
                var result = await _clientFeatureRepository.Delete(id, userId,
                    DocumentHelper.GetPartitionKeyByEnvironment(environment, applicationId));
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<bool> IsDuplicate(Constants.Enums.ClientFeature feature, string applicationId, string environment,string clientId)
        {
            try
            {
                var result = await _clientFeatureRepository.Get(x => !x.IsDeleted && x.Feature == feature && x.ClientId == clientId,
                    DocumentHelper.GetPartitionKeyByEnvironment(environment, applicationId));
                return result != null ? true : false;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    return false;
                else
                    throw;
            }
        }
    }
}
