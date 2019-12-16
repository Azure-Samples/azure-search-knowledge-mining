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
using Microsoft.Azure.Search.Models;

namespace CognitiveSearch.UI.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration { get; set; }
        private DocumentSearchClient _docSearch { get; set; }
        private string _idField { get; set; }
        bool _isPathBase64Encoded { get; set; }

        // data source information. Currently supporting 3 data sources indexed by different indexers
        private static string[] containerAddresses = null; 
        private static string[] tokens = null;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            _docSearch = new DocumentSearchClient(configuration);
            _idField = _configuration.GetSection("KeyField")?.Value;
            _isPathBase64Encoded = (_configuration.GetSection("IsPathBase64Encoded")?.Value == "True");
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

        [HttpPost]
        public IActionResult GetDocuments(string q = "", SearchFacet[] searchFacets = null, int currentPage = 1)
        {
            GetContainerSasUris();
 
            var selectFilter = _docSearch.Model.SelectFilter;

            if (!string.IsNullOrEmpty(q))
            {
                q = q.Replace("?", "");
            }

            var response = _docSearch.Search(q, searchFacets, selectFilter, currentPage);
            var searchId = _docSearch.GetSearchId().ToString();
            var facetResults = new List<object>();
            var tagsResults = new List<object>();

            if (response != null && response.Facets != null)
            {
                // Return only the selected facets from the Search Model
                foreach (var facetResult in response.Facets.Where(f => _docSearch.Model.Facets.Where(x => x.Name == f.Key).Any()))
                {
                    var cleanValues = GetCleanFacetValues(facetResult);

                    facetResults.Add(new
                    {
                        key = facetResult.Key,
                        value = cleanValues
                    });
                }

                foreach (var tagResult in response.Facets.Where(t => _docSearch.Model.Tags.Where(x => x.Name == t.Key).Any()))
                {
                    var cleanValues = GetCleanFacetValues(tagResult);

                    tagsResults.Add(new
                    {
                        key = tagResult.Key,
                        value = cleanValues
                    });
                }
            }

            return new JsonResult(new DocumentResult
            {
                Results = (response == null? null : response.Results),
                Facets = facetResults,
                Tags = tagsResults,
                Count = (response == null? 0 :  Convert.ToInt32(response.Count)),
                SearchId = searchId,
                IdField = _idField,
                IsPathBase64Encoded = _isPathBase64Encoded
            });
        }

        /// <summary>
        /// In some situations you may want to restrict the facets that are displayed in the
        /// UI. This allows you to add some heuristics to remove facets that you may consider unnecessary.
        /// </summary>
        /// <param name="facetResult"></param>
        /// <returns></returns>
        private static IList<FacetResult> GetCleanFacetValues(KeyValuePair<string, IList<FacetResult>> facetResult)
        {
            IList<FacetResult> cleanValues = new List<FacetResult>();
            
            if (facetResult.Key == "people")
            {
                // only add names that are long enough 
                foreach (var element in facetResult.Value)
                {
                    if (element.Value.ToString().Length >= 4)
                    {
                        cleanValues.Add(element);
                    }
                }

                return cleanValues;
            }
            else
            {
                return facetResult.Value;
            }
        }

        private static string Base64Decode(string input)
        {
            if (input == null) throw new ArgumentNullException("input");
            int inputLength = input.Length;
            if (inputLength  < 1) return null;

            // Get padding chars
            int numPadChars = (int)input[inputLength - 1] - (int)'0';            
            if (numPadChars < 0 || numPadChars > 10)
            {
                return null;
            }

            // replace '-' and '_'
            char[] base64Chars = new char[inputLength - 1 + numPadChars];
            for (int iter = 0; iter < inputLength - 1; iter++)
            {
                char c = input[iter];

                switch (c)
                {
                    case '-':
                        base64Chars[iter] = '+';
                        break;

                    case '_':
                        base64Chars[iter] = '/';
                        break;

                    default:
                        base64Chars[iter] = c;
                        break;
                }
            }

            // Add padding chars
            for (int iter = inputLength - 1; iter < base64Chars.Length; iter++)
            {
                base64Chars[iter] = '=';
            }

            var charArray = Convert.FromBase64CharArray(base64Chars, 0, base64Chars.Length);
            return System.Text.Encoding.Default.GetString(charArray);
        }


        [HttpPost]
        public IActionResult GetDocumentById(string id = "")
        {
            var response = _docSearch.LookUp(id);

            var decodedPath = id;

            if (_isPathBase64Encoded)
            {
                decodedPath = Base64Decode(id);
            }

            string tokenToUse = GetToken(decodedPath);

            return new JsonResult(
                new DocumentResult
                {
                    Result = response,
                    Token = tokenToUse,
                    DecodedPath = decodedPath,
                    IdField = _idField,
                    IsPathBase64Encoded = _isPathBase64Encoded
                });
        }


        public class MapCredentials
        {
            public string MapKey { get; set; }
        }


        [HttpPost]
        public IActionResult GetMapCredentials()
        {
            string mapKey = _configuration.GetSection("AzureMapsSubscriptionKey")?.Value;

            return new JsonResult(
                new MapCredentials
                {
                    MapKey = mapKey
                });
        }

        private string GetToken(string decodedPath)
        {
            // Initialize tokens and containers if not already initialized
            GetContainerSasUris();

            // Determine which token to use.
            string tokenToUse;
            if (decodedPath.ToLower().Contains(containerAddresses[1])) { tokenToUse = tokens[1]; }
            else if (decodedPath.ToLower().Contains(containerAddresses[2])) { tokenToUse = tokens[2]; }
            else { tokenToUse = tokens[0]; }

            return tokenToUse;
        }

        /// <summary>
        /// This will return up to 3 tokens for the storage accounts
        /// </summary>
        /// <returns></returns>
        private void GetContainerSasUris()
        {
            if (tokens == null)
            {
                tokens = new string[3];
                containerAddresses = new string[3];

                string accountName = _configuration.GetSection("StorageAccountName")?.Value;
                string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;

                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                containerAddresses[0] = _configuration.GetSection("StorageContainerAddress")?.Value.ToLower();
                CloudBlobContainer container = new CloudBlobContainer(new Uri(containerAddresses[0]), new StorageCredentials(accountName, accountKey));
                tokens[0] = container.GetSharedAccessSignature(adHocPolicy, null);

                // Get token for second indexer data source
                containerAddresses[1] = _configuration.GetSection("StorageContainerAddress2")?.Value.ToLower();
                CloudBlobContainer container2 = new CloudBlobContainer(new Uri(containerAddresses[1]), new StorageCredentials(accountName, accountKey));
                tokens[1] = container2.GetSharedAccessSignature(adHocPolicy, null);

                // Get token for third indexer data source
                containerAddresses[2] = _configuration.GetSection("StorageContainerAddress3")?.Value.ToLower();
                CloudBlobContainer container3 = new CloudBlobContainer(new Uri(containerAddresses[2]), new StorageCredentials(accountName, accountKey));
                tokens[2] = container3.GetSharedAccessSignature(adHocPolicy, null);
            }
        }

        [HttpPost]
        public JObject GetGraphData(string query)
        {
            string facetName = _configuration.GetSection("GraphFacet")?.Value;

            if (query == null)
            {
                query = "*";
            }
            FacetGraphGenerator graphGenerator = new FacetGraphGenerator(_docSearch);
            var graphJson = graphGenerator.GetFacetGraphNodes(query, facetName);

            return graphJson;
        }

        [HttpPost, HttpGet]
        public ActionResult Suggest(string term, bool fuzzy = true)
        {
            // Change to _docSearch.Suggest if you would prefer to have suggestions instead of auto-completion
            var response = _docSearch.Autocomplete(term, fuzzy);

            List<string> suggestions = new List<string>();
            if (response != null)
            {
                foreach (var result in response.Results)
                {
                    suggestions.Add(result.Text);
                }
            }

            // Get unique items
            List<string> uniqueItems = suggestions.Distinct().ToList();

            return new JsonResult
            (
                uniqueItems
            );

        }
    }
}
