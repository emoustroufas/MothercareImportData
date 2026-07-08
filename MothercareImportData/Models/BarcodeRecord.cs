using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class BarcodeRecord
    {
        public string Barcode { get; set; } //CD
        public string ItemCode { get; set; }//AP_EIDH_CD
        public string ColorCode { get; set; }//AP_XROM_CD
        public string SizeCode { get; set; }//AP_MEGE1_CD1
    }
}
