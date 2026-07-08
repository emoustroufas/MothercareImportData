using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class TagRecord
    {
        public string Code { get; set; }//CD
        public string Description { get; set; }//PERI
        public string ItemCode { get; set; }//AP_EIDH_CD
        public DateTime? DateFrom { get; set; }//HMER_APO
        public DateTime? DateTo { get; set; }//HMER_EOS
        public bool Active { get; set; }//ENERG_SW
    }
}
