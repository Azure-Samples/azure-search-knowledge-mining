using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

#region Input-Output Documentation
/*****
 *  Object Detecion Custom Skill. 
 *  Important: Make sure to update credentials in the code. See credentials section.
 *  
 *  Sample input:
 *  
 *
         {
           "values": [
                {
                    "recordId": "foobar1",
                    "data":
                    {
                       "url":  "https://images.metmuseum.org/CRDImages/dp/web-large/DP803270.jpg"
                    }
                }
           ]
         }
 *  
 * 
 * Sample output:
 * 
         {
            "values": [
                {
                    "recordId": "foobar1",
                    "data": {
                        "objects": [
                            {
                                "object": "Wheel",
                                "confidence": "0.76"
                            }
                        ]
                    },
                    "errors": null,
                    "warnings": null
                }
            ]
        }

 * 
 * ****************/

#endregion

namespace SampleSkills
{
    public static class ObjectDetectionSkill
    {

        #region Credentials
        static string path = "https://westus.api.cognitive.microsoft.com/vision/v2.0/detect";
        // IMPORTANT: Enter your Cognitive Services key below.
        static string cognitiveServicesKey = "";
        #endregion

        #region Class used to deserialize the request
        public class InputRecord
        {
            public class InputRecordData
            {
                public string url;
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
            public class ComputerVisionObject
            {
                [JsonProperty(PropertyName = "object")]
                public string cvObject { get; set; } 
                public string confidence { get; set; }
            }

            public class OutputRecordData
            {
                public List<ComputerVisionObject> objects { get; set; }
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
            public List<OutputRecord> values { get; set; }
        }
        #endregion

        [FunctionName("ObjectDetectionSkill")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Object detection function: C# HTTP trigger function processed a request.");

            var response = new WebApiResponse();
            response.values = new List<OutputRecord>();

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            var data = JsonConvert.DeserializeObject<WebApiRequest>(requestBody);

            // Do some schema validation
            if (data == null)
            {
                return new BadRequestObjectResult("The request schema does not match expected schema.");
            }
            if (data.values == null)
            {
                return new BadRequestObjectResult("The request schema does not match expected schema. Could not find values array.");
            }

            // Calculate the response for each value.
            foreach (var record in data.values)
            {
                if (record == null || record.recordId == null) continue;

                OutputRecord responseRecord = new OutputRecord();
                responseRecord.recordId = record.recordId;

                try
                {
                    responseRecord.data = GetObjects(record.data.url).Result;
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

            return (ActionResult)new OkObjectResult(response);
        }


        /// <summary>
        /// Use Cognitive Services to find objects in an image
        /// </summary>
        /// <param name="imageUrl">The image to extract objects for.</param>
        /// <returns>Asynchronous task that returns objects identified in the image. </returns>
        async static Task<OutputRecord.OutputRecordData> GetObjects(string imageUrl)
        {
            var requestBody = "{ \"url\": \"" + imageUrl + "\" }";
            var uri = $"{path}";
            var result = new OutputRecord.OutputRecordData();

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", cognitiveServicesKey);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                result = JsonConvert.DeserializeObject<OutputRecord.OutputRecordData>(responseBody);
            }

            return result;
        }
    }
}
