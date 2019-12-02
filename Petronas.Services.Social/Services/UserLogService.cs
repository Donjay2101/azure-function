using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Petronas.Services.Social.Configurations;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Helpers;
using Petronas.Services.Social.Models;
using Petronas.Services.Social.Repositories.Interfaces;
using Petronas.Services.Social.Services.Interfaces;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Services
{
    public class UserLogService : IUserLogService
    {
        private readonly IUserLogRepository _userLogRepository;

        public UserLogService(IUserLogRepository userLogRepository)
        {
            _userLogRepository = userLogRepository;
        }

        public IEnumerable<UserLog> GetAllUserLogs()
        {
            var result = _userLogRepository.GetAll(new FeedOptions
            {
                MaxItemCount = -1,
                EnableCrossPartitionQuery = true
            }).ToList();
            return result;
        }

        public async Task<UserLogListModel> GetUserLogList(UserLogListContract contract)
        {
            var queryResult = await _userLogRepository.GetList(
                x => !x.IsDeleted,
                contract.ResourceId,
                contract.PageSize,
                contract.ContinuationToken);
            var result = queryResult.GetType<UserLogListModel>();

            return result;
        }

        public async Task<UserLog> GetUserLog(string id, string resourceId)
        {
            var result = await _userLogRepository.Get(id, resourceId);
            return result;
        }

        public async Task<Document> AddUserLog(UserLogContract contract, string userId)
        {
            var userLog = AutoMapperConfig.MapObject<UserLogContract, UserLog>(contract);
            userLog.Id = GeneralHelper.GenerateNewId();
            userLog.CreatedOn = DateTime.UtcNow;
            userLog.CreatedBy = userId;
            var result = await _userLogRepository.Add(userLog);
            return result;
        }

        public async Task<Document> UpdateUserLog(string id, UserLogContract contract, string userId)
        {
            var userLog = AutoMapperConfig.MapObject<UserLogContract, UserLog>(contract);
            var result = await _userLogRepository.Update(id, userLog, userId, contract.ResourceId);
            return result;
        }

        public async Task<Document> DeleteUserLog(string id, string resourceId, string userId)
        {
            var result = await _userLogRepository.Delete(id, userId, resourceId);
            return result;
        }
    }
}
