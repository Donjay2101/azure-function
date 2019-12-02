using System.Threading.Tasks;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Services.Interfaces
{
    public interface IClientService
    {
        ResponseObjectModel GetAllClients();
        Task<ResponseObjectModel> GetClientList(ClientListContract contract);
        Task<ResponseObjectModel> GetClient(string id, string applicationId, string environment);
        Task<ResponseObjectModel> AddClient(ClientContract contract, string userId);
        Task<ResponseObjectModel> UpdateClient(string id, ClientContract contract, string userId);
        Task<ResponseObjectModel> DeleteClient(string id, string applicationId, string userId, string environment);
        Task<bool> IsExisting(string id, string applicationId, string environment);
        Task<bool> IsDuplicate(string name, string applicationId, string environment);
    }
}
