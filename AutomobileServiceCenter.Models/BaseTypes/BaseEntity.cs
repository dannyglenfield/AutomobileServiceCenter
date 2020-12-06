using System;
using Microsoft.Azure.Cosmos.Table;

namespace AutomobileServiceCenter.Models.BaseTypes
{
    public class BaseEntity : TableEntity
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
