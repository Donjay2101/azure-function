using System.Threading.Tasks;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Models;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Services.Interfaces
{
    public interface IPostService
    {
        ResponseObjectModel GetAllPosts();
        Task<ResponseObjectModel> GetPostList(PostListContract contract);
        Task<ResponseObjectModel> GetPost(string id, string clientId, string environment);
        Task<ResponseObjectModel> AddPost(PostContract contract, string userId);
        Task<ResponseObjectModel> UpdatePost(string id, PostContract contract, string userId);
        Task<ResponseObjectModel> PublishPost(string id, string clientId, string userId,string environment);
        Task<ResponseObjectModel> DeletePost(string id, string clientId, string userId, string environment);
        Task<ResponseObjectModel> UpdatePost(string id, string userId, Post post );

        Task<ResponseObjectModel> UpdatePartial(string id, PostContract postContract,string userId);
        Task<bool> IsExisting(string id, string clientId);
    }
}
