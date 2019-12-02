using System.Threading.Tasks;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Services.Interfaces
{
    public interface ICommentService
    {
        ResponseObjectModel GetComments(string parentId);
        ResponseObjectModel GetUserComments(string resourceId);
        Task<ResponseObjectModel> GetCommentList(CommentListContract contract);
        Task<ResponseObjectModel>  GetComment(string id, string parentId, string environment);
        Task<ResponseObjectModel> AddComment(CommentContract contract, string userId);
        Task<ResponseObjectModel> UpdateComment(string id, CommentContract contract, string userId);
        Task<ResponseObjectModel> DeleteComment(string id, string parentId, string userId,string environment);
    }
}
