using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Models;
using Petronas.Services.Social.Repositories.Interfaces;

namespace Petronas.Services.Social.Repositories
{
    public class ApplicationRepository : BaseRepository<Application>, IApplicationRepository
    {
        public ApplicationRepository() : base(CollectionNames.Applications) { }
    }
}
