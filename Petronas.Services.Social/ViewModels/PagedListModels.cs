using System.Collections.Generic;
using Petronas.Services.Social.Configurations;

namespace Petronas.Services.Social.ViewModels
{
    public class PagedListModel<T> where T : class, new()
    {
        public string ContinuationToken { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<T> Data { get; set; }

        public Derived GetType<Derived>() where Derived : class, new()
        {
            return AutoMapperConfig.MapObject<PagedListModel<T>, Derived>(this);
        }
    }

    public class ApplicationListModel : PagedListModel<Models.Application> { }

    public class ClientListModel : PagedListModel<Models.Client> { }

    public class ClientFeatureListModel : PagedListModel<Models.ClientFeature> { }

    public class PostListModel : PagedListModel<Models.Post> { }

    public class LikeListModel : PagedListModel<Models.Like> { }

    public class CommentListModel : PagedListModel<Models.Comment> { }

    public class UserLogListModel : PagedListModel<Models.UserLog> { }

    public class HashTagListModel : PagedListModel<Models.HashTag> { }
}
