using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class ProductAttributeRecord
    {
        public int LanguageCode { get; set; }//CN_LANG_CD
        public string ProductCode { get; set; }//AP_EIDH_CD
        public string AttributeCode { get; set; }//AP_ATTR0_CD
        public string AttributeValueCode { get; set; }//AP_ATTR1_CD
        public string FreeText { get; set; }//PERI
    }
    public class ProductAttributesGrouped
    {
        public string ProductCode { get; set; }
        public List<ProductAttributeGrouped> Attributes { get; set; }
        public ProductAttributesGrouped()
        {
            Attributes = new List<ProductAttributeGrouped>();
        }
    }
    public class ProductAttributeGrouped
    {
        public string AttributeCode { get; set; }
        public List<ProductAttributeValueGrouped> Values { get; set; }
        public ProductAttributeGrouped()
        {
            Values = new List<ProductAttributeValueGrouped>();
        }
    }
    public class ProductAttributeValueGrouped
    {
        public string AttributeValueCode { get; set; }
        public List<ProductAttributeTranslation> Translations { get; set; }
        public ProductAttributeValueGrouped()
        {
            Translations = new List<ProductAttributeTranslation>();
        }
    }
    public class ProductAttributeTranslation
    {
        public int LanguageCode { get; set; }
        public string Description { get; set; }
    }
}
