namespace Petronas.Services.Social.Contracts
{
    public class LikeContract
    {
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public string PostId { get; set; }
        public Constants.Enums.LikeTypes Type { get; set; }
        public string TypeId { get; set; }
        public string ResourceId { get; set; }
        public string ResourceName { get; set; }
        public string Environment { get; set; }
    }
}
