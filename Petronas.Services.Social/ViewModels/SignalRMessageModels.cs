namespace Petronas.Services.Social.ViewModels
{
    public class SignalRMessageModels<T> where T : class, new()
    {
        public string Action { get; set; }
        public T Payloads { get; set; }
        public string UserId { get; set; }
    }
}
