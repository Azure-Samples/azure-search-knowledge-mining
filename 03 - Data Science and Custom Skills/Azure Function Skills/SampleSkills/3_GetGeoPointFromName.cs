/***
 * This custom skill takes a string input that represents a location (city, country, address or point of interest) and 
 * returns a geo-point with the coordinates for that location.
 * 
 * Important: Please enter your credentials to the Azure Maps API in the credentials section.
 * 
 * Sample Input:
     {
	    "values": 
	    [
    	    {
        	    "recordId": "foo1",
          	    "data": { "address":  "Guatemala City"}
            },
            {
        	    "recordId": "bar2",
          	    "data": { "address":  "20019 8th Dr SE, Bothell WA, 98012"}
            }
        ]
    }

   Sample Output:

   {
    "values": [
        {
            "recordId": "foo1",
            "data": {
                "mainGeoPoint": {
                    "type": "Point",
                    "coordinates": [
                        -90.51557,
                        14.60043
                    ]
                },
                "results": [
                    {
                        "type": "POI",
                        "score": "4.203",
                        "position": {
                            "lat": "14.60043",
                            "lon": "-90.51557"
                        }
                    },
                    {
                        "type": "POI",
                        "score": "4.048",
                        "position": {
                            "lat": "10.3132",
                            "lon": "-85.7697"
                        }
                    },
                    ...
                ]
            },
            "errors": null,
            "warnings": null
        },
        ...
    ]
}

 * 
 * 
 * 
 * ***/

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;

namespace SampleSkills
{
    public static class GetGeoPointFromName
    {
        #region Credentials
        static string azureMapstUri = "https://atlas.microsoft.com/search/fuzzy/json";        
        static string azureMapsKey = "";   // NOTE: Enter a valid subscription key.
        #endregion

        #region Class used to deserialize the request
        public class InputRecord
        {
            public class InputRecordData
            {
                public string address;
            }

            public string recordId { get; set; }
            public InputRecordData data { get; set; }
        }

        private class WebApiRequest
        {
            public List<InputRecord> values { get; set; }
        }
        #endregion

        #region Classes used to serialize the response
        public class OutputRecord
        {
            public class Position
            {
                public string lat;
                public string lon;
            }

            public class EdmGeographPoint
            {
                public EdmGeographPoint(double lat, double lon)
                {
                    type = "Point";
                    coordinates = new double[2];
                    coordinates[0] = lon;
                    coordinates[1] = lat;
                }

                public string type;
                public double[] coordinates { get; set; }
            }

            public class Geography
            {
                public string type { get; set; } 
                public string score { get; set; }
                public Position position { get; set; }
            }

            public class OutputRecordData
            {
                public List<Geography> results { get; set; }
                public EdmGeographPoint mainGeoPoint { get; set; }
            }

            public class OutputRecordErrors
            {
                public string message { get; set; }
            }

            public class OutputRecordWarnings
            {
                public string message { get; set; }
            }

            public string recordId { get; set; }
            public OutputRecordData data { get; set; }
            public List<OutputRecordErrors> errors { get; set; }
            public List<OutputRecordWarnings> warnings { get; set; }
        }

        private class WebApiResponse
        {
            public WebApiResponse()
            {
                this.values = new List<OutputRecord>();
            }

            public List<OutputRecord> values { get; set; }
        }
        #endregion

        [FunctionName("GetGeoPointFromName")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Custom skill: C# HTTP trigger function processed a request.");

            // Read input, deserialize it and validate it.
            var data = GetStructuredInput(req.Body);
            if (data == null)
            {
                return new BadRequestObjectResult("The request schema does not match expected schema.");
            }

            // Calculate the response for each value.
            var response = new WebApiResponse();
            foreach (var record in data.values)
            {
                if (record == null || record.recordId == null) continue;

                OutputRecord responseRecord = new OutputRecord();
                responseRecord.recordId = record.recordId;

                try
                {
                    responseRecord.data = GetPosition(record.data).Result;

                    
                    if (responseRecord.data != null && responseRecord.data.results != null && responseRecord.data.results.Count > 0)
                    {

                        var firstPoint = responseRecord.data.results[0];

                        if (firstPoint.position != null)
                        {
                            responseRecord.data.mainGeoPoint = new OutputRecord.EdmGeographPoint(
                                Convert.ToDouble(firstPoint.position.lat), 
                                Convert.ToDouble(firstPoint.position.lon));
                        }
                    }

                }
                catch (Exception e)
                {
                    // Something bad happened, log the issue.
                    var error = new OutputRecord.OutputRecordErrors
                    {
                        message = e.Message
                    };

                    responseRecord.errors = new List<OutputRecord.OutputRecordErrors>();
                    responseRecord.errors.Add(error);
                }
                finally
                {
                    response.values.Add(responseRecord);
                }
            }

            return new OkObjectResult(response);
        }

        private static WebApiRequest GetStructuredInput(Stream requestBody)
        {
            string request = new StreamReader(requestBody).ReadToEnd();

            var data = JsonConvert.DeserializeObject<WebApiRequest>(request);
            if (data == null)
            {
                return null;
            }
            return data;
        }


        /// <summary>
        /// Use Azure Maps to find location of an address
        /// </summary>
        /// <param name="address">The address to search for.</param>
        /// <returns>Asynchronous task that returns objects identified in the image. </returns>
        async static Task<OutputRecord.OutputRecordData> GetPosition(InputRecord.InputRecordData inputRecord)
        {
            var result = new OutputRecord.OutputRecordData();

            var uri = azureMapstUri + "?api-version=1.0&query=" + inputRecord.address;

            try
            {
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(uri);
                    request.Headers.Add("X-ms-client-id", azureMapsKey);

                    var response = await client.SendAsync(request);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var data = JsonConvert.DeserializeObject<OutputRecord.OutputRecordData>(responseBody);

                    result = data;
                }
            }
            catch
            {
                result = new OutputRecord.OutputRecordData();
            }

            return result;
        }













    }
}
