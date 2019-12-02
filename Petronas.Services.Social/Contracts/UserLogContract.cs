using System;

namespace Petronas.Services.Social.Contracts
{
    public class UserLogContract
    {
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public string ResourceId { get; set; }
        public string Activity { get; set; }
        public DateTime Timestamp { get; set; }
        public int Point { get; set; }
    }
}
