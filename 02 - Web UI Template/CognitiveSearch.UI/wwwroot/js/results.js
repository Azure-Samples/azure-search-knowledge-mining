// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

function Base64Decode(token) {
    if (token.length === 0) return null;
    // The last character in the token is the number of padding characters.
    var numberOfPaddingCharacters = token.slice(-1);
    // The Base64 string is the token without the last character.
    token = token.slice(0, -1);
    // '-'s are '+'s and '_'s are '/'s.
    token = token.replace(/-/g, '+');
    token = token.replace(/_/g, '/');
    // Pad the Base64 string out with '='s
    for (var i = 0; i < numberOfPaddingCharacters; i++)
        token += "=";
    return atob(token);
}

function UpdateResults(data) {
    var resultsHtml = '';

    $("#doc-count").html(` Available Results: ${data.count}`);

    for (var i = 0; i < data.results.length; i++) {

        var result = data.results[i];
        var document = result.document;
        document.idx = i;

        var name;
        var title;
        var content = (result.highlights
                            ? result.highlights.content[0]
                            : document.content?.substring(0, 400))
                            || "";
        var icon = " ms-Icon--Page";
        var id = document[data.idField]; 
        var tags = GetTagsHTML(document);
        var path;

        // get path
        if (data.isPathBase64Encoded) {
            path = Base64Decode(document.metadata_storage_path) + token;
        }
        else {
            path = document.metadata_storage_path + token;
        }

        if (document["metadata_storage_name"] !== undefined) {
            name = document.metadata_storage_name.split(".")[0];
        }
        
        if (document["metadata_title"] !== undefined && document["metadata_title"] !== null) {
            title = document.metadata_title;
        }
        else {
            // Bring up the name to the top
            title = name;
            name = "";
        }


        if (path !== null) {
            var classList = "results-div ";
            if (i === 0) classList += "results-sizer";

            var pathLower = path.toLowerCase();

            if (pathLower.includes(".pdf")) {
                icon = "ms-Icon--PDF";
            }
            else if (pathLower.includes(".htm")) {
                icon = "ms-Icon--FileHTML";
            }
            else if (pathLower.includes(".xml")) {
                icon = "ms-Icon--FileCode";
            }
            else if (pathLower.includes(".doc")) {
                icon = "ms-Icon--WordDocument";
            }
            else if (pathLower.includes(".ppt")) {
                icon = "ms-Icon--PowerPointDocument";
            }
            else if (pathLower.includes(".xls")) {
                icon = "ms-Icon--ExcelDocument";
            }

            var resultContent = "";
            var imageContent = "";

            if (pathLower.includes(".jpg") || pathLower.includes(".png")) {
                icon = "ms-Icon--FileImage";
                imageContent = `<img class="img-result" style='max-width:100%;' src="${path}"/>`;
            }
            else if (pathLower.includes(".mp3")) {
                icon = "ms-Icon--MusicInCollection";
                resultContent = `<div class="audio-result-div">
                                    <audio controls>
                                        <source src="${path}" type="audio/mp3">
                                        Your browser does not support the audio tag.
                                    </audio>
                                </div>`;
            }
            else if (pathLower.includes(".mp4")) {
                icon = "ms-Icon--Video";
                resultContent = `<div class="video-result-div">
                                    <video controls class="video-result">
                                        <source src="${path}" type="video/mp4">
                                        Your browser does not support the video tag.
                                    </video>
                                </div>`;
            }

            var tagsContent = tags ? `<div class="results-body">
                                        <div id="tagdiv${i}" class="tag-container max-lines" style="margin-top:10px;">${tags}</div>
                                    </div>` : "";
            // display:none
            // <div class="col-md-1"><img id="tagimg${i}" src="/images/expand.png" height="30px" onclick="event.stopPropagation(); ShowHideTags(${i});"></div>

            var contentPreview = content ? `<p class="max-lines">${content}</p>` : "";

            resultsHtml += `<div id="resultdiv${i}" class="${classList}" onclick="ShowDocument('${id}');">
                                    <div class="search-result">
                                        ${imageContent}
                                        <div class="results-icon col-md-1">
                                            <div class="ms-CommandButton-icon">
                                                <i class="html-icon ms-Icon ${icon}" style="font-size: 26px;"></i>
                                            </div>
                                        </div>
                                        <div class="results-body col-md-11">
                                            <h4>${title}</h4>
                                            ${contentPreview}
                                            <h5>${name}</h5>
                                            ${tagsContent}
                                            ${resultContent}
                                        </div>
                                    </div>
                                </div>`;
        }
        else {
            resultsHtml += `<div class="${classList}" );">
                                    <div class="search-result">
                                        <div class="results-header">
                                            <h4>Could not get metadata_storage_path for this result.</h4>
                                        </div>
                                    </div>
                                </div>`; 
        }
    }

    $("#doc-details-div").html(resultsHtml);
}

function ShowHideTags(i) {
    var node = document.getElementById("tagdiv" + i);
    var image = document.getElementById("tagimg" + i);
    if (node.style.display === "none") {
        node.style.display = "block";
        image.src = "/images/collapse.png";
    }
    else {
        node.style.display = "none";
        image.src = "/images/expand.png";
    }

    $grid.masonry('layout');
}