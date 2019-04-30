/****************************************************************************************
 * 
 * This template should make it easy for you to create a custom skill as
 * it takes care of most of the boilerplate code for serialization and  deserialization.
 * 
 * STEPS:
 * 1. Define the input fields for your skill.
 *    Modify the InputRecordData class.
 * 
 * 2. Define the output fields for your skill.
 *    Modify the OutputRecordData class.
 *    
 * 3. Define what action your will will take to enrich/transform the input into the output.
 *    Modify the DoWorkHere class.
 *    
 ****************************************************************************************/

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


namespace SampleSkills
{

    public static class CustomSkillTemplate
    {

        #region Class used to deserialize the request
        public class InputRecord
        {
            public class InputRecordData
            {
                public string myInputField;
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
                public string myOutputField { get; set; }
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

        [FunctionName("CustomSkillTemplate")]
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
                    responseRecord.data = DoWork(record.data).Result;
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
        /// Replace this method with a method that enriches or transforms the data in
        /// a meaningful manner
        /// </summary>
        /// <param name="inputField">Replace this with any fields you need to process.</param>
        /// <returns>Feel free to change the return type to meet your needs. </returns>
        async static Task<OutputRecord.OutputRecordData> DoWork(InputRecord.InputRecordData inputRecord)
        {
            var outputRecord = new OutputRecord.OutputRecordData();
            outputRecord.myOutputField = "Hello " + inputRecord.myInputField;

            return outputRecord;
        }
    }
}
