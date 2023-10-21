using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Subscription.Dto
{
    public class PaddleSubscriptionPlanDto
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("billing_type")]
        public string BillingType { get; set; }

        [JsonProperty("billing_period")]
        public int BillingPeriod { get; set; }

        [JsonProperty("initial_price")]
        public Dictionary<string, string> InitialPrice { get; set; }

        [JsonProperty("recurring_price")]
        public Dictionary<string, string> RecuringPrice { get; set; }

        [JsonProperty("trial_days")]
        public int TrialDays { get; set; }
    }
}
