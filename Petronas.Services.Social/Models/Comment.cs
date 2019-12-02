using Newtonsoft.Json;

namespace Petronas.Services.Social.Models
{
    public class Comment : BaseModel
    {
        public string Environment { get; set; }
        [JsonProperty("partitionKey")]
        public new string PartitionKey
        {
            get
            {
                return Environment + "-" + ParentId;
            }
        }
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public string PostId { get; set; }
        public string ParentId { get; set; }
        public string Content { get; set; }
        public int Point { get; set; }
        public string ResourceId { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
    }
}
