// Graph Configuration
var nodeRadius = 15;
var nodeSeparationFactor = 1;
var nodeChargeStrength = -100;
var nodeChargeAccuracy = 0.4;
var nodeDistance = 100;

function SearchEntities() {
    if (currentPage > 1) {
        if (q !== $("#e").val()) {
            currentPage = 1;
        }
    }
    q = $("#e").val();

    Unload();
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
    GetGraph(q);
}

function UnLoadEntityMap() {
    document.getElementById("results-container").style.display = "block";
    document.getElementById("entity-map").style.display = "none";
    Unload();
}

// Gets DOMRect(angle) value
// TODO: Limit users dragging of circles
function GetSvgBox() {
    var graphRect = document.querySelector("svg");
    return graphRect.getBoundingClientRect();
}

function EntityMapClick() {
    if (document.getElementById("entity-map").style.display === "none") {
        LoadEntityMap();
    }
    else {
        UnLoadEntityMap();
    }
}

// Graph implementation
var colors = d3.scaleOrdinal(d3.schemeCategory10);
var svg = d3.select("svg"),
    width = +svg.attr("width"),
    height = +svg.attr("height"),
    node,
    link;

svg.append('defs').append('marker')
    .attrs({
        'id': 'arrowhead',
        'viewBox': '-0 -5 10 10',
        'refX': 13,
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

var simulation = d3.forceSimulation()
    .force("link", d3.forceLink()
        .id(function (d) { return d.id; })
        .distance(300).strength(.5))
    .force("charge", d3.forceManyBody()
        .strength(nodeChargeStrength)
        .theta(nodeChargeAccuracy))
    .force("center", d3.forceCenter(width / 2, height / 2))
    .force("collide", d3.forceCollide(nodeRadius))
    ;

function Unload() {
    svg.selectAll(".link").remove();
    svg.selectAll(".edgepath").remove();
    svg.selectAll(".node").remove();
    svg.selectAll(".edgelabel").remove();
}

function update(links, nodes) {

    link = svg.selectAll(".link")
        .data(links)
        .enter()
        .append("line")
        .attr("class", "link");
    //.attr('marker-end', 'url(#arrowhead)')
    link.append("title")
        .text(function (d) { return d.type; });

    node = svg.selectAll(".node")
        .data(nodes)
        .enter()
        .append("g")
        .attr("class", "node")
        .call(d3.drag()
            .on("start", dragstarted)
            .on("drag", dragged)
            //.on("end", dragended)
        );
    node.append("circle")
        .attr("r", nodeRadius)
        .style("fill", function (d, i) { return colors(i); });
    node.append("title")
        .text(d => d.id);

    // Text Attributes for nodes
    node.append("text")
        .attr("dx", 15)
        .attr("dy", ".35em")
        .attr("font-family", "sans-serif")
        .attr("font-size", "20px")
        .attr("font-weight", "bold")
        .attr("fill", "black")
        .text(d => d.name);

    edgepaths = svg.selectAll(".edgepath")
        .data(links)
        .enter()
        .append('path')
        .attrs({
            'class': 'edgepath',
            'fill-opacity': 0,
            'stroke-opacity': 0,
            'id': function (d, i) { return 'edgepath' + i; }
        })
        .style("pointer-events", "none");

    // TODO: Add curved Text path
    //node.append("text")
    //    .append("textPath")
    //    .attr("xlink:href", (d, i) => `#labelarc${i}`)
    //    .attr("startOffset", "50%");

    simulation
        .nodes(nodes)
        .on("tick", ticked);
    simulation.force("link")
        .links(links);

    var svgRect = GetSvgBox();
    keepCoordInCanvas(svgRect);
}



function keepCoordInCanvas(svgRect) {
    // n = int
    // dim = "dimensions" x and y coordinates
    // margin = int
    // all above are in an object which is a number (?)

    // if(dim == "X")
    // return max value of (
    // margin vs.
    // min value of (
    // (svgRect.width - margin) or n ))
    // return max value of (
    // margin vs.
    // min value of (
    // (svgRect.height -margin) OR n))
}

function ticked() {
    node
        .attr("transform", function (d) { return "translate(" + d.x + ", " + d.y + ")"; });

    link
        .attr("x1", function (d) { return d.source.x; })
        .attr("y1", function (d) { return d.source.y; })
        .attr("x2", function (d) { return d.target.x; })
        .attr("y2", function (d) { return d.target.y; });

    edgepaths.attr('d', function (d) {
        return 'M ' + d.source.x + ' ' + d.source.y + ' L ' + d.target.x + ' ' + d.target.y;
    });
    // TODO: fix labels
    // edgelabels.attr('transform', function (d) {
    //     if (d.target.x < d.source.x) {
    //         var bbox = this.getBBox();
    //         rx = bbox.x + bbox.width / 2;
    //         ry = bbox.y + bbox.height / 2;
    //         return 'rotate(180 ' + rx + ' ' + ry + ')';
    //     }
    //     else {
    //         return 'rotate(0)';
    //     }
    // });
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
//function dragended(d) {
//    if (!d3.event.active) simulation.alphaTarget(0);
//    d.fx = undefined;
//    d.fy = undefined;
//}