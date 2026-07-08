using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class AttributeRecord
    {
        public int LanguageCode { get; set; }//CN_LANG_CD
        public string ItemCode { get; set; }//AP_EIDH_CD
        public string Attribute0Code { get; set; }//AP_ATTR0_CD
        public string Attribute0Description { get; set; }//AP_ATTR0_PERI
        public string Attribute1Code { get; set; }//AP_ATTR1_CD
        public string Attribute1Description { get; set; }//AP_ATTR1_PERI
        public string FreeText { get; set; }//FREE_TEXT
    }
}
