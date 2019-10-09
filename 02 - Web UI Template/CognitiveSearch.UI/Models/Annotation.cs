using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace CognitiveSearch.UI.Models
{
    public class Annotations
    {
        public class Annotation : TableEntity
        {
            public Annotation(string PKey, string RKey)
            {
                this.PartitionKey = PKey;
                this.RowKey = "A" + RKey;
            }

            public Annotation() { }

            public string AnnotationID { get; set; }
            public string ClassificationID { get; set; }
            public string DocumentID { get; set; }
            public string StartCharLocation { get; set; }
            public string EndCharLocation { get; set; }
            public string HighlightedText { get; set; }
            public int Accept { get; set; }
            public int Deny { get; set; }
        }
    }
}
