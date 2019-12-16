// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Initialize global properties
var q, sortType, tempdata, instrumentationKey;
var results = [];
var facets = [];
var selectedFacets = [];
var currentPage = 1;
var searchId;
var searchServiceName = "";
var indexName = "";
var scoringProfile = "";

// When 'Enter' clicked from Search Box, execute Search()
$("#q").keyup(function (e) {
    if (e.keyCode === 13) {
        Search();
    }
});

$("#transcript-search-input").keyup(function (e) {
    if (e.keyCode === 13) {
        SearchTranscript($('#transcript-search-input').val());
    }
});

// Search with query and facets
function Search() {
    if (results && results.length > 0) {
        $('#loading-indicator').show();
    }
    else $('#progress-indicator').show();

    if (currentPage > 1) {
        if (q != $("#q").val()) {
            currentPage = 1;
        }
    }
    q = $("#q").val();



    // Get center of map to use to score the search results
    $.post('/home/getdocuments',
        {
            q: q != undefined ? q : "*",
            searchFacets: selectedFacets,
            currentPage: currentPage
        },
        function (data) {
            $('#loading-indicator').css("display", "none");
            $('#progress-indicator').css("display", "none");
            Update(data);
        });
}

function Update(data) {
    results = data.results;
    facets = data.facets;
    tags = data.tags;

    searchId = data.searchId;

    //Facets
    UpdateFacets();

    //Results List
    UpdateResults(data);

    //Pagination
    UpdatePagination(data.count);

    // Log Search Events
    LogSearchAnalytics(data.count);

    //Filters
    UpdateFilterReset();

    InitLayout();

    $('html, body').animate({ scrollTop: 0 }, 'fast');

    FabricInit();
}

function UpdatePagination(docCount) {
    var totalPages = Math.round(docCount / 10);
    // Set a max of 5 items and set the current page in middle of pages
    var startPage = currentPage;

    var maxPage = startPage + 5;
    if (totalPages < maxPage)
        maxPage = totalPages + 1;
    var backPage = parseInt(currentPage) - 1;
    if (backPage < 1)
        backPage = 1;
    var forwardPage = parseInt(currentPage) + 1;

    var htmlString = "";
    if (currentPage > 1) {
        htmlString = `<li><a href="javascript:void(0)" onclick="GoToPage('${backPage}')" class="ms-Icon ms-Icon--ChevronLeftMed"></a></li>`;
    }

    htmlString += '<li class="active"><a href="#">' + currentPage + '</a></li>';

    if (currentPage <= totalPages) {
        htmlString += `<li><a href="javascript:void(0)" onclick="GoToPage('${forwardPage}')" class="ms-Icon ms-Icon--ChevronRightMed"></a></li>`;
    }
    $("#pagination").html(htmlString);
    $("#paginationFooter").html(htmlString);
}

function GoToPage(page) {
    currentPage = page;
    Search();
}

function SampleSearch(text) {
    $('#index-search-input').val(text);
    $('#index-search-submit').click();
}

