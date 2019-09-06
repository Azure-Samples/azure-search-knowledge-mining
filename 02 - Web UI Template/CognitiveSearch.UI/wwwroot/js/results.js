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

        var result = data.results[i].document;
        var name;
        var title; 

        result.idx = i;

        var id = result[data.idField]; 

        var tags = GetTagsHTML(result);

        var path;
        if (data.isPathBase64Encoded) {
            path = Base64Decode(result.metadata_storage_path) + token;
        }
        else {
            path = result.metadata_storage_path + token;
        }

        if (result["metadata_storage_name"] !== undefined) {
            name = result.metadata_storage_name.split(".")[0];
        }
        
        if (result["metadata_title"] !== undefined) {
            title = result.metadata_title;
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

            if (pathLower.includes(".jpg") || pathLower.includes(".png")) {
                resultsHtml += `<div class="${classList}" onclick="ShowDocument('${id}');">
                                    <div class="search-result">
                                        <img class="img-result" style='max-width:100%;' src="${path}"/>
                                        <div class="results-header">
                                            <h4>${name}</h4>
                                        </div>
                                        <div>${tags}</div>
                                    </div>
                                </div>`;
            }
            else if (pathLower.includes(".mp3")) {
                resultsHtml += `<div class="${classList}" onclick="ShowDocument('${id}');">
                                    <div class="search-result">
                                        <div class="audio-result-div">
                                            <audio controls>
                                                <source src="${path}" type="audio/mp3">
                                                Your browser does not support the audio tag.
                                            </audio>
                                        </div>
                                        <div class="results-header">
                                            <h4>${name}</h4>
                                        </div>
                                        <div>${tags}</div>                               
                                    </div>
                                </div>`;
            }
            else if (pathLower.includes(".mp4")) {
                resultsHtml += `<div class="${classList}" onclick="ShowDocument('${id}');">
                                    <div class="search-result">
                                        <div class="video-result-div">
                                            <video controls class="video-result">
                                                <source src="${path}" type="video/mp4">
                                                Your browser does not support the video tag.
                                            </video>
                                        </div>
                                        <hr />
                                        <div class="results-header">
                                            <h4>${name}</h4>
                                        </div>
                                        <div>${tags}</div>                                 
                                    </div>
                                </div>`;
            }
            else {
                var icon = " ms-Icon--Page";

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

                resultsHtml += `<div class="${classList}" onclick="ShowDocument('${id}');">
                                    <div class="search-result">
                                       <div class="results-icon col-md-1">
                                            <div class="ms-CommandButton-icon">
                                                <i class="html-icon ms-Icon ${icon}"></i>
                                            </div>
                                        </div>
                                        <div class="results-body col-md-11">
                                            <h4>${title}</h4>
                                            <h5>${name}</h5>
                                            <div style="margin-top:10px;">${tags}</div>
                                        </div>
                                    </div>
                                </div>`;
            }
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