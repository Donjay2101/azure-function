using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Petronas.Services.Social.Models
{
    public class Application : BaseModel
    {
        public Application()
        {
            AllowedEnvironments = new List<Environment>();
        }

        [JsonProperty("partitionKey")]
        public string PartitionKey
        {
            get
            {
                return Id;
            }
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Environment> AllowedEnvironments { get; set; }
    }
}
