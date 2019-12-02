using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Models;
using Petronas.Services.Social.Repositories.Interfaces;

namespace Petronas.Services.Social.Repositories
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository() : base(CollectionNames.Comments) { }
    }
}
