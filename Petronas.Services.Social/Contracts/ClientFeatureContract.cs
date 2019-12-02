namespace Petronas.Services.Social.Contracts
{
    public class ClientFeatureContract
    {
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public Constants.Enums.ClientFeature Feature { get; set; }
        public int Point { get; set; }
        public string Environment { get; set; }
    }
}
