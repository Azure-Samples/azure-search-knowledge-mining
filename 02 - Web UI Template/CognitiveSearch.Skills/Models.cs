// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace CognitiveSearch.Skills
{
    public class WebApiResponseError
    {
        public string message { get; set; }
    }

    public class WebApiResponseWarning
    {
        public string message { get; set; }
    }

    public class WebApiResponseRecord
    {
        public string recordId { get; set; }
        public Dictionary<string, object> data { get; set; }
        public List<WebApiResponseError> errors { get; set; }
        public List<WebApiResponseWarning> warnings { get; set; }
    }

    public class WebApiEnricherResponse
    {
        public List<WebApiResponseRecord> values { get; set; }
    }
}
