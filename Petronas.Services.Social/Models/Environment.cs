using System;
using Newtonsoft.Json;
using Petronas.Services.Social.Constants.Enums;

namespace Petronas.Services.Social.Models
{
    public class Environment
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public EnvironmentType Type { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
    }
}
