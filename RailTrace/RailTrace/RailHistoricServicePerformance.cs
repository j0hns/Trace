using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RailTrace
{
    public static class RailHistoricServicePerformance
    {
       

        

        public static ServiceMetricsResponse GetServiceMetrics(ServiceMetricsRequest serviceMetricsRequest)
        {
            return GetData<ServiceMetricsRequest, ServiceMetricsResponse>(ConfigurationManager.AppSettings["ServiceMetricsUrl"], serviceMetricsRequest);
        }

        public static ServiceDetailResponse GetServiceDetail(ServiceDetailRequest serviceDetailRequest)
        {
            return GetData<ServiceDetailRequest, ServiceDetailResponse>(ConfigurationManager.AppSettings["ServiceDetailUrl"] , serviceDetailRequest);
        }

        private static TResponse GetData<TRequest,TResponse>(string url,TRequest request)
        {
            var payloadJson = JsonConvert.SerializeObject(request);
            var user = ConfigurationManager.AppSettings["UserEmail"];
            var pwd = ConfigurationManager.AppSettings["UserPwd"];
            var authenticationHeaderContents = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{pwd}"));


            using (var webClient = new WebClient())
            {

                webClient.Headers["Content-Type"] = "application/json";
                webClient.Headers["Authorization"] = $"Basic {authenticationHeaderContents}";

                var data = webClient.UploadString(url, payloadJson);
                Debug.WriteLine(data);
                return JsonConvert.DeserializeObject<TResponse>(data);
            }
        }
        public class ServiceDetailRequest
        {
            [JsonProperty("rid")]
            public string Rid { get; set; }
        }

        public class ServiceDetailResponse
        {
            public class LocationInfo
            {

                [JsonProperty("location")]
                public string Location { get; set; }

              
                [JsonProperty("gbtt_ptd")]
                public string DepartureTime { get; set; }

                [JsonProperty("gbtt_pta")]
                public string ArrivalTime { get; set; }

                [JsonProperty("actual_td")]
                public string ActualDepartureTime { get; set; }

                [JsonProperty("actual_ta")]
                public string ActualArrivalTime { get; set; }

                [JsonProperty("late_canc_reason")]
                public string Reason { get; set; }
            }

            public class ServiceAttributesDetailsInfo
            {
                [JsonProperty("date_of_service")]
                public string DateOfService { get; set; }

                [JsonProperty("toc_code")]
                public string TocCode { get; set; }

                [JsonProperty("rid")]
                public string Rid { get; set; }

                [JsonProperty("locations")]
                public LocationInfo[] Locations { get; set; }
            }

            [JsonProperty("serviceAttributesDetails")]
            public ServiceAttributesDetailsInfo ServiceAttributesDetails { get; set; }
           
        }

        public class ServiceMetricsRequest
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


            [JsonProperty("toc_filter1")]
            public string toc_filter1 { get; set; }

            [JsonProperty("tolerance2")]
            public double tolerance2 { get; set; }
        }

        public class ServiceMetricsResponse
        {
            public class ResponseHeader
            {
                [JsonProperty("from_location")]
                public string FromLoc { get; set; }

                [JsonProperty("to_location")]
                public string ToLoc { get; set; }

            }

            public class ResponseService
            {
                public class ResponseServiceAttributesMetrics
                {
                    //    "origin_location": "BTN",
                    //    "destination_location": "VIC",
                    //    "gbtt_ptd": "0712",
                    //    "gbtt_pta": "0823",
                    //    "toc_code": "GX",
                    //    "matched_services": "22",
                    //    "rids": [ "201607013361753", "201607043443704", ... ]

                    [JsonProperty("origin_location")]
                    public string OriginLocation { get; set; }

                    [JsonProperty("destination_location")]
                    public string DestinationLocation { get; set; }

                    [JsonProperty("gbtt_ptd")]
                    public string DepartureTime { get; set; }

                    [JsonProperty("gbtt_pta")]
                    public string ArrivalTime { get; set; }

                    [JsonProperty("toc_code")]
                    public string TrainOperatorCode { get; set; }

                    [JsonProperty("matched_services")]
                    public string MatchedServices { get; set; }

                    [JsonProperty("rids")]
                    public string[] TrainJourneyIds { get; set; }


                }

                public class ResponseMetrics
                {
                    //    "Metrics": [
                    //{
                    //    "tolerance_value": "0",
                    //    "num_not_tolerance": "5",
                    //    "num_tolerance": "17",
                    //    "percent_tolerance": "77",
                    //    "global_tolerance": true
                    //}

                    [JsonProperty("tolerance_value")]
                    public string ToleranceValue { get; set; }

                    [JsonProperty("num_not_tolerance")]
                    public string TrainsOutsideToleranceValue { get; set; }

                    [JsonProperty("num_tolerance")]
                    public string TrainsInsideToleranceValue { get; set; }


                    [JsonProperty("percent_tolerance")]
                    public string PercentTolerance { get; set; }

                    [JsonProperty("global_tolerance")]
                    public bool GlobalTolerance { get; set; }
                }

                public ResponseServiceAttributesMetrics ServiceAttributesMetrics { get; set; }
                public ResponseMetrics[] Metrics { get; set; }

            
            }

           




            public ResponseHeader Header { get; set; }
            public ResponseService[] Services { get; set; }
        }

        
    }
}
