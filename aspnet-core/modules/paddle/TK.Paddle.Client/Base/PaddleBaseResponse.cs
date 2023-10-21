using Newtonsoft.Json;

namespace TK.Paddle.Client.Base
{
    public class PaddleBaseResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public PaddleErrorDto Error { get; set; }
    }
}
