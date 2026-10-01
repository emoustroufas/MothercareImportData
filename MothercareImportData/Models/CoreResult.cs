using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class CoreResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public class CoreDataResult<T>  : CoreResult
    {
        public T Data { get; set; }
    }
}
