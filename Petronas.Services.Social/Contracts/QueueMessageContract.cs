using Newtonsoft.Json;

namespace Petronas.Services.Social.Contracts
{
    public class QueueMessageContract
    {
        public string Action { get; set; }
        public string Id { get; set; }
        public string Payloads { get; set; }
        public string UserId { get; set; }
        public string Environment { get; set; }
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class ApplicationQueueContract : QueueMessageContract { }

    public class EnvironmentQueueContract : QueueMessageContract { }

    public class ClientQueueContract : QueueMessageContract
    {
        public string ApplicationId { get; set; }
    }

    public class ClientFeatureQueueContract : QueueMessageContract
    {
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
    }

    public class PostQueueContract : QueueMessageContract
    {
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
    }

    public class LikeQueueContract : QueueMessageContract
    {
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public string PostId { get; set; }
        public Constants.Enums.LikeTypes Type { get; set; }
        public string TypeId { get; set; }
    }

    public class CommentQueueContract : QueueMessageContract
    {
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public string PostId { get; set; }
        public string ParentId { get; set; }
    }

    public class HashTagQueueContract : QueueMessageContract 
    {
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public string ParentId { get; set; }
        
    }
}
