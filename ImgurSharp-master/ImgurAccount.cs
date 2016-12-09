using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgurSharp
{
    public class ImgurAccount
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("url")]
        public string URL { get; set; }
        [JsonProperty("bio")]
        public string Description { get; set; }
        [JsonProperty("reputation")]
        public int Reputation { get; set; }
        [JsonProperty("created")]
        public string Created { get; set; }
        [JsonProperty("pro_expiration")]
        public bool Pro_expiration { get; set; }
    }
}
