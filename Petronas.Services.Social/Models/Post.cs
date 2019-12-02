using System;
using Newtonsoft.Json;

namespace Petronas.Services.Social.Models
{
    public class Post : BaseModel
    {
        public string Environment { get; set; }
        [JsonProperty("partitionKey")]
        public new string PartitionKey
        {
            get
            {
                return Environment + "-" + ClientId;
            }
        }
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Content { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int Point { get; set; }
        public int TotalPoint { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public bool IsDraft { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedDate { get; set; }
        public int ViewCount { get; set; }
    }
}
