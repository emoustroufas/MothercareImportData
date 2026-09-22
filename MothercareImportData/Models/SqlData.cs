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
        public int LanguageCode { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int? LanguageCode_Translation { get; set; }
        public string Description_Translation { get; set; }
        public int Line_Id { get; set; }
        public string Line_Code { get; set; }
        public string Line_Description { get; set; }
        public string Line_DescriptionTranslation { get; set; }
    }
}
