using MothercareImportData.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Services
{
    public class ExcelEditor
    {
        private XSSFWorkbook XlsxWorkBook { get; set; } = null;
        private HSSFWorkbook XlsWorkBook { get; set; } = null;
        public string LastError { get; private set; }
        public bool CheckFile(string fullFileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullFileName))
                {
                    LastError = "Το όνομα του αρχείου είναι κενό!";
                    return false;
                }
                if (!File.Exists(fullFileName))
                {
                    LastError = $"Δε βρέθηκε το αρχείο {fullFileName}.";
                    return false;
                }
                if (!fullFileName.EndsWith(".xls") && !fullFileName.EndsWith(".xlsx") && !fullFileName.EndsWith(".XLSX") && !fullFileName.EndsWith(".XLS"))
                {
                    LastError = "Λανθασμένος τύπος αρχείου! Η διαδικασία υποστηρίζει αρχεία excel.";
                    return false;

                }
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }
        public bool LoadFromFile(string fullFileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullFileName))
                {
                    LastError = "Το όνομα του αρχείου είναι κενό!";
                    return false;
                }

                if (!File.Exists(fullFileName))
                {
                    LastError = $"Δε βρέθηκε το αρχείο {fullFileName}.";
                    return false;
                }



                if (!fullFileName.EndsWith(".xls") && !fullFileName.EndsWith(".xlsx") && !fullFileName.EndsWith(".XLSX") && !fullFileName.EndsWith(".XLS"))
                {
                    LastError = "Λανθασμένος τύπος αρχείου! Η διαδικασία υποστηρίζει αρχεία excel.";
                    return false;

                }
                //11/11/2022
                using (FileStream fs = new FileStream(fullFileName, FileMode.Open))
                {

                    var wb = WorkbookFactory.Create(fs);

                    if (wb is HSSFWorkbook)
                    {
                        XlsWorkBook = (HSSFWorkbook)wb;
                    }
                    else if (wb is XSSFWorkbook)
                    {
                        XlsxWorkBook = (XSSFWorkbook)wb;
                    }
                    else
                    {
                        LastError = "Λανθασμένος τύπος αρχείου!";
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }
        public List<LiRow> ExportExcelData(string sheetName)
        {
            var retData = new List<LiRow>();
            if (XlsWorkBook == null && XlsxWorkBook == null)
            {
                LastError = "Παρακαλώ επιλέξτε πρώτα αρχείο.";
                return retData;
            }
            var sheet = XlsxWorkBook == null ? XlsWorkBook.GetSheet(sheetName) : XlsxWorkBook.GetSheet(sheetName);
            if (sheet == null)
            {
                var activeshhet = XlsxWorkBook != null ? XlsxWorkBook.ActiveSheetIndex : XlsWorkBook.ActiveSheetIndex;
                sheet = XlsxWorkBook != null ? XlsxWorkBook.GetSheetAt(activeshhet) : XlsWorkBook.GetSheetAt(activeshhet);
                //throw new Exception($"Δεν υπάρχει φύλλο εργασίας με όνομα «{sheetName}» στο επιλεγμένο αρχείο προς εισαγωγή.Διορθώστε τη τιμή του πεδίου «Όνομα Φύλλου» με το αντίστοιχο του φύλλου, από το αρχείο που θέλετε να εισάγετε.");
            }
            for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row != null) //null is when the row only contains empty cells 
                {
                    try
                    {
                        var liRow = new LiRow();
                        for (int colIndex = 0; colIndex <= row.LastCellNum; colIndex++)
                        {
                            var cell = row.GetCell(colIndex);

                            if (cell == null || cell.CellType == CellType.Blank || cell.CellType == CellType.Error || cell.CellType == CellType.Formula)
                            {
                                liRow.Columns.Add("");
                            }
                            else if (cell.CellType == CellType.Numeric)
                            {
                                if (HSSFDateUtil.IsCellDateFormatted(cell))
                                {
                                    liRow.Columns.Add(cell.DateCellValue.ToString());
                                }
                                else
                                {
                                    liRow.Columns.Add(cell.NumericCellValue.ToString());
                                }
                            }
                            else
                            {
                                if (cell.StringCellValue == "" || cell.StringCellValue == "NULL")
                                {
                                    liRow.Columns.Add("");
                                }
                                else
                                {
                                    liRow.Columns.Add(cell?.StringCellValue ?? "");
                                }
                                //liRow.Columns.Add(cell?.StringCellValue ?? "");
                            }
                        }
                        if (liRow.Columns.All(x => string.IsNullOrEmpty(x)))
                        {
                            continue;
                        }
                        retData.Add(liRow);
                    }
                    catch (Exception ex)
                    {
                        var error = ex.Message;
                        return null;
                    }
                }
            }
            return retData;
        }
    }
}
