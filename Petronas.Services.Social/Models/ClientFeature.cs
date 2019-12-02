using Newtonsoft.Json;

namespace Petronas.Services.Social.Models
{
    public class ClientFeature : BaseModel
    {
        public string Environment { get; set; }
        [JsonProperty("partitionKey")]
        public new string PartitionKey
        {
            get
            {
                return Environment + "-" + ApplicationId;
            }
        }
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public Constants.Enums.ClientFeature Feature { get; set; }
        public string FeatureName { get; set; }
        public int Point { get; set; }
    }
}
