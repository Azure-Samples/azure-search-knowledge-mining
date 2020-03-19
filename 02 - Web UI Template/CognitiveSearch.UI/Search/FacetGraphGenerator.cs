using Microsoft.Azure.Search.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveSearch.UI
{
    public class NodeInfo
    {
        public NodeInfo(int index, int colorId)
        {
            Index = index;
            ColorId = colorId;
        }
        public int Index { get; set; }
        public int ColorId { get; set; }
        public int Age { get; set; }
    }

    public class FacetGraphGenerator
    {
        private DocumentSearchClient _searchHelper;

        public FacetGraphGenerator(DocumentSearchClient searchClient)
        {
            _searchHelper = searchClient;
        }

        public JObject GetFacetGraphNodes(string q, List<string> facetNames)
        {
            // Calculate nodes for 3 levels
            JObject dataset = new JObject();
            int MaxEdges = 10;
            int MaxLevels = 3;
            int CurrentNodes = 0;
            int originalDistance = 100;

            List<FDGraphEdges> FDEdgeList = new List<FDGraphEdges>();
            // Create a node map that will map a facet to a node - nodemap[0] always equals the q term

            var NodeMap = new Dictionary<string, NodeInfo>();
            
            NodeMap[q] = new NodeInfo(CurrentNodes, 0) { Age = originalDistance };

            // If blank search, assume they want to search everything
            if (string.IsNullOrWhiteSpace(q))
            {
                q = "*";
            }

            List<string> currentLevelTerms = new List<string>();

            List<string> NextLevelTerms = new List<string>();
            NextLevelTerms.Add(q);

            // Iterate through the nodes up to MaxLevels deep to build the nodes or when I hit max number of nodes
            for (var CurrentLevel = 0; CurrentLevel <= MaxLevels && MaxEdges > 0; ++CurrentLevel, MaxEdges /= 2)
            {
                currentLevelTerms = NextLevelTerms.ToList();
                NextLevelTerms.Clear();
                var levelFacetCount = 0;

                foreach (var k in NodeMap)
                    k.Value.Age *= 2;

                foreach (var t in currentLevelTerms)
                {
                    if (levelFacetCount >= MaxEdges)
                        break;

                    DocumentSearchResult<Document> response = _searchHelper.GetFacets(t, facetNames, 10);
                    if (response != null)
                    {
                        int facetColor = 0;

                        foreach (var facetName in facetNames)
                        {
                            var facetVals = (response.Facets)[facetName];
                            facetColor++;

                            var facetIds = new List<int>();
                            foreach (FacetResult facet in facetVals)
                            {
                                var facetValue = facet.Value.ToString();
                                NodeInfo nodeInfo = new NodeInfo(-1, -1);
                                if (NodeMap.TryGetValue(facetValue, out nodeInfo) == false)
                                {
                                    // This is a new node
                                    ++levelFacetCount;
                                    NodeMap[facetValue] = new NodeInfo(++CurrentNodes, facetColor) { Age = originalDistance };

                                    if (CurrentLevel < MaxLevels)
                                    {
                                        NextLevelTerms.Add(facetValue);
                                    }
                                }

                                // Add this facet to the fd list
                                if (NodeMap[t] != NodeMap[facetValue])
                                {
                                    var newNode = NodeMap[facetValue];
                                    FDEdgeList.Add(new FDGraphEdges { source = NodeMap[t].Index, target = newNode.Index, distance = newNode.Age });
                                }
                            }
                        }
                    }
                }
            }

            // Create nodes
            JArray nodes = new JArray();
            int nodeNumber = 0;
            foreach (KeyValuePair<string, NodeInfo> entry in NodeMap)
            {
                nodes.Add(JObject.Parse("{name: \"" + entry.Key.Replace("\"", "") + "\"" + ", id: " + entry.Value.Index + ", color: " + entry.Value.ColorId + "}"));
                nodeNumber++;
            }

            // Create edges
            JArray edges = new JArray();
            foreach (FDGraphEdges entry in FDEdgeList)
            {
                edges.Add(JObject.Parse("{source: " + entry.source + ", target: " + entry.target + ", distance: " + entry.distance + "}"));
            }

            dataset.Add(new JProperty("links", edges));
            dataset.Add(new JProperty("nodes", nodes));

            return dataset;
        }

        public class FDGraphEdges
        {
            public int source { get; set; }
            public int target { get; set; }
            public int distance { get; set; }
        }
    }
}
