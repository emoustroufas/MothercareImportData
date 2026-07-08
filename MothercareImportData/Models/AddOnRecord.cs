using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class AddOnRecord
    {
        public string AddonTypeCode { get; set; }//SP_ADDONS0_CD
        public string ItemCode { get; set; }//AP_EIDH_CD
        public string AddOnItemCode { get; set; }//AP_EIDH_CD_ADD
    }
}
