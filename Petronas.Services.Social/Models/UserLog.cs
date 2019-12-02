using System;
using Newtonsoft.Json;

namespace Petronas.Services.Social.Models
{
    public class UserLog : BaseModel
    {
        public string Environment { get; set; }
        [JsonProperty("partitionKey")]
        public new string PartitionKey
        {
            get
            {
                return Environment + "-" + ResourceId;
            }
        }
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public string ResourceId { get; set; }
        public string Activity { get; set; }
        public DateTime Timestamp { get; set; }
        public int Point { get; set; }
    }
}
