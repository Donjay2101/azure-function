using System.Threading.Tasks;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Constants.Enums;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Services.Interfaces
{
    public interface IApplicationService
    {
        Task<ResponseObjectModel> GetApplicationList(PagedListContract contract);
        Task<ResponseObjectModel> GetApplication(string id);
        Task<ResponseObjectModel> AddApplication(ApplicationContract contract, string userId);
        Task<ResponseObjectModel> UpdateApplication(string id, ApplicationContract contract, string userId);
        Task<ResponseObjectModel> DeleteApplication(string id, string userId);
        Task<bool> IsExisting(string id);
        Task<bool> IsDuplicate(string name);
        Task<ResponseObjectModel> AddAllowedEnvironment(string applicationId, EnvironmentType type, string userId);
        Task<ResponseObjectModel> DeleteAllowedEnvironment(string applicationId, EnvironmentType type, string userId);
        Task<ResponseObjectModel> AddAllowedEnvironments(EnvironmentContract contract, string userId);

        Task<ResponseObjectModel> DeleteAllowedEnvironments(EnvironmentContract contract, string userId);
    }
}
