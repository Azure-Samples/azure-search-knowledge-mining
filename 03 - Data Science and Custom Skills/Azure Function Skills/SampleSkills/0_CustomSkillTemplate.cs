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
 * 3. Define what action your skill will take to enrich/transform the input into the output.
 *    Modify the DoWork method.  
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
        private class InputRecord
        {
            public class InputRecordData
            {
                public string MyInputField;
            }

            public string RecordId { get; set; }
            public InputRecordData Data { get; set; }
        }

        private class WebApiRequest
        {
            public List<InputRecord> Values { get; set; }
        }
        #endregion

        #region Classes used to serialize the response
        private class OutputRecord
        {
            public class OutputRecordData
            {
                public string MyOutputField { get; set; }
            }

            public class OutputRecordMessage
            {
                public string Message { get; set; }
            }

            public string RecordId { get; set; }
            public OutputRecordData Data { get; set; }
            public List<OutputRecordMessage> Errors { get; set; }
            public List<OutputRecordMessage> Warnings { get; set; }
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
            foreach (var record in data.Values)
            {
                if (record == null || record.RecordId == null) continue;

                OutputRecord responseRecord = new OutputRecord();
                responseRecord.RecordId = record.RecordId;

                try
                {
                    responseRecord.Data = DoWork(record.Data).Result;
                }
                catch (Exception e)
                {
                    // Something bad happened, log the issue.
                    var error = new OutputRecord.OutputRecordMessage
                    {
                        Message = e.Message
                    };

                    responseRecord.Errors = new List<OutputRecord.OutputRecordMessage>
                    {
                        error
                    };
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
            return data;
        }

        /// <summary>
        /// Replace this method with a method that enriches or transforms the data in
        /// a meaningful manner
        /// </summary>
        /// <param name="inputField">Replace this with any fields you need to process.</param>
        /// <returns>Feel free to change the return type to meet your needs. </returns>
        private static async  Task<OutputRecord.OutputRecordData> DoWork(InputRecord.InputRecordData inputRecord)
        {
            var outputRecord = new OutputRecord.OutputRecordData();
            outputRecord.MyOutputField = "Hello " + inputRecord.MyInputField;

            return outputRecord;
        }
    }
}
