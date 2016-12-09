using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgurSharp
{
    public class ImgurToken
    {
        [JsonProperty("access_token")]
        public string Access_token { get; set; }
        [JsonProperty("refresh_token")]
        public string Refresh_token { get; set; }
        [JsonProperty("expires_in")]
        public string Expires_in { get; set; }
        [JsonProperty("token_type")]
        public string Token_type { get; set; }
        [JsonProperty("account_username")]
        public string Account_username { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("account_id")]
        public string Account_id { get; set; }
    }
}
