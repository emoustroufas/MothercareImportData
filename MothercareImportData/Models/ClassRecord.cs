using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class ClassRecord
    {
        public string Division { get; set; }//DIVISION
        public string DepartmentCode { get; set; }//AP_OMAD1_CD
        public string SubdeptCode { get; set; }//AP_OMAD2_CD
        public string Code { get; set; }//CD
        public string Description { get; set; }//PERI
    }
}
