using MothercareImportData.Models;
using MothercareImportData.Services;
using Softone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData
{
    [WorksOn("CCCVMCIMPESHOP")]
    public class McImpDataEshop : TXCode
    {
        //SQL Data
        private List<SqlData> sqlData;
        private List<SqlData> item_list;
        private List<SqlData> theme_list;
        //Excel Data
        private List<AttributeRecord> excelAttributes;
        private List<AddOnRecord> addOns;
        private List<TagRecord> tags;
        private List<ProductAttributeRecord> excelProductAttributes;
        private List<ItemTextsRecord> texts;
        private List<NidRecord> nids;
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

                //Eshop Data
                //Attributes, Tags, AddOns, SimilarItems δεν χρειάζονται να δημιουργούνται πριν τα Items γιατί δεν έχουν κωδικό είδους για να συνδεθούν. Θα δημιουργούνται μετά τα Items.
                //Attributes
                excelData = excelClient.ExportExcelData("attributes");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                excelAttributes = ExcelFileService.GetExcelData<AttributeRecord>(excelData, firstLineInUse);
                if (excelAttributes.Count > 0)
                {
                    var softOneAttributes = softoneService.GetSqlAttributeData2(); //softoneService.GetSqlAttributeData();
                    var result = new List<AttributeRecord>();
                    foreach (var excelAttribute in excelAttributes)
                    {
                        var softAttribute = softOneAttributes.FirstOrDefault(x => string.Equals(x.Code, excelAttribute.Code, StringComparison.OrdinalIgnoreCase));
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
                            var softTranslation = softAttribute.Translations.FirstOrDefault(x => x.LanguageCode == excelTranslation.LanguageCode);
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
                            var softValue = softAttribute.Values.FirstOrDefault(x => string.Equals(x.Code, excelValue.Code, StringComparison.OrdinalIgnoreCase));
                            // Νέα τιμή
                            if (softValue == null) 
                            {
                                if (excelValue.Code != "")// evala to excelValue.Code !=""
                                {
                                    difference.Values.Add(excelValue);
                                }
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
                                if (softValueTranslation == null || !string.Equals(softValueTranslation.Description, excelValueTranslation.Description, StringComparison.Ordinal))
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
                    if (result.Count > 0)
                    {
                        softoneService.CreateUpdateAttributes(result);
                    }
                }
                //Attributes per product
                excelData = excelClient.ExportExcelData("attributes per product");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                excelProductAttributes = ExcelFileService.GetExcelData<ProductAttributeRecord>(excelData, firstLineInUse);
                if (excelProductAttributes.Count > 0)
                {
                    var softOneAttributes = softoneService.GetSqlAttributeData();
                    var softOneProductAttributes = softoneService.GetSqlProductAttributeData();
                    var result = new List<ProductAttributeRecord>();
                    foreach (var excel in excelProductAttributes)
                    {
                        var softOne = softOneProductAttributes.FirstOrDefault(x =>
                            string.Equals(x.ProductCode, excel.ProductCode, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.AttributeCode, excel.AttributeCode, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.AttributeValueCode, excel.AttributeValueCode, StringComparison.OrdinalIgnoreCase) &&
                            x.LanguageCode == excel.LanguageCode
                        );

                        // Δεν υπάρχει καθόλου στη βάση
                        if (softOne == null)
                        {
                            result.Add(excel);
                            continue;
                        }

                        // Υπάρχει αλλά έχει διαφορετική περιγραφή
                        if (!string.Equals(
                                softOne.FreeText?.Trim(),
                                excel.FreeText?.Trim(),
                                StringComparison.Ordinal))
                        {
                            result.Add(excel);
                        }
                    }
                    if (result.Count > 0)
                    {
                        softoneService.CreateUpdateProductAttributes(result, sqlData, softOneAttributes);
                    }
                }
                //AddOns
                excelData = excelClient.ExportExcelData("add ons");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                addOns = ExcelFileService.GetExcelData<AddOnRecord>(excelData, firstLineInUse);
                if (addOns.Count > 0)
                {
                    softoneService.SetAddOns(addOns, item_list);
                }
                //κείμενα
                excelData = excelClient.ExportExcelData("κείμενα");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                texts = ExcelFileService.GetExcelData<ItemTextsRecord>(excelData, firstLineInUse);
                if (texts.Count > 0)
                {
                    softoneService.SetItemTexts(texts, item_list);
                }
                //tags
                excelData = excelClient.ExportExcelData("tags");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                tags = ExcelFileService.GetExcelData<TagRecord>(excelData, firstLineInUse);
                if (tags.Count > 0)
                {
                    softoneService.CreateTag(tags, item_list);
                }
                //nid
                excelData = excelClient.ExportExcelData("nid");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                var nids = ExcelFileService.GetExcelData<NidRecord>(excelData, firstLineInUse);
                if (nids.Count > 0)
                {
                    //GetNids
                    var softOneNids = softoneService.GetNids();
                    var result = new List<NidRecord>();
                    foreach (var excel in nids)
                    {
                        var softOne = softOneNids.FirstOrDefault(x => string.Equals(x.ItemCode, excel.ItemCode, StringComparison.OrdinalIgnoreCase));
                        // Δεν υπάρχει καθόλου στη βάση
                        if (softOne == null)
                        {
                            result.Add(excel);
                            continue;
                        }
                        // Υπάρχει αλλά έχει διαφορετικά καταστήματα
                        var excelStores = excel.NidStores.Select(x => x.StoreId).ToList();
                        var softOneStores = softOne.NidStores.Select(x => x.StoreId).ToList();
                        if (!excelStores.SequenceEqual(softOneStores))
                        {
                            result.Add(excel);
                        }
                    }
                    if (result.Count > 0)
                    {
                        softoneService.SetNid(result, item_list);
                    }
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
