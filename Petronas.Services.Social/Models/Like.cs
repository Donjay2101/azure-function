using Newtonsoft.Json;

namespace Petronas.Services.Social.Models
{
    public class Like : BaseModel
    {
        public string Environment { get; set; }
        [JsonProperty("partitionKey")]
        public new string PartitionKey
        {
            get
            {
                return Environment + "-" + TypeId;
            }
        }
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public string PostId { get; set; }
        public Constants.Enums.LikeTypes Type { get; set; }
        public string TypeId { get; set; }
        public int Point { get; set; }
        public string ResourceId { get; set; }
        public string ResourceName { get; set; }
    }
}
