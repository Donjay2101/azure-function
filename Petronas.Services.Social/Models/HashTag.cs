using Newtonsoft.Json;

namespace Petronas.Services.Social.Models
{
    public class HashTag : BaseModel
    {
        public string ParentId{get;set;}
        public string ApplicationId{get;set;}
        public string ClientId{get;set;}
        public string Environment { get; set; }

        [JsonProperty("partitionKey")]
        public new string PartitionKey
        {
            get
            {
                return Environment + "-" + ApplicationId;
            }
        }
        [JsonProperty("Tag")]
        public string Tag { get; set; }
    }
}
