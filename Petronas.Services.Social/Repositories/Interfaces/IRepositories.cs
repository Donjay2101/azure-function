using Petronas.Services.Social.Models;

namespace Petronas.Services.Social.Repositories.Interfaces
{
    public interface IApplicationRepository : IBaseRepository<Application> { }
    public interface IClientFeatureRepository : IBaseRepository<ClientFeature> { }
    public interface IClientRepository : IBaseRepository<Client> { }
    public interface IPostRepository : IBaseRepository<Post> { }
    public interface ICommentRepository : IBaseRepository<Comment> { }
    public interface ILikeRepository : IBaseRepository<Like> { }
    public interface IUserLogRepository : IBaseRepository<UserLog> { }

    public interface IHashTagRepository : IBaseRepository<HashTag> { }
}
