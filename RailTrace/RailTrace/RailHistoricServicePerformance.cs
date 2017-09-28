using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RailTrace
{
    public class RailHistoricServicePerformance
    {
        private string url = "https://hsp-prod.rockshore.net/";




        private T GetData<T,TPAYLOAD>(TPAYLOAD payload)
        {
            var payloadJson= JsonConvert.SerializeObject(payload);
            using (var webClient=new WebClient())
            {
                webClient.Headers["Content-Type"] = "application/json";
                //Authorization: Basic {Base64("email:password")}
                var data =webClient.UploadString(url, payloadJson);
                return JsonConvert.DeserializeObject<T>(data);
            }
        }

        public class ServiceMetrics
        {
            [JsonProperty("from_loc")]
            public string FromLoc { get; set; }

            [JsonProperty("to_loc")]
            public string ToLoc { get; set; }

            [JsonProperty("from_time")]
            public string FromTime { get; set; }

            [JsonProperty("to_time")]
            public string ToTime { get; set; }

            [JsonProperty("from_date")]
            public string FromDate  { get; set; }

            [JsonProperty("to_date")]
            public string ToDate { get; set; }

            [JsonProperty("days")]
            public string Days { get; set; }
        }

    }
}
