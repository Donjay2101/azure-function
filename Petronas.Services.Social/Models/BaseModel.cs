using System;
using Newtonsoft.Json;

namespace Petronas.Services.Social.Models
{
    public class BaseModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
