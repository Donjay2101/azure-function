using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Models;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Services.Interfaces
{
    public interface IUserLogService
    {
        IEnumerable<UserLog> GetAllUserLogs();
        Task<UserLogListModel> GetUserLogList(UserLogListContract contract);
        Task<UserLog> GetUserLog(string id, string resourceId);
        Task<Document> AddUserLog(UserLogContract contract, string userId);
        Task<Document> UpdateUserLog(string id, UserLogContract contract, string userId);
        Task<Document> DeleteUserLog(string id, string resourceId, string userId);
    }
}
