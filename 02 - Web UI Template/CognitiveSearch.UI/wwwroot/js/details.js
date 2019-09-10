// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Details
function ShowDocument(id) {
    $.post('/home/getdocumentbyid',
        {
            id: id
        },
        function (data) {
            result = data.result;

            var pivotLinksHTML = "";

            $('#details-pivot-content').html("");
            $('#reference-viewer').html("");
            $('#tag-viewer').html("");
            $('#details-viewer').html("").css("display", "none");

            $('#result-id').val(id);

            var path;
            if (data.isPathBase64Encoded) {
                path = Base64Decode(result.metadata_storage_path) + token;
            }
            else {
                path = result.metadata_storage_path + token;
            }

            var fileContainerHTML = GetFileHTML(path);

            var transcriptContainerHTML = htmlDecode(result.content.trim());

            // If we have merged content, let's use it.
            if (result.merged_content.length > 0) {
                transcriptContainerHTML = htmlDecode(result.merged_content.trim());
            }

            var fileName = "File";

            $('#details-pivot-content').html(`<div id="file-pivot" class="ms-Pivot-content" data-content="file">
                                            <div id="file-viewer" style="height: 100%;">
                                                <pre id="file-viewer-pre"></pre>
                                            </div>
                                        </div>
                                        <div id="transcript-pivot" class="ms-Pivot-content" data-content="transcript">
                                            <div id="transcript-viewer" style="height: 100%;">
                                                <div id='transcript-div'>
                                                    <pre id="transcript-viewer-pre"></pre>
                                                </div>
                                            </div>
                                        </div>`);

            $('#file-viewer-pre').html(fileContainerHTML);
            $('#transcript-viewer-pre').html(transcriptContainerHTML);

            pivotLinksHTML += `<li id="file-pivot-link" class="ms-Pivot-link is-selected" data-content="file" title="File" tabindex="1">${fileName}</li>
                       <li id="transcript-pivot-link" class="ms-Pivot-link " data-content="transcript" title="Transcript" tabindex="1">Transcript</li>`;

            var tagContainerHTML = GetTagsHTML(result);

            $('#details-pivot-links').html(pivotLinksHTML);
            $('#tag-viewer').html(tagContainerHTML);
            $('#details-modal').modal('show');

            var PivotElements = document.querySelectorAll(".ms-Pivot");
            for (var i = 0; i < PivotElements.length; i++) {
                new fabric['Pivot'](PivotElements[i]);
            }

            //Log Click Events
            LogClickAnalytics(result.metadata_storage_name, 0);
        });
}

function GetMatches(string, regex, index) {
    var matches = [];
    var match;
    while (match = regex.exec(string)) {
        matches.push(match[index]);
    }
    return matches;
}

function GetFileHTML(path) {

    if (path !== null) {
        var pathLower = path.toLowerCase();
        var fileContainherHTML;

        if (pathLower.includes(".pdf")) {
            fileContainerHTML =
                `<object class="file-container" data="${path}" type="application/pdf">
                </object>`;
        }
        else if (pathLower.includes(".txt") || pathLower.includes(".json")  ) {            
            fileContainerHTML = htmlDecode(result.content.trim());
        }
        else if (pathLower.includes(".jpg") || pathLower.includes(".jpeg") || pathLower.includes(".gif") || pathLower.includes(".png")) {
            fileContainerHTML =
                `<div class="file-container">
                    <img style='max-width:100%;' src="${path}"/>
                </div>`;
        }
        else if (pathLower.includes(".xml")) {
            fileContainerHTML =
                `<iframe class="file-container" src="${path}" type="text/xml">
                    This browser does not support XMLs. Please download the XML to view it: <a href="${path}">Download XML</a>"
                </iframe>`;
        }
        else if (pathLower.includes(".htm")) {
            var srcPrefixArr = result.metadata_storage_path.split('/');
            srcPrefixArr.splice(-1, 1);
            var srcPrefix = srcPrefixArr.join('/');

            var htmlContent = result.content.replace(/src=\"/gi, `src="${srcPrefix}/`);

            fileContainerHTML =
                `${htmlContent}`;
        }
        else if (pathLower.includes(".mp3")) {
            fileContainerHTML =
                `<audio controls>
                  <source src="${path}" type="audio/mp3">
                  Your browser does not support the audio tag.
                </audio>`;
        }
        else if (pathLower.includes(".mp4")) {
            fileContainerHTML =
                `<video controls class="video-result">
                        <source src="${path}" type="video/mp4">
                        Your browser does not support the video tag.
                    </video>`;
        }
        else if (pathLower.includes(".doc") || pathLower.includes(".ppt") || pathLower.includes(".xls")) {
            var src = "https://view.officeapps.live.com/op/view.aspx?src=" + encodeURIComponent(path);

            fileContainerHTML =
                `<iframe class="file-container" src="${src}"></iframe>`;
        }
        else {
            fileContainerHTML =
                `<div>This file cannot be previewed. Download it here to view: <a href="${path}">Download</a></div>`;
        }
    }
    else {
        fileContainerHTML =
            `<div>This file cannot be previewed or downloaded.`;
    }

    return fileContainerHTML;
}

function GetSearchReferences(q) {
    var copy = q;

    copy = copy.replace(/~\d+/gi, "");
    matches = GetMatches(copy, /\w+/gi, 0);

    matches.forEach(function (match) {
        GetReferences(match, true);
    });
}

function SearchTranscript(searchText) {
    $('#reference-viewer').html("");

    if (searchText !== "") {
        // get whole phrase
        GetReferences(searchText, false);
    }
}