using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class VatPerCountry
    {
        public string CountryCode { get; set; }
        public DateTime FromDate { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Subdepartment { get; set; }
        public string Class { get; set; }
        public string Style { get; set; }
        public double Perc { get; set; }
        public int SoftoneId { get; internal set; }
    }
}
