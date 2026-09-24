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
        private List<AddOnRecord> addOns;
        private List<AttributeRecord> excelAttributes;
        private List<TagRecord> tags;
        private List<SizeGuideRecord> sizeGuides;
        private List<SeasonalityRecord> seasonalities;
        private List<HouseRecord> houses;
        private List<SupBarcodeRecord> supbarcodes;
        public override void Initialize()
        {
            XModule.SetEvent("ON_CCCVMCIMPPARAMS_PATH", On_CccVMCImpParams_Path);
            var softoneTools = new SoftoneTools();
            var softoneService = new SoftoneService(XSupport);
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
                var softoneService = new SoftoneService(XSupport);

                switch (datatype)
                {
                    case 1:
                        excelData = excelClient.ExportExcelData("Αρχείο Ειδών");
                        excelData.RemoveRange(0, numLinesToRemove);
                        items = ExcelFileService.GetExcelData<ItemMasterRecord>(excelData, firstLineInUse);
                        items = items.Where(x => x.ItemType == 1).ToList();//ΕΜΠΟΡΕΥΜΑ
                        if (items.Count > 0)
                        {
                            var newdata = false;
                            // Πρεπει να δημιουργούνται τα Intrastat, Size, Color, House, Vat, Supplier, Themes πριν τα Items
                            var intrastats = items.Select(i => i.Intrastat).Distinct().ToList();
                            if (intrastats.Count > 0)
                            {
                                var differences = intrastats.Where(d => !intrastat_list.Any(s => s.Code == d.Substring(0, (d.Length > 8 ? 8 : d.Length)))).ToList();
                                if (differences.Count > 0)
                                {
                                    newdata = true;
                                    softoneService.CreateIntrastat(differences);
                                }
                            }
                            var themes = items.Select(i => i.StyleNo).Distinct().ToList();
                            if (themes.Count > 0)
                            {
                                var differences = themes.Where(d => !theme_list.Any(s => s.Name == d)).ToList();
                                if (differences.Count > 0)
                                {
                                    newdata = true;
                                    softoneService.CreateTheme(differences);
                                }
                            }
                            if (newdata)
                            {
                                sqlData = softoneService.GetSqlData();
                            }
                            softoneService.CreateUpdateItems(items, sqlData);
                        }
                        break;
                    case 2:
                        excelData = excelClient.ExportExcelData("barcode");
                        excelData.RemoveRange(0, numLinesToRemove);
                        barcodes = ExcelFileService.GetExcelData<BarcodeRecord>(excelData, firstLineInUse);
                        if (barcodes.Count > 0)
                        {
                            var itemCodesSet = new HashSet<string>(item_list.Select(s => s.Code));
                            var existingItemCodes = barcodes
                                .Where(d => itemCodesSet.Contains(d.ItemCode))
                                .ToList();
                            //var test = existingItemCodes.Select(x => x.ItemCode).Distinct().ToList();
                            if (existingItemCodes.Count > 0)
                            {
                                softoneService.ImportBarcode(existingItemCodes, item_list);
                            }
                        }
                        break;
                    case 3:
                        excelData = excelClient.ExportExcelData("division");
                        excelData.RemoveRange(0, numLinesToRemove);
                        divisions = ExcelFileService.GetExcelData<DivisionRecord>(excelData, firstLineInUse);
                        if (divisions.Count > 0)
                        {
                            var differences = divisions.Where(d => !division_list.Any(s => s.Code == d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateDivision(differences);
                            }
                        }
                        break;
                    case 4:
                        excelData = excelClient.ExportExcelData("Department");
                        excelData.RemoveRange(0, numLinesToRemove);
                        departments = ExcelFileService.GetExcelData<DepartmentRecord>(excelData, firstLineInUse);
                        if (departments.Count > 0)
                        {
                            var differences = departments.Where(d => !department_list.Any(s => s.Code == d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateDepartment(differences);
                            }
                        }
                        break;
                    case 5:
                        excelData = excelClient.ExportExcelData("Subdepartment");
                        excelData.RemoveRange(0, numLinesToRemove);
                        subdepartments = ExcelFileService.GetExcelData<SubdepartmentRecord>(excelData, firstLineInUse);
                        if (subdepartments.Count > 0)
                        {
                            var differences = subdepartments.Where(d => !subdepartment_list.Any(s => s.Code == d.DepartmentCode + "-" + d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateSubdepartment(differences);
                            }
                        }
                        break;
                    case 6:
                        excelData = excelClient.ExportExcelData("Clas");
                        excelData.RemoveRange(0, numLinesToRemove);
                        classes = ExcelFileService.GetExcelData<ClassRecord>(excelData, firstLineInUse);
                        if (classes.Count > 0)
                        {
                            var differences = classes.Where(d => !class_list.Any(s => s.Code == d.DepartmentCode + "-" + d.SubdeptCode + "-" + d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateClass(differences);
                            }
                        }
                        break;
                    case 7:
                        excelData = excelClient.ExportExcelData("Brand");
                        excelData.RemoveRange(0, numLinesToRemove);
                        brands = ExcelFileService.GetExcelData<BrandRecord>(excelData, firstLineInUse);
                        if (brands.Count > 0)
                        {
                            var differences = brands.Where(d => !brand_list.Any(s => s.Code == "MC" + d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateBrand(differences);
                            }
                        }
                        break;
                    case 8:
                        excelData = excelClient.ExportExcelData("Συλλογή"); // th 1268 prepein na thn aferesoume apo thn bash
                        excelData.RemoveRange(0, numLinesToRemove);
                        collections = ExcelFileService.GetExcelData<CollectionRecord>(excelData, firstLineInUse);
                        if (collections.Count > 0)
                        {
                            var differences = collections.Where(d => !collection_list.Any(s => s.Code == d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateCollection(differences);
                            }
                        }
                        break;
                    case 9:
                        excelData = excelClient.ExportExcelData("Εμπορική Συλλογή");
                        excelData.RemoveRange(0, numLinesToRemove);
                        commercialCollections = ExcelFileService.GetExcelData<CommercialCollectionRecord>(excelData, firstLineInUse);
                        if (commercialCollections.Count > 0)
                        {
                            var differences = commercialCollections.Where(d => !commercialcollection_list.Any(s => s.Code == d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateCommercialCollection(differences);
                            }
                        }
                        break;
                    case 10:
                        excelData = excelClient.ExportExcelData("BU");
                        excelData.RemoveRange(0, numLinesToRemove);
                        businessUnits = ExcelFileService.GetExcelData<BuRecord>(excelData, firstLineInUse);
                        if (businessUnits.Count > 0)
                        {
                            var differences = businessUnits.Where(d => !busunit_list.Any(s => s.Code == "MC" + d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateBusinessUnit(differences);
                            }
                        }
                        break;
                    case 11:
                        excelData = excelClient.ExportExcelData("τύπος είδους");
                        excelData.RemoveRange(0, numLinesToRemove);
                        itemTypes = ExcelFileService.GetExcelData<ItemTypeRecord>(excelData, firstLineInUse);
                        if (itemTypes.Count > 0)
                        {
                            var differences = itemTypes.Where(d => !itemtype_list.Any(s => s.Code == "MC" + d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateItemType(differences);
                            }
                        }
                        break;
                    case 13:
                        excelData = excelClient.ExportExcelData("τύπος για λογιστική");
                        excelData.RemoveRange(0, numLinesToRemove);
                        accountingTypes = ExcelFileService.GetExcelData<AccountingTypeRecord>(excelData, firstLineInUse);
                        if (accountingTypes.Count > 0)
                        {
                            var differences = accountingTypes.Where(d => !accountingtype_list.Any(s => s.Code == "MC" + d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateAccountingType(differences);
                            }
                        }
                        break;
                    case 14:
                        excelData = excelClient.ExportExcelData("όμοια είδη");
                        excelData.RemoveRange(0, numLinesToRemove);
                        similarItems = ExcelFileService.GetExcelData<SimilarItemRecord>(excelData, firstLineInUse);
                        if (similarItems.Count > 0)
                        {
                            softoneService.SetSimilarItems(similarItems, item_list);
                        }
                        break;
                    //case 15:
                    //    excelData = excelClient.ExportExcelData("add ons");
                    //    excelData.RemoveRange(0, numLinesToRemove);
                    //    addOns = ExcelFileService.GetExcelData<AddOnRecord>(excelData, firstLineInUse);
                    //    break;

                    //case 16:
                    //    excelData = excelClient.ExportExcelData("attributes");
                    //    excelData.RemoveRange(0, numLinesToRemove);
                    //    attributes = ExcelFileService.GetExcelData<AttributeRecord>(excelData, firstLineInUse);
                    //    if (attributes.Count > 0)
                    //    {
                    //        var sqlAttributes = softoneService.GetSqlAttributeData();
                    //        softoneService.CreateUpdateAttributes(attributes, sqlAttributes);

                    //    }
                    //    break;
                    //case 17:
                    //    excelData = excelClient.ExportExcelData("tags");
                    //    excelData.RemoveRange(0, numLinesToRemove);
                    //    tags = ExcelFileService.GetExcelData<TagRecord>(excelData, firstLineInUse);
                    //break;
                    case 18:
                        excelData = excelClient.ExportExcelData("μεγεθολόγιο");
                        excelData.RemoveRange(0, numLinesToRemove);
                        sizeGuides = ExcelFileService.GetExcelData<SizeGuideRecord>(excelData, firstLineInUse);
                        if (sizeGuides.Count > 0)
                        {
                            var differences = sizeGuides.Where(d => !sizeguide_list.Any(s => s.Code == d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateSizeGuide(differences);
                            }
                        }
                        break;
                    case 19:
                        excelData = excelClient.ExportExcelData("Εποχικότητα");
                        excelData.RemoveRange(0, numLinesToRemove);
                        seasonalities = ExcelFileService.GetExcelData<SeasonalityRecord>(excelData, firstLineInUse);
                        if (seasonalities.Count > 0)
                        {
                            var differences = seasonalities.Where(d => !seasonality_list.Any(s => s.Code == d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateSeasonality(differences);
                            }
                        }
                        break;
                    case 20:
                        excelData = excelClient.ExportExcelData("Οίκος");
                        excelData.RemoveRange(0, numLinesToRemove);
                        houses = ExcelFileService.GetExcelData<HouseRecord>(excelData, firstLineInUse);
                        if (houses.Count > 0)
                        {
                            var differences = houses.Where(d => !house_list.Any(s => s.Code == d.Code)).ToList();
                            if (differences.Count > 0)
                            {
                                softoneService.CreateHouse(differences);
                            }
                        }
                        break;
                    case 21:
                        excelData = excelClient.ExportExcelData("barcode προμηθευτών");
                        excelData.RemoveRange(0, numLinesToRemove);
                        supbarcodes = ExcelFileService.GetExcelData<SupBarcodeRecord>(excelData, firstLineInUse);
                        if (supbarcodes.Count > 0)
                        {
                            softoneService.UpdateSupBarcodes(supbarcodes);
                        }
                        break;
                }

                //Attributes, Tags, AddOns, SimilarItems δεν χρειάζονται να δημιουργούνται πριν τα Items γιατί δεν έχουν κωδικό είδους για να συνδεθούν. Θα δημιουργούνται μετά τα Items.
                //Attributes
                excelData = excelClient.ExportExcelData("attributes");
                excelData.RemoveRange(0, numLinesToRemove);
                excelAttributes = ExcelFileService.GetExcelData<AttributeRecord>(excelData, firstLineInUse);
                if (excelAttributes.Count > 0)
                {
                    var softOneAttributes = softoneService.GetSqlAttributeData();
                    var result = new List<AttributeRecord>();
                    foreach (var excelAttribute in excelAttributes)
                    {
                        var softAttribute = softOneAttributes.FirstOrDefault(x =>string.Equals(x.Code, excelAttribute.Code,StringComparison.OrdinalIgnoreCase));
                        // ==========================================
                        // 1. Το Attribute δεν υπάρχει στο SoftOne
                        // ==========================================
                        if (softAttribute == null)
                        {
                            result.Add(excelAttribute);
                            continue;
                        }
                        // ==========================================
                        // 2. Το Attribute υπάρχει
                        //    Δημιουργούμε record μόνο με τις διαφορές
                        // ==========================================
                        var difference = new AttributeRecord
                        {
                            Code = excelAttribute.Code,
                            SoftOneId = softAttribute.SoftOneId
                        };
                        // ==========================================
                        // 3. Σύγκριση Attribute Translations
                        // ==========================================
                        foreach (var excelTranslation in excelAttribute.Translations)
                        {
                            var softTranslation = softAttribute.Translations.FirstOrDefault(x =>x.LanguageCode == excelTranslation.LanguageCode);
                            if (softTranslation == null ||
                                !string.Equals(
                                    softTranslation.Description,
                                    excelTranslation.Description,
                                    StringComparison.Ordinal))
                            {
                                difference.Translations.Add(excelTranslation);
                            }
                        }
                        // ==========================================
                        // 4. Σύγκριση Attribute Values
                        // ==========================================
                        foreach (var excelValue in excelAttribute.Values)
                        {
                            var softValue = softAttribute.Values.FirstOrDefault(x =>string.Equals(x.Code, excelValue.Code,StringComparison.OrdinalIgnoreCase));
                            // Νέα τιμή
                            if (softValue == null)
                            {
                                difference.Values.Add(excelValue);
                                continue;
                            }
                            // ==========================================
                            // 5. Η τιμή υπάρχει - σύγκριση translations
                            // ==========================================
                            var valueDifference = new AttributeValue
                            {
                                Code = excelValue.Code,
                                SoftOneId = softValue.SoftOneId
                            };
                            foreach (var excelValueTranslation in excelValue.Translations)
                            {
                                var softValueTranslation = softValue.Translations.FirstOrDefault(x => x.LanguageCode == excelValueTranslation.LanguageCode);
                                if (softValueTranslation == null || !string.Equals(softValueTranslation.Description,excelValueTranslation.Description,StringComparison.Ordinal))
                                {
                                    valueDifference.Translations.Add(
                                        excelValueTranslation);
                                }
                            }
                            // Κρατάμε το Value μόνο αν έχει κάποια διαφορά
                            if (valueDifference.Translations.Count > 0)
                            {
                                difference.Values.Add(valueDifference);
                            }
                        }
                        // ==========================================
                        // 6. Κρατάμε το Attribute μόνο αν έχει διαφορά
                        // ==========================================
                        if (difference.Translations.Count > 0 ||
                            difference.Values.Count > 0)
                        {
                            result.Add(difference);
                        }
                    }
                    if(result.Count > 0)
                    {
                        softoneService.CreateUpdateAttributes(result);
                    }
                }
                XSupport.Warning("Τέλος Εργασίας!");
                XModule.CloseForm();
            }
            catch (Exception ex)
            {
                XSupport.Exception(ex.Message);
            }
        }
    }

}
