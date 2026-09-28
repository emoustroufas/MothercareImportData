using MathNet.Numerics.Optimization.LineSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class NidRecord
    {
        public string ItemCode { get; set; }
        public List<NidStores> NidStores { get; set; }
    }
    public class NidStores
    {
        public int StoreId { get; set; }
        public int Nid { get; set; }
        public DateTime InsDate { get; set; }
    }
}
