// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
            int MaxEdges = 50;
            int MaxLevels = 3;
            int CurrentLevel = 1;
            int CurrentNodes = 0;

            List<FDGraphEdges> FDEdgeList = new List<FDGraphEdges>();
            // Create a node map that will map a facet to a node - nodemap[0] always equals the q term

            Dictionary<string, NodeInfo> NodeMap = new Dictionary<string, NodeInfo>();
            
            NodeMap[q] = new NodeInfo(CurrentNodes, 0);

            // If blank search, assume they want to search everything
            if (string.IsNullOrWhiteSpace(q))
            {
                q = "*";
            }

            List<string> NextLevelTerms = new List<string>();
            NextLevelTerms.Add(q);

            // Iterate through the nodes up to 3 levels deep to build the nodes or when I hit max number of nodes
            while ((NextLevelTerms.Count() > 0) && (CurrentLevel <= MaxLevels) && (FDEdgeList.Count() < MaxEdges))
            {
                q = NextLevelTerms.First();
                NextLevelTerms.Remove(q);
                if (NextLevelTerms.Count() == 0)
                {
                    CurrentLevel++;
                }
                DocumentSearchResult<Document> response = _searchHelper.GetFacets(q, facetNames, 10);
                if (response != null)
                {
                    int facetColor = 0;
                    
                    foreach (var facetName in facetNames)
                    {
                        IList<FacetResult> facetVals = (response.Facets)[facetName];
                        facetColor++;

                        foreach (FacetResult facet in facetVals)
                        {
                            NodeInfo nodeInfo = new NodeInfo(-1,-1);
                            if (NodeMap.TryGetValue(facet.Value.ToString(), out nodeInfo) == false)
                            {
                                // This is a new node
                                CurrentNodes++;
                                int node = CurrentNodes;
                                NodeMap[facet.Value.ToString()] =  new NodeInfo(node, facetColor);
                            }

                            // Add this facet to the fd list
                            if (NodeMap[q] != NodeMap[facet.Value.ToString()])
                            {
                                FDEdgeList.Add(new FDGraphEdges { source = NodeMap[q].Index, target = NodeMap[facet.Value.ToString()].Index });
                                if (CurrentLevel < MaxLevels)
                                {
                                    NextLevelTerms.Add(facet.Value.ToString());
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
                edges.Add(JObject.Parse("{source: " + entry.source + ", target: " + entry.target + "}"));
            }

            dataset.Add(new JProperty("links", edges));
            dataset.Add(new JProperty("nodes", nodes));

            return dataset;
        }

        public class FDGraphEdges
        {
            public int source { get; set; }
            public int target { get; set; }
        }
    }
}
