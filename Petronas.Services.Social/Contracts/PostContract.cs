namespace Petronas.Services.Social.Contracts
{
    public class PostContract
    {
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Content { get; set; }
        public string PropertyName{get;set;}
        public Constants.Enums.UpdateAction Action{get;set;}

        public object UpdateValue{get;set;}

        public string Environment { get; set; }
    }
}
