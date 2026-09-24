using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    ////public class AttributeRecord
    ////{
    ////    public int LanguageCode { get; set; }//CN_LANG_CD
    ////    public string ItemCode { get; set; }//AP_EIDH_CD
    ////    public string Attribute0Code { get; set; }//AP_ATTR0_CD
    ////    public string Attribute0Description { get; set; }//AP_ATTR0_PERI
    ////    public string Attribute1Code { get; set; }//AP_ATTR1_CD
    ////    public string Attribute1Description { get; set; }//AP_ATTR1_PERI
    ////    public string FreeText { get; set; }//FREE_TEXT
    ////}
    ////public class AttributeDifference
    ////{
    ////    public string ItemCode { get; set; }
    ////    public int ExcelLanguageCode { get; set; }
    ////    public string Attribute0Code { get; set; }
    ////    public string Attribute1Code { get; set; }
    ////    public string Field { get; set; }      // π.χ. "Attribute0Description", "Attribute1Description"
    ////    public string ExcelValue { get; set; }
    ////    public string SqlValue { get; set; }
    ////    public string Reason { get; set; }     // "Mismatch" | "MissingInSql" | "MissingInExcel"
    ////}
    public class AttributeRecord
    {   
        public string Code { get; set; }// AP_ATTR0_CD από Excel,CODE από SoftOne
        public int SoftOneId { get; set; } // MTRATTRIBUTE από SoftOne
        public List<AttributeTranslation> Translations { get; set; } // Περιγραφή ανά γλώσσα
        public List<AttributeValue> Values { get; set; } // Τιμές attribute
        public AttributeRecord()
        {
            Translations = new List<AttributeTranslation>();
            Values = new List<AttributeValue>();
        }
    }
    public class AttributeTranslation
    {
        public int LanguageCode { get; set; } // CN_LANG_CD / CCCLANGUAGE
        public string Description { get; set; } // ATTR0_DESCR / TRANSLATION
    }

    public class AttributeValue
    {
        public string Code { get; set; } // AP_ATTR1_CD / CODE
        public int SoftOneId { get; set; } // MTRATTRIBUTELN
        public List<AttributeValueTranslation> Translations { get; set; }
        public AttributeValue()
        {
            Translations = new List<AttributeValueTranslation>();
        }
    }

    public class AttributeValueTranslation
    {
        public int LanguageCode { get; set; } // CN_LANG_CD / CCCLANGUAGE
        public string Description { get; set; } // ATTR1_DESCR / TRANSLATION
    }
}
