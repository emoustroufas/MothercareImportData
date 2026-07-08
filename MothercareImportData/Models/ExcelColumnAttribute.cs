using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    /// <summary>
    /// Δηλώνει σε ποια στήλη (με βάση το header text της γραμμής headers) αντιστοιχεί
    /// η ιδιότητα (property) της κλάσης. Αν δεν οριστεί, χρησιμοποιείται το όνομα
    /// της property ως header name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExcelColumnAttribute : Attribute
    {
        public string HeaderName { get; }

        public ExcelColumnAttribute(string headerName)
        {
            HeaderName = headerName;
        }
    }
}
