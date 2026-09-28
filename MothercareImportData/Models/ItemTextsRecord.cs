using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class ItemTextsRecord
    {
        public string ItemCode { get; set; }
        public List<TextsTranslation> Texts { get; set; }
    }
    public class TextsTranslation
    {
        public int LanguageCode { get; set; } 
        public string LabelTitle { get; set; }
        public string LabelDescription { get; set; }
        public string EshopTitle { get; set; }
        public string SmallDescription { get; set; }
        public string FeaturesAndBenefits { get; set; }
        public string LongDescription { get; set; }
    }
}
