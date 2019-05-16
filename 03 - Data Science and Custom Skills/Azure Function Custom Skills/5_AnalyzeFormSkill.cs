/****************************************************************************************
 * FORMS ANALYSIS SKILL
 * 
 * This is an example that consumes an endpoint trained using the forms recognizer
 * cognitive service. Since each endpoint produced by the forms recognizer is heavily
 * dependent on the training data, you should use this example as a guideline. 
 *  
 * 
 * To use this template:
 * 1. Create a Forms Recognizer Resource. At the time this template was written, 
 *    Forms Recognizer was in a gated public preview. If you have not done so, you may need to
 *    request access (https://aka.ms/FormRecognizerRequestAccess).
 *    
 *    1.1 Update the FormsRecognizerEndpoint and FormsRecognizerKey in the credentials section.
 *  
 * 2. You will need to train a model with your forms. The model that was used for this example 
 *    was trained using sample data  at https://go.microsoft.com/fwlink/?linkid=2090451.
 *    
 *    2.1 After training (See Training section of https://docs.microsoft.com/en-us/azure/cognitive-services/form-recognizer/quickstarts/python-train-extract )
 *        you will need to update the ModelId in the credentials section.
 *        
 * 3. Now that you have a model, you may want to modify a few things with the code:
 *    3.1 Modify the skill to return the properties you care about. Take a look at the properties of the 
 *        OutputRecordData class and modify them accordingly.
 *    3.2 You may also modify any calls to the GetField method to match the keys you are interested 
 *        in returning as skill outputs.
 *    3.3 This example was written to deal with PDF files. If you are working different file types    
 *        ensure to change the content-type sent to the forms recognizer.
 *     
 *    
 * Sample Input:
 * 
    {
	  "values": 
	  [
    	{
          "recordId": "foo1",
          "data": { 
          		        "formUrl":  "https://pathToTheInvoice/Invoice_4.pdf",
          		        "formSasToken":  "?st=sasTokeThatWillBeGeneratedByCognitiveSearch"
          }
        }
      ]
    }
 *  
 * Sample Output:
 * 
   {
      "values": [
        {
            "recordId": "foo1",
            "data": {
                "address": "1111 8th st. Bellevue, WA 99501 ",
                "recipient": "Southridge Video 1060 Main St. Atlanta, GA 65024 "
            },
            "errors": null,
            "warnings": null
        }
      ]
    }
 *
 *
 * 4. Connecting the forms recognizer to your skillset
 *    This is a sample skillset definition for this example: 
 *
         {
            "name": "formsdemo",
            "description": "extract fields usign a pre trained form reconition model",
            "skills": [
                {
                    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
                    "name": "formrecognizer", 
                    "description": "Extracts fields from a form",
                    "uri": "https://sampleskills.azurewebsites.net/api/AnalyzeForm?code=nN36C9ET85Ipf4rq/9WJyT2hNmDV8aOTG84klvHI46WqyXJECfVOhQ==",
                    "httpMethod": "POST",
                    "timeout": "PT30S",
                    "context": "/document",
                    "batchSize": 1,
                    "inputs": [
                        {
                            "name": "formUrl",
                            "source": "/document/metadata_storage_path"
                        },
                        {
                            "name": "formSasToken",
                            "source": "/document/metadata_storage_sas_token"
                        }
                    ],
                    "outputs": [
                        {
                            "name": "address",
                            "targetName": "address"
                        },
                        {
                            "name": "recipient",
                            "targetName": "recipient"
                        }
                    ]
                }
            ]
        }
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
using System.Net;
using System.Net.Http.Headers;

namespace SampleSkills
{
    public static class FormsRecognizerSkill
    {
        #region Credentials
        static readonly string FormsRecognizerEndpoint = @"https://westus2.api.cognitive.microsoft.com";
        static readonly string FormsRecognizerKey = @"ENTER KEY HERE";
        static readonly string ModelId = "ENTER MODEL ID HERE";
        #endregion

        #region Class used to deserialize the request
        private class InputRecord
        {
            public class InputRecordData
            {
                public string formUrl;
                public string formSasToken;
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
                public string Address { get; set; }
                public string Recipient { get; set; }
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

        #region Classes used to interact with the Forms Recognizer Analyze API
        private class FormsRecognizerResponse
        {
            public string Status;
            public Page[] pages { get; set; }
        }

        private class Page
        {
            public int Number { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
            public int ClusterId { get; set; }

            public KeyValuePair[] KeyValuePairs { get; set; }

            public class KeyValuePair
            {
                public BoundedElement[] Key { get; set; }
                public BoundedElement[] Value { get; set; }
            }

            public class BoundedElement
            {
                public string Text { get; set; }
                public double[] BoundingBox { get; set; }

                public double confidence { get; set; }
            }
        }
        #endregion


        [FunctionName("AnalyzeForm")]
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
                    responseRecord.Data = AnalyzeForm(record.Data).Result;
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
        /// Use Cognitive Service to translate text from one language to another.
        /// </summary>
        /// <param name="inputRecord">The input record that contains the original text to translate.</param>
        /// <param name="toLanguage">The language you want to translate to.</param>
        /// <returns>Asynchronous task that returns the translated text. </returns>
        async static Task<OutputRecord.OutputRecordData> AnalyzeForm(InputRecord.InputRecordData inputRecord)
        {
            string base_url = FormsRecognizerEndpoint + @"/formrecognizer/v1.0-preview/custom";
            string fileUrl = inputRecord.formUrl;
            string sasToken = inputRecord.formSasToken;

            var outputRecord = new OutputRecord.OutputRecordData();
            byte[] bytes = null;

            using (WebClient client = new WebClient())
            {
                // Read the form to analyze
                bytes = client.DownloadData(fileUrl + sasToken);
            }

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                var url = base_url + "/models/" + ModelId + "/analyze";

                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(url);
                request.Content = new ByteArrayContent(bytes);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                request.Headers.Add("Ocp-Apim-Subscription-Key", FormsRecognizerKey);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic data = JsonConvert.DeserializeObject(responseBody);

                    var result = JsonConvert.DeserializeObject<FormsRecognizerResponse>(responseBody);

                    var addressValue = GetField(result, "Address", 0);
                    var invoiceFor = GetField(result, "Invoice For", 0);

                    outputRecord.Address = addressValue;
                    outputRecord.Recipient = invoiceFor;

                    return outputRecord;
                }
                else
                {
                    throw new SystemException(response.StatusCode.ToString() + ": " + response.ToString() + "\n " + responseBody);
                }
            }
        }

        /// <summary>
        /// Searches for a field in a given page and returns the concatenated results.
        /// </summary>
        /// <param name="response">the responsed from the forms recognizer service.</param>
        /// <param name="fieldName">The field to search for</param>
        /// <param name="pageNumber">The page where the field should appear</param>
        /// <returns></returns>
        private static string GetField(FormsRecognizerResponse response, string fieldName, int pageNumber)
        {
            // Find the Address in Page 0
            if (response.pages != null)
            {
                //Assume that a given field is in the first page.
                if (response.pages[pageNumber] != null)
                {
                    foreach (var pair in response.pages[pageNumber].KeyValuePairs)
                    {
                        foreach (var key in pair.Key)
                        {
                            /// You may want to have a different comparer here 
                            /// depending on your needs.
                            if (key.Text.Contains(fieldName))
                            {
                                // then concatenate the result;
                                StringBuilder sb = new StringBuilder();
                                foreach (var value in pair.Value)
                                {
                                    sb.Append(value.Text);
                                    // You could replace this for a newline depending on your scenario.
                                    sb.Append(" ");
                                }

                                return sb.ToString();
                            }
                        }
                    }
                }
            }

            // Could not find it in that page.
            return null;
        }
    }
}
