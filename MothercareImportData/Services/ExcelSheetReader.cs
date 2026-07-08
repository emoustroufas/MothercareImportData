using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NPOI.SS.UserModel;
using System.Text;
using System.Threading.Tasks;
using MothercareImportData.Models;

namespace MothercareImportData.Services
{
    /// <summary>
    /// Γενικός μηχανισμός ανάγνωσης ενός NPOI ISheet σε List&lt;T&gt;.
    /// Κάνει mapping ανά όνομα header (ExcelColumnAttribute ή, αν λείπει, το όνομα της property),
    /// άρα δεν εξαρτάται από τη σειρά των στηλών.
    /// </summary>
    public static class ExcelSheetReader
    {
        /// <param name="sheet">Το worksheet προς ανάγνωση.</param>
        /// <param name="headerRowIndex">0-based index της γραμμής που περιέχει τα headers.
        /// Οι περισσότερες καρτέλες το έχουν στο 0. Οι "add ons" και "tags" το έχουν στο 1
        /// (η πρώτη γραμμή είναι σχόλιο).</param>
        public static List<T> Read<T>(ISheet sheet, int headerRowIndex = 0) where T : new()
        {
            var result = new List<T>();

            var headerRow = sheet.GetRow(headerRowIndex)
                ?? throw new InvalidOperationException(
                    $"Το φύλλο '{sheet.SheetName}' δεν έχει γραμμή headers στη θέση {headerRowIndex}.");

            // header text -> column index
            var headerMap = new Dictionary<string, int>();
            foreach (ICell cell in headerRow)
            {
                var text = GetCellAsString(cell)?.Trim();
                if (!string.IsNullOrEmpty(text) && !headerMap.ContainsKey(text))
                    headerMap[text] = cell.ColumnIndex;
            }

            // property -> column index
            var propertyMap = typeof(T).GetProperties()
                .Select(p => new
                {
                    Property = p,
                    Header = p.GetCustomAttribute<ExcelColumnAttribute>()?.HeaderName ?? p.Name
                })
                .Where(x => headerMap.ContainsKey(x.Header))
                .Select(x => (x.Property, ColumnIndex: headerMap[x.Header]))
                .ToList();

            for (int rowIdx = headerRowIndex + 1; rowIdx <= sheet.LastRowNum; rowIdx++)
            {
                var row = sheet.GetRow(rowIdx);
                if (row == null || IsRowEmpty(row)) continue;

                var item = new T();
                foreach (var (property, columnIndex) in propertyMap)
                {
                    var cell = row.GetCell(columnIndex);
                    SetPropertyValue(item, property, cell);
                }
                result.Add(item);
            }

            return result;
        }

        private static bool IsRowEmpty(IRow row)
        {
            foreach (ICell cell in row)
            {
                if (cell.CellType != CellType.Blank && !string.IsNullOrWhiteSpace(GetCellAsString(cell)))
                    return false;
            }
            return true;
        }
        /*
        private static void SetPropertyValue(object target, PropertyInfo property, ICell cell)
        {
            if (cell == null || cell.CellType == CellType.Blank) return;

            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            try
            {
                object value = targetType switch
                {
                    Type t when t == typeof(string) => GetCellAsString(cell),
                    Type t when t == typeof(int) => (int)GetCellAsDouble(cell),
                    Type t when t == typeof(long) => (long)GetCellAsDouble(cell),
                    Type t when t == typeof(decimal) => (decimal)GetCellAsDouble(cell),
                    Type t when t == typeof(double) => GetCellAsDouble(cell),
                    Type t when t == typeof(bool) => GetCellAsBool(cell),
                    Type t when t == typeof(DateTime) => GetCellAsDateTime(cell),
                    _ => null
                };

                if (value != null)
                    property.SetValue(target, value);
            }
            catch
            {
                // Σφάλμα μετατροπής σε επίπεδο κελιού -> αγνοείται (προαιρετικά: logging εδώ).
            }
        }
        */
        // Type -> συνάρτηση μετατροπής ICell -> object. Γεμίζεται μία φορά, στατικά.
        private static readonly Dictionary<Type, Func<ICell, object>> CellConverters;

        static ExcelSheetReader()
        {
            CellConverters = new Dictionary<Type, Func<ICell, object>>();
            CellConverters.Add(typeof(string), cell => GetCellAsString(cell));
            CellConverters.Add(typeof(int), cell => (int)GetCellAsDouble(cell));
            CellConverters.Add(typeof(long), cell => (long)GetCellAsDouble(cell));
            CellConverters.Add(typeof(decimal), cell => (decimal)GetCellAsDouble(cell));
            CellConverters.Add(typeof(double), cell => GetCellAsDouble(cell));
            CellConverters.Add(typeof(bool), cell => GetCellAsBool(cell));
            CellConverters.Add(typeof(DateTime), cell => GetCellAsDateTime(cell));
        }

        private static void SetPropertyValue(object target, PropertyInfo property, ICell cell)
        {
            if (cell == null || cell.CellType == CellType.Blank) return;

            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (!CellConverters.ContainsKey(targetType)) return;

            try
            {
                var convert = CellConverters[targetType];
                var value = convert(cell);

                if (value != null)
                    property.SetValue(target, value);
            }
            catch
            {
                // Σφάλμα μετατροπής σε επίπεδο κελιού -> αγνοείται (προαιρετικά: logging εδώ).
            }
        }

        private static string GetCellAsString(ICell cell)
        {
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    return DateUtil.IsCellDateFormatted(cell)
                        ? cell.DateCellValue?.ToString("dd/MM/yyyy") ?? ""
                        : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                case CellType.Formula:
                    return cell.CachedFormulaResultType == CellType.Numeric
                        ? cell.NumericCellValue.ToString(CultureInfo.InvariantCulture)
                        : cell.StringCellValue;
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                default:
                    return cell.ToString()?.Trim() ?? "";
            }
        }

        private static double GetCellAsDouble(ICell cell)
        {
            if (cell.CellType == CellType.Numeric) return cell.NumericCellValue;
            if (cell.CellType == CellType.Formula && cell.CachedFormulaResultType == CellType.Numeric)
                return cell.NumericCellValue;
            if (double.TryParse(GetCellAsString(cell), NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                return val;
            return 0;
        }

        private static bool GetCellAsBool(ICell cell)
        {
            if (cell.CellType == CellType.Boolean) return cell.BooleanCellValue;
            if (cell.CellType == CellType.Numeric) return cell.NumericCellValue != 0;
            var s = GetCellAsString(cell).ToLowerInvariant();
            return true;
        }

        private static DateTime? GetCellAsDateTime(ICell cell)
        {
            if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                return cell.DateCellValue;
            if (cell.CellType == CellType.String && DateTime.TryParse(cell.StringCellValue, out var dt))
                return dt;
            return null;
        }
    }
}
