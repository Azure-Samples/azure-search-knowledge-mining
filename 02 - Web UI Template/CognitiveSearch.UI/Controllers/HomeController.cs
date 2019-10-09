// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using CognitiveSearch.UI.Models;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using Microsoft.Azure;
using static CognitiveSearch.UI.Models.ClustomerEntity;

namespace CognitiveSearch.UI.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration { get; set; }
        private DocumentSearchClient _docSearch { get; set; }

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            _docSearch = new DocumentSearchClient(configuration);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(string q)
        {
            var searchidId = _docSearch.GetSearchId().ToString();

            if (searchidId != string.Empty)
                TempData["searchId"] = searchidId;

            TempData["query"] = q;
            TempData["applicationInstrumentationKey"] = _configuration.GetSection("InstrumentationKey")?.Value;


            return View();
        }

        public ActionResult CreateTable(string sText)
        {

            // The code in this section goes here.
            CloudStorageAccount storageAccount = new CloudStorageAccount(
            new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            "saramisstorage", "yErhAqOlVgL8VDhkAhsrQdzRnCHjQDx5FacWnPG2KhCEx/d3H/mo503Vbt1SJUCinYSWlXnoKIpXhTUsDusrng=="), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("TestTable");

            ViewBag.TableName = table.Name;

            async void CreateTestTableAsync()
            {
                // Create the CloudTable if it does not exist
                await table.CreateIfNotExistsAsync();
            }

            CreateTestTableAsync();

            // Create a customer entity and add it to the table.
            CustomerEntity customer1 = new CustomerEntity("1", "1");
            customer1.LastName = "Dorroh";
            customer1.FirstName = "Makayla";
            customer1.Email = "mbdorroh@crimson.ua.edu";

            if (sText == "")
            {
                customer1.HighlightedText = sText;
            }
            else
            {
                customer1.HighlightedText = "Somethings not right.";
            }

            TableOperation insertOperation = TableOperation.Insert(customer1);

            async void AddEntities()
            {
                await table.ExecuteAsync(insertOperation);
            }
            AddEntities();

            return RedirectToAction("CreateTable", "Home");
        }

        [HttpPost]
        public IActionResult GetDocuments(string q = "", SearchFacet[] searchFacets = null, int currentPage = 1)
        {
            var token = GetContainerSasUri();
            var selectFilter = _docSearch.Model.SelectFilter;

            if (!string.IsNullOrEmpty(q))
            {
                q = q.Replace("-", "").Replace("?", "");
            }

            var response = _docSearch.Search(q, searchFacets, selectFilter, currentPage);
            var searchId = _docSearch.GetSearchId().ToString();
            var facetResults = new List<object>();
            var tagsResults = new List<object>();

            if (response.Facets != null)
            {
                // Return only the selected facets from the Search Model
                foreach (var facetResult in response.Facets.Where(f => _docSearch.Model.Facets.Where(x => x.Name == f.Key).Any()))
                {
                    facetResults.Add(new
                    {
                        key = facetResult.Key,
                        value = facetResult.Value
                    });
                }

                foreach (var tagResult in response.Facets.Where(t => _docSearch.Model.Tags.Where(x => x.Name == t.Key).Any()))
                {
                    tagsResults.Add(new
                    {
                        key = tagResult.Key,
                        value = tagResult.Value
                    });
                }
            }

            return new JsonResult(new DocumentResult { Results = response.Results, Facets = facetResults, Tags = tagsResults, Count = Convert.ToInt32(response.Count), Token = token, SearchId = searchId });
        }

        [HttpPost]
        public IActionResult GetDocumentById(string id = "")
        {
            var token = GetContainerSasUri();

            var response = _docSearch.LookUp(id);
            var facetResults = new List<object>();

            return new JsonResult(new DocumentResult { Result = response, Token = token });
        }

        private string GetContainerSasUri()
        {
            string sasContainerToken;
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            string containerAddress = _configuration.GetSection("StorageContainerAddress")?.Value;
            CloudBlobContainer container = new CloudBlobContainer(new Uri(containerAddress), new StorageCredentials(accountName, accountKey));

            SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Read
            };

            sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            return sasContainerToken;
        }

        [HttpPost]
        public JObject GetGraphData(string query)
        {
            if (query == null)
            {
                query = "*";
            }
            FacetGraphGenerator graphGenerator = new FacetGraphGenerator(_docSearch);
            var graphJson = graphGenerator.GetFacetGraphNodes(query, "keyPhrases");

            return graphJson;
        }
    }
}
