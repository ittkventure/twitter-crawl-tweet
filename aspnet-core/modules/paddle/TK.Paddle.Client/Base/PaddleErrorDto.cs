using Newtonsoft.Json;

namespace TK.Paddle.Client.Base
{
    public class PaddleErrorDto
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
