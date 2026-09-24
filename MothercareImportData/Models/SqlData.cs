using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class SqlData
    {
        public string Obj { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class SqlAttributeData
    {
        //public int LanguageCode { get; set; }
        //public int Id { get; set; }
        //public string Code { get; set; }
        //public string Description { get; set; }
        //public int? LanguageCode_Translation { get; set; }
        //public string Description_Translation { get; set; }
        //public int Line_Id { get; set; }
        //public string Line_Code { get; set; }
        //public string Line_Description { get; set; }
        //public string Line_DescriptionTranslation { get; set; }
        //public List<SoftOneAttribute> Attributes { get; set; }
        //public List<SoftOneAttributeTranslation> AttributeTranslations { get; set; }
        //public List<SoftOneAttributeValue> AttributeValues { get; set; }
        //public List<SoftOneAttributeValueTranslation> AttributeValueTranslations { get; set; }
    }

    //// SQL SoftOne Attribute
    //public class SoftOneAttribute
    //{
    //    public int Id { get; set; }          // MTRATTRIBUTE
    //    public string Code { get; set; }        // CODE
    //    public string Name { get; set; }     // NAME
    //}
    //public class SoftOneAttributeTranslation
    //{
    //    public int AttributeId { get; set; }     // MTRATTRIBUTE
    //    public string LanguageCode { get; set; }    // CCCLANGUAGE
    //    public string Translation { get; set; }
    //}
    //public class SoftOneAttributeValue
    //{
    //    public int ValueId { get; set; }         // MTRATTRIBUTELN
    //    public int AttributeId { get; set; }     // MTRATTRIBUTE
    //    public string Code { get; set; }            // CODE
    //    public string Value { get; set; }        // SOVALUE
    //}
    //public class SoftOneAttributeValueTranslation
    //{
    //    public int AttributeId { get; set; }         // MTRATTRIBUTE
    //    public int ValueId { get; set; }             // MTRATTRIBUTELN
    //    public string LanguageCode { get; set; }        // CCCLANGUAGE
    //    public string Translation { get; set; }
    //}
}
