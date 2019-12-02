namespace Petronas.Services.Social.Contracts
{
    public class CommentContract
    {
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public string PostId { get; set; }
        public string ParentId { get; set; }
        public string Content { get; set; }
        public int Point { get; set; }
        public string ResourceId { get; set; }
        public string ResourceName { get; set; }
        public string Environment { get; set; }
    }
}
