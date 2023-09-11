namespace TK.TwitterAccount.Domain.Clients
{
    public class TwitterClient
    {
        protected HttpClient Client { get; set; }

        public TwitterClient()
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri("https://api.twitter.com");
            Client.Timeout = TimeSpan.FromSeconds(60);
        }
    }
}
