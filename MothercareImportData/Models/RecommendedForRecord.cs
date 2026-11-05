using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class RecommendedForRecord
    {
        public string ItemCode { get; set; }
        public string AttributeCode { get; set; }//AP_ATTR0_CD = 56 Προτείνεται για (Age Group)
    }
    public class RecommendedForChange
    {
        public string ItemCode { get; set; }

        public string AttributeCode { get; set; }

        public string Action { get; set; } // INSERT ή DELETE
    }
}