namespace Petronas.Services.Social.Contracts
{
    public class PagedListContract
    {
        public int PageSize { get; set; } = 10;
        public string ContinuationToken { get; set; }
        public string SearchTerm { get; set; }
        public string Environment { get; set; }
    }

    public class ClientListContract : PagedListContract
    {
        public string ApplicationId { get; set; }
    }

    public class ClientFeatureListContract : PagedListContract
    {
        public string ApplicationId { get; set; }
    }

    public class PostListContract : PagedListContract
    {
        public string ClientId { get; set; }
    }

    public class LikeListContract : PagedListContract
    {
        public string TypeId { get; set; }
    }

    public class CommentListContract : PagedListContract
    {
        public string ParentId { get; set; }
    }


    public class HashTagListContract : PagedListContract
    {
        public string ParentId { get; set; }
    }

    public class UserLogListContract : PagedListContract
    {
        public string ResourceId { get; set; }
    }
}
