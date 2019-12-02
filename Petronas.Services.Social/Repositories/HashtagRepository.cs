using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Models;
using Petronas.Services.Social.Repositories.Interfaces;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Repositories
{
    public class HashTagRepository : BaseRepository<HashTag>, IHashTagRepository
    {
        public HashTagRepository() : base(CollectionNames.Comments) { }
    }
}
