// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Graph Configuration
var nodeRadius = 15;
var nodeChargeStrength = -300;
var nodeChargeAccuracy = 0.8;

function SearchEntities() {
    if (currentPage > 1) {
        if (q !== $("#e").val()) {
            currentPage = 1;
        }
    }
    q = $("#e").val();

    Unload();
    document.getElementById("entity-loading-indicator").style.display = "block";

    GetGraph(q);

    // Get center of map to use to score the search results
    $.post('/home/getdocuments',
        {
            q: q !== undefined ? q : "*",
            searchFacets: selectedFacets,
            currentPage: currentPage
        },
        function (data) {
            Update(data);
        });
}

// Load Graph with Search data
function GetGraph(q) {
    if (q === null) {
        q = "*";
    }
    $.ajax({
        type: "POST",
        url: "/Home/GetGraphData",
        data: { query: q },
        success: function (data) {
            // Do something interesting here.
            update(data.links, data.nodes);
        }
    });
}

function LoadEntityMap() {
    document.getElementById("results-container").style.display = "none";
    document.getElementById("details-modal").style.display = "none";
    document.getElementById("entity-map").style.display = "block";
    document.getElementById("entity-loading-indicator").style.display = "block";
    GetGraph(q);
    document.getElementById("e").value = q;
    q = q;
}

function UnloadEntityMap() {
    document.getElementById("results-container").style.display = "block";
    document.getElementById("entity-map").style.display = "none";
    Unload();

    document.getElementById("results-container").style = "row content-results";
    document.getElementById("q").value = q;
    document.getElementById("search-button").click();
}

function EntityMapClick() {
    if (document.getElementById("entity-map").style.display === "none") {
        LoadEntityMap();
    }
    else {
        UnloadEntityMap();
    }
}


function Unload() {
    svg.selectAll(".link").remove();
    svg.selectAll(".edgepath").remove();
    svg.selectAll(".node").remove();
    svg.selectAll(".edgelabel").remove();
    svg.selectAll(".text").remove();
}

var colors = d3.scaleOrdinal(d3.schemeCategory10);
var svg = d3.select("svg"),
    width = +svg.attr("width"),
    height = +svg.attr("height"),
    node,
    link;
    

function nodeBounds() {
    var nodes;

    function force() {
        var i,
            n = nodes.length,
            node;

        for (i = 0; i < n; ++i) {
            node = nodes[i];
            var clampedx = Math.max(nodeRadius, Math.min(width - nodeRadius, node.x));
            var clampedy = Math.max(nodeRadius, Math.min(height - nodeRadius, node.y));
            node.x = clampedx;
            node.y = clampedy;
        }
    }

    force.initialize = function (_) {
        nodes = _;
    };

    return force;
};

function setupSimulation(simulation) {
    return simulation
        .force("link", d3.forceLink()
            .id(function (d) { return d.id; })
            .distance(function (d) { return d.distance; })
            .strength(.5))
        .force("charge", d3.forceManyBody()
            .strength(nodeChargeStrength)
            .theta(nodeChargeAccuracy))
        .force("center", nodeBounds())
        .force("collide", d3.forceCollide(nodeRadius * 2))
        ;
}
var simulation = setupSimulation(d3.forceSimulation());


function update(links, nodes) {
    // Graph implementation
    var colors = d3.scaleOrdinal(d3.schemeCategory10);
    var svg = d3.select("svg"),
        width = +svg.attr("width"),
        height = +svg.attr("height");
        
    svg.append('defs').append('marker')
        .attrs({
            'id': 'arrowhead',
            'viewBox': '-0 -5 10 10',
            'refX': 10 + nodeRadius,
            'refY': 0,
            'orient': 'auto',
            'markerWidth': 10,
            'markerHeight': 10,
            'xoverflow': 'visible'
        })
        .append('svg:path')
        .attr('d', 'M 0,-5 L 10 ,0 L 0,5')
        .attr('fill', '#999')
        .style('stroke', 'none');

    simulation = setupSimulation(d3.forceSimulation());

    link = svg.selectAll(".link")
        .data(links)
        .enter()
        .append("line")
        .attr("class", "link");

    node = svg.selectAll(".node")
        .data(nodes)
        .enter()
        .append("g")
        .attr("class", "node")
        .call(d3.drag()
            .on("start", dragstarted)
            .on("drag", dragged)
            .on("end", dragended)
        );

    node.append("circle")
        .attr("r", nodeRadius)
        .style("fill", function (d, i) {
            return colors(d.color);
        })
        .on("click", function (d) {
            $("#e").val(d.name);
            SearchEntities();
        })
        .append("svg:title")
        .text(function (d) {
            // Determine an initial position
            if (d.id == 0) {
                // Root element is on the left side of the screen
                d.fx = width * 0.15;
                d.fy = height * 0.5;
            }
            else {
                // Arrange other nodes along the right side of the screen. 
                //  start them some varyin offset so the simulation is stable on start.
                d.x = width * 0.8;
                d.y = height * (d.id /100);
            }
            return d.name;
        });

    node.append("title")
        .text(d => d.id);


    edgepaths = svg.selectAll(".edgepath")
        .data(links)
        .enter()
        .append('path')
        .attrs({
            'class': 'edgepath',
            'fill-opacity': 0,
            'stroke-opacity': 0,
            'marker-end': 'url(#arrowhead)'
        })
        .style("pointer-events", "none");

    // Render text in a second pass so it's on top of all the gfx.
    texts = svg.selectAll(".text")
        .data(nodes)
        .enter()
        .append("g")
            .attr("class", "text")
        .append("text")
            .attr("dx", 15)
            .attr("dy", ".35em")
            .attr("font-family", "sans-serif")
            .attr("font-size", "20px")
            .attr("font-weight", "bold")
            .attr("fill", "black")
            .text(d => d.name);


    simulation
        .nodes(nodes)
        .on("tick", ticked);
    simulation.force("link")
        .links(links);
    document.getElementById("entity-loading-indicator").style.display = "none";

    // Step the simulation to let it settle
    for (var i = 0; i < 30; ++i)
        simulation.tick();
}

function ticked() {
    node
        .attr("transform", function (d) { return "translate(" + d.x + ", " + d.y + ")"; });
    texts
        .attr("transform", function (d) { return "translate(" + d.x + ", " + d.y + ")"; });

    link
        .attr("x1", function (d) { return d.source.x; })
        .attr("y1", function (d) { return d.source.y; })
        .attr("x2", function (d) { return d.target.x; })
        .attr("y2", function (d) { return d.target.y; });

    edgepaths.attr('d', function (d) {
        return 'M ' + d.source.x + ' ' + d.source.y + ' L ' + d.target.x + ' ' + d.target.y;
    });

}


function dragstarted(d) {
    if (!d3.event.active) {
        simulation.alphaTarget(0.3).restart();
    }
    d.fx = d.x;
    d.fy = d.y;
}
function dragged(d) {

    // Check if movement beyond svg width/height and set to node
    d.fx = Math.max(nodeRadius, Math.min(width - nodeRadius, d3.event.x));
    d.fy = Math.max(nodeRadius, Math.min(height - nodeRadius, d3.event.y));
}

function dragended(d) {
    //if (!d3.event.active) simulation.alphaTarget(0);

    //// Dont unlock the node if it's the root
    //if (d.id != 0) {
    //    d.fx = undefined;
    //    d.fy = undefined;
    //}
}