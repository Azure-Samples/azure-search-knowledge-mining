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
using static CognitiveSearch.UI.Models.Annotations;
using static CognitiveSearch.UI.Models.Comments;
using static CognitiveSearch.UI.Models.DeletedAnnotations;
using static CognitiveSearch.UI.Models.DeletedComments;
using static CognitiveSearch.UI.Models.DocClassifications;
using static CognitiveSearch.UI.Models.Documents;
using static CognitiveSearch.UI.Models.EntityClassifications;
using static CognitiveSearch.UI.Models.TextClassifications;
using Microsoft.AspNetCore.Http;

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

        public IActionResult CreateTable(string sText, string id)
        {
            //get highlighted text from user
            string highlightedText = sText;

            //get document ID
            string docID = id;

            //used for annotation partition key, row key, and ID
            int annotationCounter = 1;

            //used for comment partition key, row key, and ID
            int commentCounter = 1;

            // connect to storage account
            CloudStorageAccount storageAccount = new CloudStorageAccount(
            new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            "saramisstorage", "yErhAqOlVgL8VDhkAhsrQdzRnCHjQDx5FacWnPG2KhCEx/d3H/mo503Vbt1SJUCinYSWlXnoKIpXhTUsDusrng=="), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable Documents = tableClient.GetTableReference("Documents");
            CloudTable Annotations = tableClient.GetTableReference("Annotations");
            CloudTable Comments = tableClient.GetTableReference("Comments");
            CloudTable TextClassifications = tableClient.GetTableReference("TextClassifications");
            CloudTable EntityClassifications = tableClient.GetTableReference("EntityClassifications");
            CloudTable DocClassifications = tableClient.GetTableReference("DocClassifications");
            CloudTable DeletedAnnotations = tableClient.GetTableReference("DeletedAnnotations");
            CloudTable DeletedComments = tableClient.GetTableReference("DeletedComments");

            ViewBag.DocumentTable = Documents.Name;
            ViewBag.AnnotationTable = Annotations.Name;
            ViewBag.CommentTable = Comments.Name;
            ViewBag.TextClassificationTable = TextClassifications.Name;
            ViewBag.EntityClassificationTable = EntityClassifications.Name;
            ViewBag.DocClassificationTable = DocClassifications.Name;
            ViewBag.DeletedAnnotationTable = DeletedAnnotations.Name;
            ViewBag.DeletedCommentTable = DeletedAnnotations.Name;

            async void CreateDocumentTableAsync()
            {
                // Create the CloudTable if it does not exist
                await Documents.CreateIfNotExistsAsync();
            }
            CreateDocumentTableAsync();

            async void CreateAnnotationTableAsync()
            {
                // Create the CloudTable if it does not exist
                await Annotations.CreateIfNotExistsAsync();
            }
            CreateAnnotationTableAsync();

            async void CreateCommentTableAsync()
            {
                // Create the CloudTable if it does not exist
                await Comments.CreateIfNotExistsAsync();
            }
            CreateCommentTableAsync();

            async void CreateTextClassificationTableAsync()
            {
                // Create the CloudTable if it does not exist
                await TextClassifications.CreateIfNotExistsAsync();
            }
            CreateTextClassificationTableAsync();

            async void CreateEntityClassificationTableAsync()
            {
                // Create the CloudTable if it does not exist
                await EntityClassifications.CreateIfNotExistsAsync();
            }
            CreateEntityClassificationTableAsync();

            async void CreateDocClassificationTableAsync()
            {
                // Create the CloudTable if it does not exist
                await DocClassifications.CreateIfNotExistsAsync();
            }
            CreateDocClassificationTableAsync();

            async void CreateDeletedAnnotationTableAsync()
            {
                // Create the CloudTable if it does not exist
                await DeletedAnnotations.CreateIfNotExistsAsync();
            }
            CreateDeletedAnnotationTableAsync();

            async void CreateDeletedCommentTableAsync()
            {
                // Create the CloudTable if it does not exist
                await DeletedComments.CreateIfNotExistsAsync();
            }
            CreateDeletedCommentTableAsync();

            //creating document classification list for dropdown list
            List<string> docClassificationList = new List<string>();
            TableContinuationToken token = null;
            do
            {
                var q = new TableQuery<DocClassification>();
                var queryResult = Task.Run(() => DocClassifications.ExecuteQuerySegmentedAsync(q, token)).GetAwaiter().GetResult();
                foreach (var item in queryResult.Results)
                {
                    docClassificationList.Add(item.Classification);
                }
                token = queryResult.ContinuationToken;
            } while (token != null);

            ViewBag.docClassList = docClassificationList;

            //add entity to existing annotation table
            async void createEntity()
            {
                //retrieves annotation entity where partitionKey = counter in table
                TableOperation retrieveOperation = TableOperation.Retrieve<Annotation>(annotationCounter.ToString(), "A" + annotationCounter.ToString());
                TableResult query = await Annotations.ExecuteAsync(retrieveOperation);

                //if entity annotation exists add to counter
                while (query.Result != null)
                {
                    annotationCounter++;
                    retrieveOperation = TableOperation.Retrieve<Annotation>(annotationCounter.ToString(), "A" + annotationCounter.ToString());

                    query = await Annotations.ExecuteAsync(retrieveOperation);
                }

                // Create an annotation entity and add it to the table.
                Annotation Annotation = new Annotation(annotationCounter.ToString(), annotationCounter.ToString());
                Annotation.AnnotationID = "A" + annotationCounter.ToString();
                Annotation.ClassificationID = "T1"; //get this value from dropdown list
                Annotation.DocumentID = docID;
                Annotation.StartCharLocation = "253"; 
                Annotation.EndCharLocation = "300";
                Annotation.Accept = 0;
                Annotation.Deny = 0;
                Annotation.HighlightedText = highlightedText;

                TableOperation insertOperation = TableOperation.Insert(Annotation);

                async void AddAnnotationEntities()
                {
                    await Annotations.ExecuteAsync(insertOperation);
                }
                AddAnnotationEntities();

                //the below comment logic will only be executed if there is actually a comment

                //retrieves comment entity where partitionKey = counter in table
                TableOperation retrieveOperation2 = TableOperation.Retrieve<Comment>(commentCounter.ToString(), "C" + commentCounter.ToString());
                TableResult query2 = await Annotations.ExecuteAsync(retrieveOperation2);

                //if comment entity exists add to counter
                while (query2.Result != null)
                {
                    commentCounter++;
                    retrieveOperation2 = TableOperation.Retrieve<Comment>(commentCounter.ToString(), "C" + commentCounter.ToString());

                    query2 = await Annotations.ExecuteAsync(retrieveOperation2);
                }

                // Create a comment entity and add it to the table.
                Comment Comment = new Comment(commentCounter.ToString(), commentCounter.ToString());
                Comment.CommentID = "C" + commentCounter.ToString();
                Comment.CommentText = "This is a comment"; //get from text box in view
                Comment.Date = DateTime.Now;
                Comment.AnnotationID = Annotation.AnnotationID;

                TableOperation insertOperation2 = TableOperation.Insert(Comment);

                async void AddCommentEntities()
                {
                    await Annotations.ExecuteAsync(insertOperation);
                }
                AddCommentEntities();
            }
            createEntity();

            return RedirectToAction("Index", "Home");
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
