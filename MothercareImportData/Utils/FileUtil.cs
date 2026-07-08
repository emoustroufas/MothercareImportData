using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Utils
{
    public static class FileUtil
    {
        public static bool InBounds(int index, List<string> array)
        {
            return (index >= 0) && (index < array.Count);
        }
    }
}
