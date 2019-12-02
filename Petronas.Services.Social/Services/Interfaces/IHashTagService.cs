using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Models;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Services.Interfaces
{
    public interface IHashTagService
    {
        Task<ResponseObjectModel> GetAll(HashTagListContract contract);
        Task<ResponseObjectModel> GetHashTagById(string Id,string parentId, string environment);
        Task<ResponseObjectModel> GetHashTagByTagName(string tagName,string parentId, string environment);

        Task<ResponseObjectModel> Add(HashTagContract contract);
        Task<ResponseObjectModel> update(HashTagContract contract, string userId);

        Task<ResponseObjectModel> Delete(HashTagContract contract);
    }
}
