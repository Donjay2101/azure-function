using Newtonsoft.Json;

namespace Petronas.Services.Social.Models
{
    public class Client : BaseModel
    {
        public string Environment { get; set; }
        [JsonProperty("partitionKey")]
        public string PartitionKey
        {
            get
            {
                return Environment + "-" + ApplicationId;
            }
        }

        public string ApplicationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
