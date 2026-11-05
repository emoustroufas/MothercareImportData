using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class CategoriesData
    {
        public string Category1Code { get; set; }
        public string Category1Description { get; set; }
        public string Category2Code { get; set; }
        public string Category2Description { get; set; }
        public string Category3Code { get; set; }
        public string Category3Description { get; set; }
        public string Category4Code { get; set; }
        public string Category4Description { get; set; }
        public int Ord1 { get; set; }
        public int Ord2 { get; set; }
        public int Ord3 { get; set; }
        public int Ord4 { get; set; }
        public string Category1DescriptionEn { get; set; }
        public string Category2DescriptionEn { get; set; }
        public string Category3DescriptionEn { get; set; }
        public string Category4DescriptionEn { get; set; }
    }
    public class  CategoriesPerProduct
    {
        public string ProductCode { get; set; }
        public string CategoryCode { get; set; }
        public int IsDefault { get; set; }
    }
    public class CategoriesPerProductChange
    {
        public string ProductCode { get; set; }
        public string CategoryCode { get; set; }
        public int IsDefault { get; set; }
        public string Action { get; set; }
    }
    public class CategoriesPerProductChangeGroup
    {
        public string ProductCode { get; set; }
        public List<CategoriesPerProductChange> Categories { get; set; }
    }
}
