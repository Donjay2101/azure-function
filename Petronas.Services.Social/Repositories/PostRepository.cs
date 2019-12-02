using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Models;
using Petronas.Services.Social.Repositories.Interfaces;

namespace Petronas.Services.Social.Repositories
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository() : base(CollectionNames.Posts) { }
    }
}
