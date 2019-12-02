using System.Threading.Tasks;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Services.Interfaces
{
    public interface ILikeService
    {
        ResponseObjectModel GetLikesByType(Constants.Enums.LikeTypes type, string typeId);
        ResponseObjectModel GetLikesByUser(string resourceId);
        Task<ResponseObjectModel> GetLikes(LikeListContract contract);
        Task<ResponseObjectModel> GetLikeDetail(string id, string typeId, string environment);
        Task<ResponseObjectModel> Like(LikeContract contract, string userId);
        Task<ResponseObjectModel> Unlike(LikeContract contract, string userId);
        Task<bool> IsLiked(string typeId, string resourceId);
    }
}
