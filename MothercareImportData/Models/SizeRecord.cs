using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class SizeRecord
    {
        public string Code { get; set; }//CD
        public string Description { get; set; }//PERI
        public string SizeGuideCode { get; set; }//AP_MEGE0_CD
        public int OrderByNo { get; set; }//ORDER_BY
    }
}
