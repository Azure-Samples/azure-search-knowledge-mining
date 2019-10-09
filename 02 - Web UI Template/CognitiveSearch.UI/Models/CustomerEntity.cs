using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace CognitiveSearch.UI.Models
{
    public class ClustomerEntity
    {
        public class CustomerEntity : TableEntity
        {
            public CustomerEntity(string PKey, string RKey)
            {
                this.PartitionKey =  PKey;
                this.RowKey = "C" + RKey;
            }

            public CustomerEntity() {}

            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string Email { get; set; }
            public string HighlightedText { get; set; }
        }
    }
}
