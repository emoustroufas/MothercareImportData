using MothercareImportData.Models;
using MothercareImportData.Services;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Softone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData
{
    [WorksOn("CCCVMCIMPDATA")]
    public class McImpData:TXCode
    {
        //SQL Data
        private List<SqlData> sqlData;
        private List<SqlData> item_list;
        private List<SqlData> theme_list;
        private List<SqlData> division_list;
        private List<SqlData> department_list;
        private List<SqlData> subdepartment_list;
        private List<SqlData> class_list;
        private List<SqlData> size_list;
        private List<SqlData> color_list;
        private List<SqlData> brand_list;
        private List<SqlData> intrastat_list;
        private List<SqlData> season_list;
        private List<SqlData> collection_list;
        private List<SqlData> vat_list;
        private List<SqlData> busunit_list;
        private List<SqlData> itemtype_list;
        private List<SqlData> accountingtype_list;
        private List<SqlData> country_list;
        private List<SqlData> supplier_list;
        private List<SqlData> sizeguide_list;
        private List<SqlData> seasonality_list;
        private List<SqlData> house_list;
        private List<SqlData> commercialcollection_list;
        //Excel Data
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
        //private List<StatusRecord> statuses;
        private List<AccountingTypeRecord> accountingTypes;
        private List<SimilarItemRecord> similarItems;       
        private List<SizeGuideRecord> sizeGuides;
        private List<SeasonalityRecord> seasonalities;
        private List<HouseRecord> houses;
        private List<SupBarcodeRecord> supbarcodes;
        private List<SizeRecord> sizes;
        
        public override void Initialize()
        {
            XModule.SetEvent("ON_CCCVMCIMPPARAMS_PATH", On_CccVMCImpParams_Path);
            var softoneTools = new SoftoneTools();
            var softoneService = new SoftoneService(XSupport,XModule);
            sqlData = softoneService.GetSqlData();
            if (sqlData.Count > 0)
            {
                item_list = sqlData.Where(x => x.Obj == "item").ToList();
                theme_list = sqlData.Where(x => x.Obj == "theme").ToList();
                division_list = sqlData.Where(x => x.Obj == "division").ToList();
                department_list = sqlData.Where(x => x.Obj == "department").ToList();
                subdepartment_list = sqlData.Where(x => x.Obj == "subdept").ToList();
                class_list = sqlData.Where(x => x.Obj == "class").ToList();
                size_list = sqlData.Where(x => x.Obj == "size").ToList();
                color_list = sqlData.Where(x => x.Obj == "color").ToList();
                brand_list = sqlData.Where(x => x.Obj == "brand").ToList();
                intrastat_list = sqlData.Where(x => x.Obj == "intrastat").ToList();
                season_list = sqlData.Where(x => x.Obj == "season").ToList();
                collection_list = sqlData.Where(x => x.Obj == "collection").ToList();
                vat_list = sqlData.Where(x => x.Obj == "vat").ToList();
                busunit_list = sqlData.Where(x => x.Obj == "busunit").ToList();
                itemtype_list = sqlData.Where(x => x.Obj == "itemtype").ToList();
                accountingtype_list = sqlData.Where(x => x.Obj == "accountingtype").ToList();
                country_list = sqlData.Where(x => x.Obj == "country").ToList();
                supplier_list = sqlData.Where(x => x.Obj == "supplier").ToList();
                sizeguide_list = sqlData.Where(x => x.Obj == "sizeguide").ToList();
                seasonality_list = sqlData.Where(x => x.Obj == "seasonality").ToList();
                house_list = sqlData.Where(x => x.Obj == "house").ToList();
                commercialcollection_list = sqlData.Where(x => x.Obj == "commercialcollection").ToList();
            }
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
        public override void BeforePost()
        {
            base.BeforePost();
            try
            {
                var impparams = XModule.GetTable("CCCVMCIMPPARAMS");
                var firstLineInUse = Convert.ToInt32(impparams.Current["FIRSTLINEINUSE"]);
                var remarks = impparams.Current["COMMENTS"];
                var sheetname = impparams.Current["SHEETNAME"];
                var datatype = Convert.ToInt32(impparams.Current["DATATYPE"] != DBNull.Value ? impparams.Current["DATATYPE"] : 0);
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
                excelClient.LoadFromFile(filePath);
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου «{fileName}» ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                var excelData = new List<LiRow>();
                var softoneService = new SoftoneService(XSupport,XModule);

                //--------------------------------------------//
                //Division
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Divisions ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("division");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                divisions = ExcelFileService.GetExcelData<DivisionRecord>(excelData, firstLineInUse);
                if (divisions.Count > 0)
                {
                    var differences = divisions.Where(d => !division_list.Any(s => s.Code == d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Division ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog =softoneService.CreateDivision(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Department
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Departments ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("Department");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                departments = ExcelFileService.GetExcelData<DepartmentRecord>(excelData, firstLineInUse);
                if (departments.Count > 0)
                {
                    var differences = departments.Where(d => !department_list.Any(s => s.Code == d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Department ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateDepartment(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Subdepartment
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Subdepartments ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("Subdepartment");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                subdepartments = ExcelFileService.GetExcelData<SubdepartmentRecord>(excelData, firstLineInUse);
                if (subdepartments.Count > 0)
                {
                    var differences = subdepartments.Where(d => !subdepartment_list.Any(s => s.Code == d.DepartmentCode + "-" + d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Subdepartment ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateSubdepartment(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Class
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Class ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("Clas");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                classes = ExcelFileService.GetExcelData<ClassRecord>(excelData, firstLineInUse);
                if (classes.Count > 0)
                {
                    var differences = classes.Where(d => !class_list.Any(s => s.Code == d.DepartmentCode + "-" + d.SubdeptCode + "-" + d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Class ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateClass(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Brand
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Brand ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("Brand");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                brands = ExcelFileService.GetExcelData<BrandRecord>(excelData, firstLineInUse);
                if (brands.Count > 0)
                {
                    var differences = brands.Where(d => !brand_list.Any(s => s.Code == "MC" + d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Brand ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateBrand(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Συλλογή
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Συλλογή ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("Συλλογή"); // th 1268 prepein na thn aferesoume apo thn bash
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                collections = ExcelFileService.GetExcelData<CollectionRecord>(excelData, firstLineInUse);
                if (collections.Count > 0)
                {
                    var differences = collections.Where(d => !collection_list.Any(s => s.Code == d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Συλλογή ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateCollection(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Εμπορική Συλλογή
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Εμπορική Συλλογή ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("Εμπορική Συλλογή");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                commercialCollections = ExcelFileService.GetExcelData<CommercialCollectionRecord>(excelData, firstLineInUse);
                if (commercialCollections.Count > 0)
                {
                    var differences = commercialCollections.Where(d => !commercialcollection_list.Any(s => s.Code == d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Εμπορική Συλλογή ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateCommercialCollection(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //BU
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου BU ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("BU");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                businessUnits = ExcelFileService.GetExcelData<BuRecord>(excelData, firstLineInUse);
                if (businessUnits.Count > 0)
                {
                    var differences = businessUnits.Where(d => !busunit_list.Any(s => s.Code == "MC" + d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή BU ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateBusinessUnit(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Τύπος Είδους
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Τύπος Είδους ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("τύπος είδους");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                itemTypes = ExcelFileService.GetExcelData<ItemTypeRecord>(excelData, firstLineInUse);
                if (itemTypes.Count > 0)
                {
                    var differences = itemTypes.Where(d => !itemtype_list.Any(s => s.Code == "MC" + d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Αρχείου Τύπος Είδους ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateItemType(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Τύπος για Λογιστική
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Τύπος για Λογιστική ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("τύπος για λογιστική");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                accountingTypes = ExcelFileService.GetExcelData<AccountingTypeRecord>(excelData, firstLineInUse);
                if (accountingTypes.Count > 0)
                {
                    var differences = accountingTypes.Where(d => !accountingtype_list.Any(s => s.Code == "MC" + d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Τύπος για Λογιστική ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateAccountingType(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Μεγεθολόγιο
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Μεγεθολόγιο ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("μεγεθολόγιο");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                sizeGuides = ExcelFileService.GetExcelData<SizeGuideRecord>(excelData, firstLineInUse);
                if (sizeGuides.Count > 0)
                {
                    var differences = sizeGuides.Where(d => !sizeguide_list.Any(s => s.Code == d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Μεγεθολόγιο ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateSizeGuide(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Μεγέθη
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Μεγέθη ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("μεγέθη");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                sizes = ExcelFileService.GetExcelData<SizeRecord>(excelData, firstLineInUse);
                if (sizes.Count > 0)
                {
                    var differences = sizes.Where(d => !size_list.Any(s => s.Code == d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Μεγέθη ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateSize(differences, sizeguide_list);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                    var similarities = sizes.Where(d => size_list.Any(s => s.Code == d.Code)).ToList();
                    if(similarities.Count>0)
                    {
                        logs_remarks = logs_remarks + $"Ενημέρωση Μεγέθη ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.UpdateSizeAttributes(sizes,size_list,sizeguide_list);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Εποχικότητα
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Εποχικότητα ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("Εποχικότητα");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                seasonalities = ExcelFileService.GetExcelData<SeasonalityRecord>(excelData, firstLineInUse);
                if (seasonalities.Count > 0)
                {
                    var differences = seasonalities.Where(d => !seasonality_list.Any(s => s.Code == d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Εποχικότητα ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateSeasonality(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Οίκος
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Οίκος ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("Οίκος");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                houses = ExcelFileService.GetExcelData<HouseRecord>(excelData, firstLineInUse);
                if (houses.Count > 0)
                {
                    var differences = houses.Where(d => !house_list.Any(s => s.Code == d.Code && d.Code != "")).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Οίκος ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateHouse(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Αρχείο Ειδών
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Ειδών ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("Αρχείο Ειδών");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                items = ExcelFileService.GetExcelData<ItemMasterRecord>(excelData, firstLineInUse);
                items = items.Where(x => x.ItemType == 1 || x.ItemType == 14).ToList();//ΕΜΠΟΡΕΥΜΑ, SET
                if (items.Count > 0)
                {
                    var newdata = false;
                    // Πρεπει να δημιουργούνται τα Intrastat, Size, Color, House, Vat, Supplier, Themes πριν τα Items
                    var intrastats = items.Select(i => i.Intrastat).Distinct().ToList();
                    if (intrastats.Count > 0)
                    {
                        var differences = intrastats.Where(d => !intrastat_list.Any(s => s.Code == d.Substring(0, (d.Length > 8 ? 8 : d.Length)))).ToList().Where(y=>y !="").ToList();
                        if (differences.Count > 0)
                        {
                            newdata = true;
                            logs_remarks = logs_remarks + $"Δημιουργία Intrastat ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                            var execlogint =  softoneService.CreateIntrastat(differences);
                            logs_remarks = logs_remarks + execlogint + System.Environment.NewLine;
                        }
                    }
                    var themes = items.Select(i => i.StyleNo).Distinct().ToList();
                    if (themes.Count > 0)
                    {
                        var differences = themes.Where(d => !theme_list.Any(s => s.Name == d )).ToList().Where(y => y != "").ToList();
                        if (differences.Count > 0)
                        {
                            newdata = true;
                            logs_remarks = logs_remarks + $"Δημιουργία Θέματος ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                            var execlogth = softoneService.CreateTheme(differences);
                            logs_remarks = logs_remarks + execlogth + System.Environment.NewLine;
                        }
                    }
                    if (newdata)
                    {
                        sqlData = softoneService.GetSqlData();
                    }
                    logs_remarks = logs_remarks + $"Εισαγωγή - Ενημέρωση Ειδών ({items.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                    var execlog = softoneService.CreateUpdateItems(items, sqlData);
                    logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                }

                sqlData = softoneService.GetSqlData();
                item_list = sqlData.Where(x => x.Obj == "item").ToList();

                //EU SALES
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου EU SALES ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("EU SALES");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                var vatPerCountries = ExcelFileService.GetExcelData<VatPerCountry>(excelData, firstLineInUse);
                if (vatPerCountries.Count > 0)
                {
                    var softoneVatPerCountries = softoneService.GetSqlVatPerCountry();
                    var differences = vatPerCountries.Where(d => !softoneVatPerCountries.Any(s => s.CountryCode == d.CountryCode
                    && s.Division == d.Division
                    && s.Department == d.Department
                    && s.Subdepartment == d.Subdepartment
                    && s.Class == d.Class
                    && s.Style == d.Style)).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Αρχείου EU SALES ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.SetVatPerCountry(differences, sqlData);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Barcode Προμηθευτών
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Barcode Προμηθευτών ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("barcode προμηθευτών");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                supbarcodes = ExcelFileService.GetExcelData<SupBarcodeRecord>(excelData, firstLineInUse);
                if (supbarcodes.Count > 0)
                {
                    logs_remarks = logs_remarks + $"Εισαγωγή Barcode Προμηθευτών ({supbarcodes.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                    var execlog = softoneService.UpdateSupBarcodes(supbarcodes);
                    logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                }
                //Όμοια Είδη
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Όμοια Είδη ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("όμοια είδη");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                similarItems = ExcelFileService.GetExcelData<SimilarItemRecord>(excelData, firstLineInUse);
                if (similarItems.Count > 0)
                {
                    var softoneSimilarItems = softoneService.GetSqlSimilarItems();
                    var differences = similarItems.Where(d => !softoneSimilarItems.Any(s => s.ItemCode.Trim() == d.ItemCode.Trim() && s.ReferenceItemCode.Trim() == d.ReferenceItemCode.Trim())).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Αρχείου Όμοια Είδη ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.SetSimilarItems(differences, item_list);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Barcode
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου barcode ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("barcode");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                barcodes = ExcelFileService.GetExcelData<BarcodeRecord>(excelData, firstLineInUse);
                if (barcodes.Count > 0)
                {
                    var softonebarcodes = softoneService.GetSqlBarcodes();
                    var differences = barcodes.Where(d => !softonebarcodes.Any(s => s.Barcode.Trim() == d.Barcode.Trim() && s.ItemCode.Trim() == d.ItemCode.Trim())).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Αρχείου barcode ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.ImportBarcode(differences, item_list);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                    //var itemCodesSet = new HashSet<string>(item_list.Select(s => s.Code));
                    //var existingItemCodes = barcodes
                    //    .Where(d => itemCodesSet.Contains(d.ItemCode))
                    //    .ToList();
                    //if (existingItemCodes.Count > 0)
                    //{
                    //    logs_remarks = logs_remarks + $"Εισαγωγή Αρχείου barcode ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                    //    var execlog = softoneService.ImportBarcode(existingItemCodes, item_list);
                    //    logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    //}
                }
                if (logs_remarks != "")
                {
                    logs_remarks = logs_remarks + $"Ολοκλήρωση Διαδικασίας ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                    impparams.Current["COMMENTS"] = logs_remarks;
                    XModule.OpenSubForm("SFErrorData", 1);
                    //1: Opens the sub form and fires the event “Before show form” 
                    //-1: Closes the sub form and fires the “Accept” event 
                    //-2: Closes the sub form and fires the “Cancel” event
                    var querylog = $@"INSERT INTO CCCMCLOGS (COMPANY,JOB,PATH,REMARKS,COMPUTERNAME,INSUSER,INSDATE)
                                      VALUES ({XSupport.ConnectionInfo.CompanyId},1,'{filePath}','{logs_remarks.Replace("'", "")}','{XSupport.ConnectionInfo.ComputerName}',{XSupport.ConnectionInfo.UserId},'{DateTime.Now:yyyyMMdd HH:mm:ss}');";
                    XSupport.ExecuteSQL(querylog);
                }
            }
            catch (Exception ex)
            {
                XSupport.Exception(ex.Message);
            }
            XSupport.Warning("Τέλος Εργασίας!");
            XModule.CloseForm();
        }
    }
}
