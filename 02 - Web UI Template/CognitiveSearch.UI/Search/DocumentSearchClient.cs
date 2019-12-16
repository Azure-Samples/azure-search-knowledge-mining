// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.ApplicationInsights;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Spatial;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CognitiveSearch.UI
{
    public class DocumentSearchClient
    {
        private SearchServiceClient _searchClient;
        private ISearchIndexClient _indexClient;
        private string searchServiceName { get; set; }
        private string apiKey { get; set; } 
        private string IndexName { get; set; }

        private string idField { get; set; }

        // Client logs all searches in Application Insights
        private static TelemetryClient telemetryClient = new TelemetryClient();
        public static string _searchId;

        public SearchSchema Schema { get; set; }
        public SearchModel Model { get; set; }

        public static string errorMessage;

        public DocumentSearchClient(IConfiguration configuration)
        {
            try
            {
                searchServiceName = configuration.GetSection("SearchServiceName")?.Value;
                apiKey = configuration.GetSection("SearchApiKey")?.Value;
                IndexName = configuration.GetSection("SearchIndexName")?.Value;
                idField = configuration.GetSection("KeyField")?.Value;
                telemetryClient.InstrumentationKey = configuration.GetSection("InstrumentationKey")?.Value;

                // Create an HTTP reference to the catalog index
                _searchClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
                _indexClient = _searchClient.Indexes.GetClient(IndexName);

                Schema = new SearchSchema().AddFields(_searchClient.Indexes.Get(IndexName).Fields);
                Model = new SearchModel(Schema);

            }
            catch (Exception e)
            {
                // If you get an exceptio here, most likely you have not set your
                // credentials correctly in appsettings.json
                throw new ArgumentException(e.Message.ToString());
            }
        }

        public DocumentSearchResult<Document> Search(string searchText, SearchFacet[] searchFacets = null, string[] selectFilter = null, int currentPage = 1)
        {
            try
            {
                SearchParameters sp = GenerateSearchParameters(searchFacets, selectFilter, currentPage);

                if (!string.IsNullOrEmpty(telemetryClient.InstrumentationKey))
                {
                    var s = GenerateSearchId(searchText, sp);
                    _searchId = s.Result;
                }

                return _indexClient.Documents.Search(searchText, sp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

        public SearchParameters GenerateSearchParameters(SearchFacet[] searchFacets = null, string[] selectFilter = null, int currentPage = 1)
        {
        // For more information on search parameters visit: 
        // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.search.models.searchparameters?view=azure-dotnet
            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.All,
                Top = 10,
                Skip = (currentPage - 1) * 10,
                IncludeTotalResultCount = true,
                QueryType = QueryType.Full,
                Select = selectFilter,
                Facets = Model.Facets.Select(f => f.Name).ToList()
            };

            string filter = null;
            var filterStr = string.Empty;

            if (searchFacets != null)
            {
                foreach (var item in searchFacets)
                {
                    var facet = Model.Facets.Where(f => f.Name == item.Key).FirstOrDefault();

                    filterStr = string.Join(",", item.Value);

                    // Construct Collection(string) facet query
                    if (facet.Type == typeof(string[]))
                    {
                        if (string.IsNullOrEmpty(filter))
                            filter = $"{item.Key}/any(t: search.in(t, '{filterStr}', ','))";
                        else
                            filter += $" and {item.Key}/any(t: search.in(t, '{filterStr}', ','))";
                    }
                    // Construct string facet query
                    else if (facet.Type == typeof(string))
                    {
                        if (string.IsNullOrEmpty(filter))
                            filter = $"{item.Key} eq '{filterStr}'";
                        else
                            filter += $" and {item.Key} eq '{filterStr}'";
                    }
                    // Construct DateTime facet query
                    else if (facet.Type == typeof(DateTime))
                    {
                        // TODO: Date filters
                    }
                }
            }

            sp.Filter = filter;
            return sp;
        }

        public DocumentSuggestResult<Document> Suggest(string searchText, bool fuzzy)
        {
            // Execute search based on query string
            try
            {
                SuggestParameters sp = new SuggestParameters()
                {
                    UseFuzzyMatching = fuzzy,
                    Top = 8
                };

                return _indexClient.Documents.Suggest(searchText, "sg", sp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

        public Document LookUp(string id)
        {
            // Execute geo search based on query string
            try
            {
                return _indexClient.Documents.Get(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

        private async Task<string> GenerateSearchId(string searchText, SearchParameters sp)
        {
            var client = new SearchIndexClient(searchServiceName, IndexName, new SearchCredentials(apiKey));
            var headers = new Dictionary<string, List<string>>() { { "x-ms-azs-return-searchid", new List<string>() { "true" } } };
            var response = await client.Documents.SearchWithHttpMessagesAsync(searchText: searchText, searchParameters: sp, customHeaders: headers);
            IEnumerable<string> headerValues;
            string searchId = string.Empty;
            if (response.Response.Headers.TryGetValues("x-ms-azs-searchid", out headerValues))
            {
                searchId = headerValues.FirstOrDefault();
            }
            return searchId;
        }

        public string GetSearchId()
        {
            if (_searchId != null) { return _searchId; }
            return string.Empty;
        }

        public DocumentSearchResult<Document> GetFacets(string searchText, string facetName, int maxCount = 30)
        {
            // Execute search based on query string
            try
            {
                SearchParameters sp = new SearchParameters()
                {
                    SearchMode = SearchMode.Any,
                    Top = 10,
                    Select = new List<String>() { idField },
                    Facets = new List<String>() { $"{facetName}, count:{maxCount}" },
                    QueryType = QueryType.Full
                };

                return _searchClient.Indexes.GetClient(IndexName).Documents.Search(searchText, sp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

    }
}
