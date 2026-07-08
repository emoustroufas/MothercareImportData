using MothercareImportData.Models;
using MothercareImportData.Services;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Softone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData
{
    [WorksOn("CCCVMCIMPDATA")]
    public class McImpData:TXCode
    {
        private List<ItemMasterRecord> items;
        private List<BarcodeRecord> barcodes;
        private List<DivisionRecord> divisions;
        private List<DepartmentRecord> departments;
        private List<SubdepartmentRecord> subdepartments;
        private List<ClassRecord> classes;
        private List<BrandRecord> brands;
        private List<CollectionRecord> collections;
        private List<CommercialCollectionRecord> commercialCollections;
        private List<BuRecord> businessUnits;
        private List<ItemTypeRecord> itemTypes;
        private List<StatusRecord> statuses;
        private List<AccountingTypeRecord> accountingTypes;
        private List<SimilarItemRecord> similarItems;
        private List<AddOnRecord> addOns;
        private List<AttributeRecord> attributes;
        private List<TagRecord> tags;
        public override void Initialize()
        {
            var softonetools = new SoftoneTools();
            XModule.SetEvent("ON_CCCVMCIMPPARAMS_PATH", On_CccVMCImpParams_Path);
            base.Initialize();
        }
        private void On_CccVMCImpParams_Path(object Sender, XEventArgs e)//Επιλογή αρχείου και έλεγχος ορθότητας path.
        {
            var impparams = XModule.GetTable("CCCVMCIMPPARAMS");
            try
            {
                var fileName = "";
                fileName = Convert.ToString(impparams.Current["PATH"]);
                if (fileName != "")
                {
                    var excelClient = new ExcelEditor();
                    var correctfile = excelClient.CheckFile(fileName);
                    if (!correctfile)
                    {
                        XSupport.Warning(excelClient.LastError);
                        impparams.Current["PATH"] = "";
                    }
                }
                impparams = XModule.GetTable("CCCVMCIMPPARAMS");
                impparams.Resync();
            }
            catch (Exception ex)
            {
                XSupport.Exception(ex.Message);
            }
        }
        public void MarkStage(int number)
        {
            var results = XModule.GetTable("RESULTS");
            //results.Current.Edit(0);
            results.Current["STAGE"] = number;
            results.Resync();
            XModule.Exec("CODE:ModuleIntf.SENDRESPONSE", XModule.Handle, 0, results.TablePtr);
            results.Current["STAGE"] = 4; //Χρειάζεται ένα στάδιο που δεν υπάρχει για να μην ξαναγράφει
        }
        public void ProgressNotify(int prmode, int prvalue)
        {
            var results = XModule.GetTable("RESULTS");
            //results.Current.Edit(0);
            switch (prmode)
            {
                case 0:
                    results.Current["STARTSTOP"] = prvalue;
                    break;
                case 1:
                    results.Current["STARTSTOP"] = prvalue;
                    break;
                case 2:
                    results.Current["TOTREC"] = prvalue;
                    break;
                case 3:
                    results.Current["CURREC"] = prvalue;
                    break;
            }
            results.Resync();
            XModule.Exec("CODE:ModuleIntf.SENDRESPONSE", XModule.Handle, 0, results.TablePtr);
            //X.EXEC('CODE:ModuleIntf.SENDRESPONSE', X.MODULE, 0, RESULTS);
            results.Current["STAGE"] = 4;
        }
        public override void BeforePost()
        {
            base.BeforePost();
            try
            {
                var impparams = XModule.GetTable("CCCVMCIMPPARAMS");
                var firstLineInUse = Convert.ToInt32(impparams.Current["FIRSTLINEINUSE"]);
                var remarks = impparams.Current["COMMENTS"];
                var sheetname = impparams.Current["SHEETNAME"];
                var datatype = impparams.Current["DATATYPE"];
                var filePath = Convert.ToString(impparams.Current["PATH"]);
                var fileName = Path.GetFileName(filePath);
                var numLinesToRemove = 0;

                //logs - Παρατηρήσεις Εργασίας
                var logs_remarks = "";
                logs_remarks = logs_remarks + $"Έναρξη Διαδικασίας ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                if (firstLineInUse == 0)
                {
                    numLinesToRemove = 0;
                }
                else
                {
                    numLinesToRemove = (firstLineInUse - 1);
                }
                var excelClient = new ExcelEditor();
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου «{fileName}» ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                var excelData = new List<LiRow>();

                switch (datatype)
                {
                    case 1:
                        excelData = excelClient.ExportExcelData("Αρχείο Ειδών");
                        excelData.RemoveRange(0, numLinesToRemove);
                        items = ExcelFileService.GetExcelData<ItemMasterRecord>(excelData, firstLineInUse);
                        break;  
                    case 2:
                        excelData = excelClient.ExportExcelData("barcode");
                        excelData.RemoveRange(0, numLinesToRemove);
                        barcodes = ExcelFileService.GetExcelData<BarcodeRecord>(excelData, firstLineInUse);
                        break;
                    case 3:
                        excelData = excelClient.ExportExcelData("division");
                        excelData.RemoveRange(0, numLinesToRemove);
                        divisions = ExcelFileService.GetExcelData<DivisionRecord>(excelData, firstLineInUse);
                        break;
                    case 4:
                        excelData = excelClient.ExportExcelData("Department");
                        excelData.RemoveRange(0, numLinesToRemove);
                        departments = ExcelFileService.GetExcelData<DepartmentRecord>(excelData, firstLineInUse);
                        break;
                    case 5:
                        excelData = excelClient.ExportExcelData("Subdepartment");
                        excelData.RemoveRange(0, numLinesToRemove);
                        subdepartments = ExcelFileService.GetExcelData<SubdepartmentRecord>(excelData, firstLineInUse);
                        break;
                    case 6:
                        excelData = excelClient.ExportExcelData("Clas");
                        excelData.RemoveRange(0, numLinesToRemove);
                        classes = ExcelFileService.GetExcelData<ClassRecord>(excelData, firstLineInUse);
                        break;
                    case 7:
                        excelData = excelClient.ExportExcelData("Brand");
                        excelData.RemoveRange(0, numLinesToRemove);
                        brands = ExcelFileService.GetExcelData<BrandRecord>(excelData, firstLineInUse);
                        break;
                    case 8:
                        excelData = excelClient.ExportExcelData("Συλλογή");
                        excelData.RemoveRange(0, numLinesToRemove);
                        collections = ExcelFileService.GetExcelData<CollectionRecord>(excelData, firstLineInUse);
                        break;
                    case 9:
                        excelData = excelClient.ExportExcelData("Εμπορική Συλλογή");
                        excelData.RemoveRange(0, numLinesToRemove);
                        commercialCollections = ExcelFileService.GetExcelData<CommercialCollectionRecord>(excelData, firstLineInUse);
                        break;
                    case 10:
                        excelData = excelClient.ExportExcelData("BU");
                        excelData.RemoveRange(0, numLinesToRemove);
                        businessUnits = ExcelFileService.GetExcelData<BuRecord>(excelData, firstLineInUse);
                        break;
                    case 11:
                        excelData = excelClient.ExportExcelData("τύπος είδους");
                        excelData.RemoveRange(0, numLinesToRemove);
                        itemTypes = ExcelFileService.GetExcelData<ItemTypeRecord>(excelData, firstLineInUse);
                        break;
                    case 12:
                        excelData = excelClient.ExportExcelData("status");
                        excelData.RemoveRange(0, numLinesToRemove);
                        statuses = ExcelFileService.GetExcelData<StatusRecord>(excelData, firstLineInUse);
                        break;
                    case 13:
                        excelData = excelClient.ExportExcelData("τύπος για λογιστική");
                        excelData.RemoveRange(0, numLinesToRemove);
                        accountingTypes = ExcelFileService.GetExcelData<AccountingTypeRecord>(excelData, firstLineInUse);
                        break;
                    case 14:
                        excelData = excelClient.ExportExcelData("όμοια είδη");
                        excelData.RemoveRange(0, numLinesToRemove);
                        similarItems = ExcelFileService.GetExcelData<SimilarItemRecord>(excelData, firstLineInUse);
                        break;
                    case 15:
                        excelData = excelClient.ExportExcelData("add ons");
                        excelData.RemoveRange(0, numLinesToRemove);
                        addOns = ExcelFileService.GetExcelData<AddOnRecord>(excelData, firstLineInUse);
                        break;
                    case 16:
                        excelData = excelClient.ExportExcelData("attributes");
                        excelData.RemoveRange(0, numLinesToRemove);
                        attributes = ExcelFileService.GetExcelData<AttributeRecord>(excelData, firstLineInUse);
                        break;
                    case 17:
                        excelData = excelClient.ExportExcelData("tags");
                        excelData.RemoveRange(0, numLinesToRemove);
                        tags = ExcelFileService.GetExcelData<TagRecord>(excelData, firstLineInUse);
                        break;
                }

            }
            catch (Exception ex)
            {
                XSupport.Exception(ex.Message);
            }
        }
    }

}
