using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace CognitiveSearch.UI.Models
{
    public class TextClassifications
    {
        public class TextClassification : TableEntity
        {
            public TextClassification(string PKey, string RKey)
            {
                this.PartitionKey = PKey;
                this.RowKey = "TC" + RKey;
            }

            public TextClassification() { }

            public string TextClassID { get; set; }
            public string Classification { get; set; }
        }
    }
}
