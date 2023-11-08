using Newtonsoft.Json;

namespace TK.CoinGecko.Client.CoinGecko.Dto
{
    public class CoinGeckoCoinDetailDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("asset_platform_id")]
        public string AssetPlatformId { get; set; }

        [JsonProperty("platforms")]
        public Dictionary<string, string> Platforms { get; set; }

        [JsonProperty("block_time_in_minutes")]
        public int? BlockTimeInMinutes { get; set; }

        [JsonProperty("hashing_algorithm")]
        public object HashingAlgorithm { get; set; }

        [JsonProperty("categories")]
        public List<string> Categories { get; set; }

        [JsonProperty("public_notice")]
        public string PublicNotice { get; set; }

        [JsonProperty("additional_notices")]
        public List<object> AdditionalNotices { get; set; }

        [JsonProperty("links")]
        public CoinGeckoLink Links { get; set; }

        [JsonProperty("image")]
        public CoinGeckoImage Image { get; set; }

        [JsonProperty("country_origin")]
        public string CountryOrigin { get; set; }

        [JsonProperty("genesis_date")]
        public object GenesisDate { get; set; }

        [JsonProperty("contract_address")]
        public string ContractAddress { get; set; }

        [JsonProperty("sentiment_votes_up_percentage")]
        public double? SentimentVotesUpPercentage { get; set; }

        [JsonProperty("sentiment_votes_down_percentage")]
        public double? SentimentVotesDownPercentage { get; set; }

        [JsonProperty("market_cap_rank")]
        public int? MarketCapRank { get; set; }

        [JsonProperty("coingecko_rank")]
        public int? CoingeckoRank { get; set; }

        [JsonProperty("coingecko_score")]
        public double? CoingeckoScore { get; set; }

        [JsonProperty("developer_score")]
        public double? DeveloperScore { get; set; }

        [JsonProperty("community_score")]
        public double? CommunityScore { get; set; }

        [JsonProperty("liquidity_score")]
        public double? LiquidityScore { get; set; }

        [JsonProperty("public_interest_score")]
        public double? PublicInterestScore { get; set; }

        [JsonProperty("community_data")]
        public CoinGeckoCommunityData CommunityData { get; set; }

        [JsonProperty("public_interest_stats")]
        public Dictionary<string, object> PublicInterestStats { get; set; }

        [JsonProperty("status_updates")]
        public List<object> StatusUpdates { get; set; }

        [JsonProperty("last_updated")]
        public DateTime? LastUpdated { get; set; }
    }
}