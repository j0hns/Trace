using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RailTrace.Tests
{
    [TestClass]
    public class RailHistoricServicePerformanceTests
    {
        [TestMethod]
        public void GetData()
        {
            //{
            //    "from_loc":"BTN",
            //    "to_loc":"VIC",
            //    "from_time":"0700",
            //    "to_time":"0800",
            //    "from_date":"2016-07-01",
            //    "to_date":"2016-08-01",
            //    "days":"WEEKDAY"
            var serviceMetricsRequest = new RailHistoricServicePerformance.ServiceMetricsRequest()
            {
                Days = "WEEKDAY",
                FromDate = "2017-09-01",
                ToDate = "2017-09-29",
                FromTime = "0600",
                ToTime = "0900",
                FromLoc = "IPS",
                ToLoc = "SRA"
            };

            var request = RailHistoricServicePerformance.GetServiceMetrics(serviceMetricsRequest);


            var serviceDetails = new List<RailHistoricServicePerformance.ServiceDetailResponse>();
            foreach (var responseService in request.Services)
            {
                var rids = responseService.ServiceAttributesMetrics.TrainJourneyIds;
                foreach (var rid in rids)
                {


                    var sericeDetailResponse = RailHistoricServicePerformance.GetServiceDetail(
                        new RailHistoricServicePerformance.ServiceDetailRequest() {Rid = rid});

                    serviceDetails.Add(sericeDetailResponse);
                }

            }
        }
    }
}
