
namespace Petronas.Services.Social.Contracts
{
    public class HashTagContract
    {
        public string Id{get;set;}
        public string UserId{get;set;}
        public string ApplicationId{get;set;}
        public string ClientId{get;set;}
        public string Name{get;set;}
         public string Environment { get; set; }
         public string ParentId { get; set; }
    }
}