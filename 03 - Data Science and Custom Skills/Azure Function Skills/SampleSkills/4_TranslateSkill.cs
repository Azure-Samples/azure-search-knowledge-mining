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
using System.Text;
using System.Collections.Generic;

namespace SampleSkills
{
    public static class TranslateSkill
    {
        #region Translator Text API Credentials
        static string path = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";
        // NOTE: Replace this example key with a valid subscription key.
        static string translatorApiKey = "";
        #endregion

        #region Class used to deserialize the request
        public class InputRecord
        {
            public class InputRecordData
            {
                public string text;
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
                public string translatedText { get; set; }
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

        [FunctionName("TranslateSkill")]
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
                    responseRecord.data = TranslateText(record.data, "en").Result;
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
        /// Use Cognitive Service to translate text from one language to another.
        /// </summary>
        /// <param name="inputRecord">The input record that contains the original text to translate.</param>
        /// <param name="toLanguage">The language you want to translate to.</param>
        /// <returns>Asynchronous task that returns the translated text. </returns>
        async static Task<OutputRecord.OutputRecordData> TranslateText(InputRecord.InputRecordData inputRecord, string toLanguage)
        {
            string originalText = inputRecord.text;

            var outputRecord = new OutputRecord.OutputRecordData();

            System.Object[] body = new System.Object[] { new { Text = originalText } };
            var requestBody = JsonConvert.SerializeObject(body);

            var uri = $"{path}&to={toLanguage}";

            string result = "";

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", translatorApiKey);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                dynamic data = JsonConvert.DeserializeObject(responseBody);

                outputRecord.translatedText = data?.First?.translations?.First?.text?.Value as string;
                return outputRecord;
            }
        }
    }
}
