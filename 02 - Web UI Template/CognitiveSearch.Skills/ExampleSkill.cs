// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Text;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web;

namespace CognitiveSearch.Skills
{
    public static class ExampleSkill
    {
        [FunctionName("ExampleSkill")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            string recordId = null;        
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Validation
            if (data?.values == null)
            {
                return new BadRequestObjectResult(" Could not find values array");
            }
            if (data?.values.HasValues == false || data?.values.First.HasValues == false)
            {
                // It could not find a record, then return empty values array.
                return new BadRequestObjectResult(" Could not find valid records in values array");
            }

            recordId = data?.values?.First?.recordId?.Value as string;
            // Parse your inputs here
            // input = data?.values ...

            try
            {
                // Do your custom skill work here
                // Ex: Make custom model calls, do lookup in external source, etc

                // Put together response.
                WebApiResponseRecord responseRecord = new WebApiResponseRecord();
                responseRecord.data = new Dictionary<string, object>();
                responseRecord.recordId = recordId;

                // Add your custom output values
                // responseRecord.data.Add("value", value);

                log.Info(JsonConvert.SerializeObject(responseRecord.data));

                WebApiEnricherResponse response = new WebApiEnricherResponse();
                response.values = new List<WebApiResponseRecord>();
                response.values.Add(responseRecord);

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                log.Info(ex.StackTrace);

                WebApiResponseRecord responseRecord = new WebApiResponseRecord();
                responseRecord.data = new Dictionary<string, object>();
                responseRecord.recordId = recordId;

                log.Info(JsonConvert.SerializeObject(responseRecord.data));

                WebApiEnricherResponse response = new WebApiEnricherResponse();
                response.values = new List<WebApiResponseRecord>();
                response.values.Add(responseRecord);

                return new OkObjectResult(response);
            }
        }
    }
}
