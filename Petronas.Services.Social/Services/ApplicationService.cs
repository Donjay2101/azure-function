using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Petronas.Services.Social.Configurations;
using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Constants.Enums;
using Petronas.Services.Social.Helpers;
using Petronas.Services.Social.Models;
using Petronas.Services.Social.Repositories.Interfaces;
using Petronas.Services.Social.Services.Interfaces;
using Petronas.Services.Social.ViewModels;
using Environment = Petronas.Services.Social.Models.Environment;

namespace Petronas.Services.Social.Services
{
    public class ApplicationService : BaseService, IApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;

        public ApplicationService(IApplicationRepository applicationRepository) : base(applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        public async Task<ResponseObjectModel> GetApplicationList(PagedListContract contract)
        {
            // Validate contract
            if (contract == null)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.ContractCannotBeNull);

            // Validate page size
            if (contract.PageSize == 0 || contract.PageSize < -1)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.PageSizeIsInvalid);

            try
            {
                var queryResult = await _applicationRepository.GetList(
                    x => !x.IsDeleted,
                    null,
                    contract.PageSize,
                    contract.ContinuationToken);
                var result = queryResult.GetType<ApplicationListModel>();

                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> GetApplication(string id)
        {
            // Validate application ID.
            if (string.IsNullOrWhiteSpace(id))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            try
            {
                var result = await _applicationRepository.Get(id, id);
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> AddApplication(ApplicationContract contract, string userId)
        {
            // Validate contract
            if (contract == null)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.ContractCannotBeNull);

            // Validate application name
            if (string.IsNullOrWhiteSpace(contract.Name))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationNameIsRequired);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);
            try
            {
                // Check application name duplication
                if (await IsDuplicate(contract.Name))
                    return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationDuplicated);

                var application = AutoMapperConfig.MapObject<ApplicationContract, Application>(contract);
                application.Id = GeneralHelper.GenerateNewId();
                application.CreatedOn = DateTime.UtcNow;
                application.CreatedBy = userId;
                var result = await _applicationRepository.Add(application);
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> UpdateApplication(string id, ApplicationContract contract, string userId)
        {
            // Validate application ID.
            if (string.IsNullOrWhiteSpace(id))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate contract
            if (contract == null)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.ContractCannotBeNull);

            // Validate application name
            if (string.IsNullOrWhiteSpace(contract.Name))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationNameIsRequired);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);

            try
            {
                var objectModel = await GetApplication(id);
                var application = objectModel.Value as Application;

                if (application == null)
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Application.ApplicationNotFound);

                if (application.Name != contract.Name && await IsDuplicate(contract.Name))
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Application.ApplicationDuplicated);

                application.Name = contract.Name;
                application.Description = contract.Description;
                var result = await _applicationRepository.Update(id, application, userId, id);
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> DeleteApplication(string id, string userId)
        {
            // Validate application ID.
            if (string.IsNullOrWhiteSpace(id))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);

            try
            {
                var objectModel = await GetApplication(id);
                var application = objectModel.Value as Application;

                if (application == null)
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Application.ApplicationNotFound);


                var result = await _applicationRepository.Delete(id, userId, id);
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<bool> IsExisting(string id)
        {
            try
            {
                var result = await _applicationRepository.Get(id, id);
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

        public async Task<bool> IsDuplicate(string name)
        {
            try
            {
                var result = await _applicationRepository.Get(x => !x.IsDeleted && x.Name == name, null);
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

        #region Environment
        public async Task<ResponseObjectModel> AddAllowedEnvironment(string applicationId, EnvironmentType type, string userId)
        {
            // Validate application
            if (string.IsNullOrWhiteSpace(applicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);

            try
            {
                // Validate application's existence
                var application = await _applicationRepository.Get(applicationId, applicationId);
                if (application == null)
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Application.ApplicationNotFound);

                // Check if environment is duplicated
                var environmentDuplicated = application.AllowedEnvironments.Where(x => !x.IsDeleted && x.Type == type).Count() > 0;
                if (environmentDuplicated)
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Environment.EnvironmentDuplicated);

                application.AllowedEnvironments.Add(new Environment
                {
                    Name = type.ToString(),
                    Type = type,
                    Id = GeneralHelper.GenerateNewId(),
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId,
                });

                var result = await _applicationRepository.Update(applicationId, application, userId, applicationId);
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> AddAllowedEnvironments(EnvironmentContract contract, string userId)
        {
            // Validate contract
            if (contract == null)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.ContractCannotBeNull);

            // Validate application
            if (string.IsNullOrWhiteSpace(contract.ApplicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);

            if (contract.Environments == null || contract.Environments.Count() == 0)
            {
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
            }

            try
            {
                // Validate application's existence
                var application = await _applicationRepository.Get(contract.ApplicationId, contract.ApplicationId);
                if (application == null)
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Application.ApplicationNotFound);

                // Check if environment is duplicated
                Document result = null;
                foreach (var environment in contract.Environments)
                {
                    EnvironmentType environmentType;
                    Enum.TryParse<EnvironmentType>(environment,true,out environmentType);
                    if (!string.Equals(environmentType.ToString(),environment,StringComparison.InvariantCultureIgnoreCase))
                    {
                        ErrorMessages.genericValue = environment;
                        return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Environment.EnvironmentDoesnotExist);
                    }

                    var environmentDuplicated = application.AllowedEnvironments.Where(x => !x.IsDeleted && x.Type == environmentType).Count() > 0;
                    if (environmentDuplicated)
                        return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Environment.EnvironmentDuplicated);

                    application.AllowedEnvironments.Add(new Environment
                    {
                        Name = environmentType.ToString(),
                        Type = environmentType,
                        Id = GeneralHelper.GenerateNewId(),
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                    });
                }
                result = await _applicationRepository.Update(contract.ApplicationId, application, userId, contract.ApplicationId);
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> DeleteAllowedEnvironment(string applicationId, EnvironmentType type, string userId)
        {
            // Validate application
            if (string.IsNullOrWhiteSpace(applicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);

            try
            {
                // Validate application's existence
                var application = await _applicationRepository.Get(applicationId, applicationId);
                if (application == null)
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Application.ApplicationNotFound);

                // Check if environment is exist
                var environment = application.AllowedEnvironments.FirstOrDefault(x => !x.IsDeleted && x.Type == type);
                if (environment == null)
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Environment.EnvironmentNotFound);

                environment.IsDeleted = true;
                environment.DeletedBy = userId;
                environment.DeletedOn = DateTime.UtcNow;

                var result = await _applicationRepository.Update(applicationId, application, userId, applicationId);
                return HttpHelper.ReturnObject(result);
            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }

        public async Task<ResponseObjectModel> DeleteAllowedEnvironments(EnvironmentContract contract, string userId)
        {
            // Validate contract
            if (contract == null)
                return HttpHelper.ThrowError(HttpStatusCode.BadRequest, ErrorMessages.Common.ContractCannotBeNull);

            // Validate application
            if (string.IsNullOrWhiteSpace(contract.ApplicationId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Application.ApplicationIdIsRequired);

            // Validate user ID
            if (string.IsNullOrWhiteSpace(userId))
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.UserIdIsRequired);

            if (contract.Environments == null || contract.Environments.Count() == 0)
            {
                return HttpHelper.ThrowError(HttpStatusCode.NotAcceptable, ErrorMessages.Common.EnvironmentRequired);
            }

            try
            {
                // Validate application's existence
                var application = await _applicationRepository.Get(contract.ApplicationId, contract.ApplicationId);
                if (application == null)
                    return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Application.ApplicationNotFound);

                Document result = null;
                foreach (var environment in contract.Environments)
                {
                    EnvironmentType? environmentType = null;
                    environmentType = (EnvironmentType)Enum.Parse(typeof(EnvironmentType), environment, true);
                    // Check if environment is exist
                    var exitEnvironment = application.AllowedEnvironments.FirstOrDefault(x => !x.IsDeleted && x.Type == environmentType);
                    if (exitEnvironment == null)
                        return HttpHelper.ThrowError(HttpStatusCode.NotFound, ErrorMessages.Environment.EnvironmentNotFound);

                    exitEnvironment.IsDeleted = true;
                    exitEnvironment.DeletedBy = userId;
                    exitEnvironment.DeletedOn = DateTime.UtcNow;
                }

                result = await _applicationRepository.Update(contract.ApplicationId, application, userId, contract.ApplicationId);
                return HttpHelper.ReturnObject(result);

            }
            catch (DocumentClientException ex)
            {
                return HttpHelper.ThrowError(ex);
            }
        }
        #endregion
    }
}
