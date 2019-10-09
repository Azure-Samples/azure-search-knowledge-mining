using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace CognitiveSearch.UI.Models
{
    public class Documents
    {
        public class Document : TableEntity
        {
            public Document(string PKey, string RKey)
            {
                this.PartitionKey = PKey;
                this.RowKey = "D" + RKey;
            }

            public Document() { }

            public string DocumentID { get; set; }
            public string DocTitle { get; set; }
            public string DocClassID { get; set; }
        }
    }
}
