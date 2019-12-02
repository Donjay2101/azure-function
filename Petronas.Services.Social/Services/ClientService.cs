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
    public class ClientService : BaseService, IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IApplicationService _applicationService;

        public ClientService(
            IClientRepository clientRepository,
            IApplicationService applicationService,
            IApplicationRepository applicationRepository) : base(applicationRepository)
        {
            _clientRepository = clientRepository;
            _applicationService = applicationService;
        }

        public ResponseObjectModel GetAllClients()
        {
            try
            {
                var result = _clientRepository.GetAll(new FeedOptions
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

        public async Task<ResponseObjectModel> GetClientList(ClientListContract contract)
        {
            // Validate contract
            if (contract == null)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.ContractCannotBeNull);

            // Validate application id
            if (string.IsNullOrWhiteSpace(contract.ApplicationId) || string.IsNullOrEmpty(contract.ApplicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate page size
            if (contract.PageSize == 0 || contract.PageSize < -1)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.PageSizeIsInvalid);

            if (string.IsNullOrEmpty(contract.Environment) || string.IsNullOrWhiteSpace(contract.Environment))
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.EnvironmentRequired);

            try
            {
                var queryResult = await _clientRepository.GetList(
                    x => !x.IsDeleted,
                    DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ApplicationId),
                    contract.PageSize,
                    contract.ContinuationToken);
                var result = queryResult.GetType<ClientListModel>();

                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> GetClient(string id, string applicationId, string environment)
        {
            // Validate client id
            if (string.IsNullOrWhiteSpace(id))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);

            // Validate application id
            if (string.IsNullOrWhiteSpace(applicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            if (string.IsNullOrEmpty(environment) || string.IsNullOrWhiteSpace(environment))
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.EnvironmentRequired);

            try
            {
                var isAppExist = await _applicationService.IsExisting(applicationId);

                if (!isAppExist)
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Application.ApplicationNotFound);

                var client = await _clientRepository.Get(id,
                    DocumentHelper.GetPartitionKeyByEnvironment(environment, applicationId));

                return HttpHelper.ReturnObject(client);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> AddClient(ClientContract contract, string userId)
        {
            // Validate contract
            if (contract == null)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.ContractCannotBeNull);

            // Validate application id
            if (string.IsNullOrWhiteSpace(contract.ApplicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate client name
            if (string.IsNullOrWhiteSpace(contract.Name))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientNameRequired);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);

            if (string.IsNullOrWhiteSpace(contract.Environment) || string.IsNullOrEmpty(contract.Environment))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);

            try
            {
                if (await IsEnvironmentNotAllowed(contract.ApplicationId, contract.Environment))
                    return HttpHelper.ThrowError(HttpStatusCode.MethodNotAllowed, ErrorMessages.Application.ApplicationNotFoundOrNotAllowed);

                if (await IsDuplicate(contract.Name, contract.ApplicationId, contract.Environment))
                    throw new Exception(ErrorMessages.ClientDuplicated);

                var client = AutoMapperConfig.MapObject<ClientContract, Client>(contract);
                client.Id = GeneralHelper.GenerateNewId();
                client.CreatedOn = DateTime.UtcNow;
                client.CreatedBy = userId;
                var result = await _clientRepository.Add(client);
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> UpdateClient(string id, ClientContract contract, string userId)
        {
            // Validate client id
            if (string.IsNullOrWhiteSpace(id))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);

            // Validate contract
            if (contract == null)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.ContractCannotBeNull);

            // Validate application id
            if (string.IsNullOrWhiteSpace(contract.ApplicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate client name
            if (string.IsNullOrWhiteSpace(contract.Name))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientNameRequired);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);

            if (string.IsNullOrWhiteSpace(contract.Environment) || string.IsNullOrEmpty(contract.Environment))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);

            try
            {
                var client = await _clientRepository.Get(id,
                    DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ApplicationId));

                if (client == null)
                    throw new Exception(ErrorMessages.ClientNotFound);

                if (client.Name != contract.Name && await IsDuplicate(contract.Name, contract.ApplicationId, contract.Environment))
                    throw new Exception(ErrorMessages.ClientDuplicated);

                client.Name = contract.Name;
                client.Description = contract.Description;
                var result = await _clientRepository.Update(id, client, userId,
                    DocumentHelper.GetPartitionKeyByEnvironment(contract.Environment, contract.ApplicationId));
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> DeleteClient(string id, string applicationId, string userId, string environment)
        {
            // Validate client id
            if (string.IsNullOrWhiteSpace(id))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Client.ClientIdRequired);

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
                var result = await _clientRepository.Delete(id, userId,
                    DocumentHelper.GetPartitionKeyByEnvironment(environment, applicationId));
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<bool> IsExisting(string id, string applicationId, string environment)
        {
            try
            {
                var result = await _clientRepository.Get(id,
                    DocumentHelper.GetPartitionKeyByEnvironment(environment, applicationId));
                return result != null ? true : false;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> IsDuplicate(string name, string applicationId, string environment)
        {
            try
            {
                var result = await _clientRepository.Get(x => !x.IsDeleted && x.Name == name,
                    DocumentHelper.GetPartitionKeyByEnvironment(environment, applicationId));
                return result != null ? true : false;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
