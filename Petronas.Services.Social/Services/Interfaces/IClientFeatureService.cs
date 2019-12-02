using System.Threading.Tasks;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Services.Interfaces
{
    public interface IClientFeatureService
    {
        ResponseObjectModel GetAllClientFeatures();
        Task<ResponseObjectModel> GetClientFeatureList(ClientFeatureListContract contract);
        Task<ResponseObjectModel> GetClientFeature(string id, string applicationId, string environment);
        Task<ResponseObjectModel> GetClientFeature(string applicationId, string clientId, Constants.Enums.ClientFeature feature, string environment);
        Task<ResponseObjectModel> AddClientFeature(ClientFeatureContract contract, string userId);
        Task<ResponseObjectModel> UpdateClientFeature(string id, ClientFeatureContract contract, string userId);
        Task<ResponseObjectModel> DeleteClientFeature(string id, string applicationId, string userId, string environment);
        Task<bool> IsDuplicate(Constants.Enums.ClientFeature feature, string applicationId, string environment, string clientId);
    }
}
