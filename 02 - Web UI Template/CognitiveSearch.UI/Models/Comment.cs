using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace CognitiveSearch.UI.Models
{
    public class Comments
    {
        public class Comment : TableEntity
        {
            public Comment(string PKey, string RKey)
            {
                this.PartitionKey = PKey;
                this.RowKey = "C" + RKey;
            }

            public Comment() { }

            public string CommentID { get; set; }
            public string CommentText { get; set; }
            public DateTime Date { get; set; }
            public string AnnotationID { get; set; }
        }
    }
}
