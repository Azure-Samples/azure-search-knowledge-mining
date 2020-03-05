// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CognitiveSearch.UI.Models;

namespace CognitiveSearch.UI.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration { get; set; }
        private DocumentSearchClient _docSearch { get; set; }
        private string _configurationError { get; set; }

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            InitializeDocSearch();
        }

        private void InitializeDocSearch()
        {
            try
            {
                _docSearch = new DocumentSearchClient(_configuration);
            }
            catch (Exception e)
            {
                _configurationError = $"The application settings are possibly incorrect. The server responded with this message: " + e.Message.ToString();
            }
        }

        /// <summary>
        /// Checks that the search client was intiailized successfully.
        /// If not, it will add the error reason to the ViewBag alert.
        /// </summary>
        /// <returns>A value indicating whether the search client was initialized succesfully.</returns>
        public bool CheckDocSearchInitialized()
        {
            if (_docSearch == null)
            {
                ViewBag.Style = "alert-warning";
                ViewBag.Message = _configurationError;
                return false;
            }

            return true;
        }

        public IActionResult Index()
        {
            CheckDocSearchInitialized();

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
            var searchidId = string.Empty;

            if (CheckDocSearchInitialized())
                searchidId = _docSearch.GetSearchId().ToString();

            if (searchidId != string.Empty)
                TempData["searchId"] = searchidId;

            TempData["query"] = q;
            TempData["applicationInstrumentationKey"] = _configuration.GetSection("InstrumentationKey")?.Value;

            return View();
        }

        [HttpPost]
        public IActionResult GetDocuments(string q = "", SearchFacet[] searchFacets = null, int currentPage = 1)
        {
            if (!CheckDocSearchInitialized())
            {
                return View();
            }

            DocumentResult result = _docSearch.GetDocuments(q, searchFacets, currentPage);

            return new JsonResult(result);
        }


        [HttpPost]
        public IActionResult GetDocumentById(string id = "")
        {
            var result = _docSearch.GetDocumentById(id);

            return new JsonResult(result);
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

        [HttpPost]
        public ActionResult GetGraphData(string query)
        {
            string facetsList = _configuration.GetSection("GraphFacet")?.Value;

            string[] facetNames = facetsList.Split(new char[] {',',' '}, StringSplitOptions.RemoveEmptyEntries);

            if (query == null)
            {
                query = "*";
            }
            FacetGraphGenerator graphGenerator = new FacetGraphGenerator(_docSearch);
            var graphJson = graphGenerator.GetFacetGraphNodes(query, facetNames.ToList<string>());

            return Content(graphJson.ToString(), "application/json");
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
