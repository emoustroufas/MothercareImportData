using MothercareImportData.Models;
using MothercareImportData.Services;
using Softone;
using System;
using System.Collections;
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
        private List<RecommendedForRecord> recommendedfor;
        private List<CategoriesData> categories;
        private List<CategoriesPerProduct> categoriesPerProduct;
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
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Attribute ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("attributes");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                excelAttributes = ExcelFileService.GetExcelData<AttributeRecord>(excelData, firstLineInUse);
                if (excelAttributes.Count > 0)
                {
                    var softOneAttributes = softoneService.GetSqlAttributeData(); //softoneService.GetSqlAttributeData();
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
                        logs_remarks = logs_remarks + $"Εισαγωγή Attribute ({result.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateUpdateAttributes(result);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //Attributes per product
                logs_remarks += $"Ανάγνωση Αρχείου Attributes Per Product ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + Environment.NewLine;
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
                    var deleteRecords = new List<ProductAttributeDeleteRecord>();
                    // ==================================================
                    // 1. EXCEL LOOKUPS
                    // ==================================================
                    // Excel: Product + Attribute + Value + Language
                    var excelLookup = excelProductAttributes.ToLookup(x => string.Join("|", (x.ProductCode ?? "").Trim(), (x.AttributeCode ?? "").Trim(), (x.AttributeValueCode ?? "").Trim(), x.LanguageCode), StringComparer.OrdinalIgnoreCase);
                    // Excel: Products
                    var excelProducts = new HashSet<string>(excelProductAttributes.Select(x => (x.ProductCode ?? "").Trim()), StringComparer.OrdinalIgnoreCase);
                    // Excel: Product + Attribute
                    var excelAttributeKeys = new HashSet<string>(excelProductAttributes.Select(x => string.Join("|", (x.ProductCode ?? "").Trim(), (x.AttributeCode ?? "").Trim())), StringComparer.OrdinalIgnoreCase);
                    // Excel: Product + Attribute + Language
                    // Μόνο εγγραφές με περιγραφή
                    var excelTranslationKeys = new HashSet<string>(excelProductAttributes.
                        //Where(x => !string.IsNullOrWhiteSpace(x.FreeText)).
                        Select(x => string.Join("|", (x.ProductCode ?? "").Trim(), (x.AttributeCode ?? "").Trim(), x.LanguageCode)), StringComparer.OrdinalIgnoreCase);
                    // ==================================================
                    // 2. DATABASE LOOKUP
                    // ==================================================
                    var softOneLookup = softOneProductAttributes.ToLookup(x => string.Join("|", (x.ProductCode ?? "").Trim(), (x.AttributeCode ?? "").Trim(), (x.AttributeValueCode ?? "").Trim(), x.LanguageCode), StringComparer.OrdinalIgnoreCase);
                    // ==================================================
                    // 3. INSERT / UPDATE COMPARISON
                    // ==================================================
                    foreach (var excel in excelProductAttributes)
                    {
                        string key = string.Join("|", (excel.ProductCode ?? "").Trim(), (excel.AttributeCode ?? "").Trim(), (excel.AttributeValueCode ?? "").Trim(), excel.LanguageCode);
                        var softOne = softOneLookup[key].FirstOrDefault();
                        // Δεν υπάρχει στη βάση -> INSERT
                        if (softOne == null)
                        {
                            result.Add(excel);
                            continue;
                        }
                        // Υπάρχει αλλά έχει διαφορετική περιγραφή -> UPDATE
                        if (!string.Equals(softOne.FreeText?.Trim(), excel.FreeText?.Trim(), StringComparison.Ordinal))
                        {
                            result.Add(excel);
                        }
                    }
                    // ==================================================
                    // 4. DELETE COMPARISON
                    // ==================================================
                    // HashSet για αποφυγή διπλών διαγραφών
                    var deletedAttributeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var deletedTranslationKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var delcounter = 0;
                    foreach (var dbAttribute in softOneProductAttributes)
                    {
                        delcounter++;
                        string productCode = (dbAttribute.ProductCode ?? "").Trim();
                        string attributeCode = (dbAttribute.AttributeCode ?? "").Trim();
                        // Ελέγχουμε μόνο προϊόντα που υπάρχουν στο Excel
                        if (!excelProducts.Contains(productCode))
                            continue;
                        string attributeKey = string.Join("|", productCode, attributeCode);
                        // ----------------------------------------------
                        // DELETE ΟΛΟΚΛΗΡΟΥ ATTRIBUTE
                        // ----------------------------------------------
                        if (!excelAttributeKeys.Contains(attributeKey))
                        {
                            if (deletedAttributeKeys.Add(attributeKey))
                            {
                                deleteRecords.Add(new ProductAttributeDeleteRecord
                                {
                                    ProductCode = productCode,
                                    AttributeCode = attributeCode,
                                    DeleteAttribute = true
                                });
                            }
                            continue;
                        }
                        // ----------------------------------------------
                        // DELETE ΜΟΝΟ ΤΗΣ ΜΕΤΑΦΡΑΣΗΣ
                        // ----------------------------------------------
                        string translationKey = string.Join("|", productCode, attributeCode, dbAttribute.LanguageCode);
                        if (!excelTranslationKeys.Contains(translationKey) && dbAttribute.FreeText !="")
                        {
                            if (deletedTranslationKeys.Add(translationKey))
                            {
                                deleteRecords.Add(new ProductAttributeDeleteRecord
                                {
                                    ProductCode = productCode,
                                    AttributeCode = attributeCode,
                                    LanguageCode = dbAttribute.LanguageCode,
                                    DeleteAttribute = false
                                });
                            }
                        }
                    }
                    // ==================================================
                    // 5. EXECUTE INSERT / UPDATE
                    // ==================================================
                    if (result.Count > 0)
                    {
                        logs_remarks += $"Εισαγωγή / Ενημέρωση Attributes Per Product ({result.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + Environment.NewLine;
                        var execlog = softoneService.CreateUpdateProductAttributesNew(result, sqlData, softOneAttributes);
                        logs_remarks += execlog + Environment.NewLine;
                    }
                    // ==================================================
                    // 6. EXECUTE DELETE
                    // ==================================================
                    if (deleteRecords.Count > 0)
                    {
                        int deleteAttributes = deleteRecords.Count(x => x.DeleteAttribute);
                        int deleteTranslations = deleteRecords.Count - deleteAttributes;
                        logs_remarks += $"Διαγραφή Attributes Per Product ({deleteAttributes} Attributes, {deleteTranslations} μεταφράσεις) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + Environment.NewLine;
                        var execlog = softoneService.DeleteProductAttributes(deleteRecords, sqlData, softOneAttributes);
                        logs_remarks += execlog + Environment.NewLine;
                    }
                }
                /*
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Attributes Per Product ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
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
                    var softOneLookup = softOneProductAttributes.ToLookup(x => string.Concat(
                        x.ProductCode?.Trim().ToUpperInvariant(), "|",
                        x.AttributeCode?.Trim().ToUpperInvariant(), "|",
                        x.AttributeValueCode?.Trim().ToUpperInvariant(), "|",
                        x.LanguageCode));
                    foreach (var excel in excelProductAttributes)
                    {
                        var key = string.Concat(
                            excel.ProductCode?.Trim().ToUpperInvariant(), "|",
                            excel.AttributeCode?.Trim().ToUpperInvariant(), "|",
                            (excel.AttributeValueCode == null ? "" : excel.AttributeValueCode)?.Trim().ToUpperInvariant(), "|",
                            excel.LanguageCode
                        );
                        var softOne = softOneLookup[key].FirstOrDefault();
                        // Δεν υπάρχει στη βάση
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
                    //DELETE DATA
                    var deleteRecords =new List<ProductAttributeDeleteRecord>();
                    var excelProducts = new HashSet<string>(excelProductAttributes.Select(x => (x.ProductCode ?? "").Trim()), StringComparer.OrdinalIgnoreCase);
                    var excelAttributeKeys = new HashSet<string>(excelProductAttributes.Select(x => string.Concat((x.ProductCode ?? "").Trim(), "|", (x.AttributeCode ?? "").Trim())), StringComparer.OrdinalIgnoreCase);
                    var excelTranslationKeys = new HashSet<string>(excelProductAttributes.Where(x => !string.IsNullOrWhiteSpace(x.FreeText)).
                        Select(x => string.Concat((x.ProductCode ?? "").Trim(), "|", (x.AttributeCode ?? "").Trim(), "|", x.LanguageCode)), StringComparer.OrdinalIgnoreCase);
                    var delcounter = 0;
                    foreach (var dbAttribute in softOneProductAttributes)
                    {
                        delcounter++;
                        string productCode = (dbAttribute.ProductCode ?? "").Trim();
                        string attributeCode = (dbAttribute.AttributeCode ?? "").Trim();
                        // Ελέγχουμε μόνο προϊόντα που υπάρχουν στο Excel
                        if (!excelProducts.Contains(productCode))
                            continue;
                        string attributeKey = string.Concat(productCode, "|", attributeCode);
                        // ============================================
                        // DELETE ΟΛΟΚΛΗΡΟΥ ATTRIBUTE
                        // ============================================
                        if (!excelAttributeKeys.Contains(attributeKey))
                        {
                            // Αποφεύγουμε διπλές εγγραφές διαγραφής
                            bool alreadyExists = deleteRecords.Any(x => x.DeleteAttribute &&
                                string.Equals(x.ProductCode, productCode, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(x.AttributeCode, attributeCode, StringComparison.OrdinalIgnoreCase));
                            if (!alreadyExists)
                            {
                                deleteRecords.Add(new ProductAttributeDeleteRecord
                                {
                                    ProductCode = productCode,
                                    AttributeCode = attributeCode,
                                    DeleteAttribute = true
                                });
                            }
                            continue;
                        }
                        // ============================================
                        // DELETE ΜΟΝΟ ΤΗΣ ΜΕΤΑΦΡΑΣΗΣ
                        // ============================================
                        string translationKey = string.Concat(productCode, "|", attributeCode, "|", dbAttribute.LanguageCode);
                        if (!excelTranslationKeys.Contains(translationKey))
                        {
                            bool alreadyExists = deleteRecords.Any(x =>
                                !x.DeleteAttribute &&
                                string.Equals(x.ProductCode, productCode, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(x.AttributeCode, attributeCode, StringComparison.OrdinalIgnoreCase)
                                && x.LanguageCode == dbAttribute.LanguageCode
                                );
                            if (!alreadyExists)
                            {
                                deleteRecords.Add(new ProductAttributeDeleteRecord
                                {
                                    ProductCode = productCode,
                                    AttributeCode = attributeCode,
                                    LanguageCode = dbAttribute.LanguageCode,
                                    DeleteAttribute = false
                                });
                            }
                        }
                    }
                    if (result.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Attributes Per Product ({result.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.CreateUpdateProductAttributesNew(result, sqlData, softOneAttributes);
                            //softoneService.CreateUpdateProductAttributes(result, sqlData, softOneAttributes);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                    if (deleteRecords.Count > 0)
                    { 
                        logs_remarks = logs_remarks + $"Διαγραφή Attributes Per Product ({result.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.DeleteProductAttributes(deleteRecords, sqlData, softOneAttributes);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                */
                //AddOns
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Add Ons ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("add ons");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                addOns = ExcelFileService.GetExcelData<AddOnRecord>(excelData, firstLineInUse);
                //if (addOns.Count > 0)
                //{
                //    var softOneAddOns = softoneService.GetSqlAddOns();
                //    var differences = addOns.Where(d => !softOneAddOns.Any(s => s.ItemCode.Trim() == d.ItemCode.Trim() && s.AddOnItemCode.Trim() == d.AddOnItemCode.Trim())).ToList();
                //    if (differences.Count > 0)
                //    {
                //        logs_remarks = logs_remarks + $"Εισαγωγή Add Ons ({addOns.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                //        var execlog = softoneService.SetAddOns(differences, item_list);
                //        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                //    }
                //}
                if (addOns.Count > 0)
                {
                    var softOneAddOns = softoneService.GetSqlAddOns();
                    var differences = new List<AddOnChange>();
                    // Excel: ItemCode -> AddOnItemCode
                    var excelGroups = addOns.GroupBy(x => (x.ItemCode ?? "").Trim(),StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(
                            g => g.Key,
                            g => new HashSet<string>(g.Select(x =>$"{(x.AddOnItemCode ?? "").Trim()}"),StringComparer.OrdinalIgnoreCase),StringComparer.OrdinalIgnoreCase);
                    // Database: ItemCode -> AddOnItemCode 
                    var dbGroups = softOneAddOns.GroupBy(x => (x.ItemCode ?? "").Trim(),StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(
                            g => g.Key,
                            g => new HashSet<string>(g.Select(x =>$"{(x.AddOnItemCode ?? "").Trim()}"),StringComparer.OrdinalIgnoreCase),StringComparer.OrdinalIgnoreCase);
                    // Ελέγχουμε όλα τα προϊόντα που υπάρχουν στο Excel
                    foreach (var excelItem in excelGroups)
                    {
                        string itemCode = excelItem.Key;
                        var excelAddOns = excelItem.Value;
                        HashSet<string> dbAddOns;
                        if (!dbGroups.TryGetValue(itemCode, out dbAddOns))
                        {
                            dbAddOns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }
                        // INSERT: Υπάρχει στο Excel, αλλά όχι στη βάση
                        foreach (var excelAddOn in addOns.Where(x =>
                            (x.ItemCode ?? "").Trim().Equals(
                                itemCode, StringComparison.OrdinalIgnoreCase)))
                        {
                            string key =$"{(excelAddOn.AddOnItemCode ?? "").Trim()}";
                            if (!dbAddOns.Contains(key))
                            {
                                differences.Add(new AddOnChange
                                {
                                    ItemCode = itemCode,
                                    AddOnItemCode = excelAddOn.AddOnItemCode,
                                    Action = "INSERT"
                                });
                            }
                        }
                        // DELETE: Υπάρχει στη βάση, αλλά όχι στο Excel
                        foreach (var dbAddOn in softOneAddOns.Where(x =>(x.ItemCode ?? "").Trim().Equals(itemCode, StringComparison.OrdinalIgnoreCase)))
                        {
                            string key =$"{(dbAddOn.AddOnItemCode ?? "").Trim()}";
                            if (!excelAddOns.Contains(key))
                            {
                                differences.Add(new AddOnChange
                                {
                                    ItemCode = itemCode,
                                    AddOnItemCode = dbAddOn.AddOnItemCode,
                                    Action = "DELETE"
                                });
                            }
                        }
                    }
                    if (differences.Count > 0)
                    {
                        logs_remarks += $"Συγχρονισμός Add Ons ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})"+ Environment.NewLine;
                        var execlog = softoneService.SetAddOns(differences, item_list);
                        logs_remarks += execlog + Environment.NewLine;
                    }
                }
                //κείμενα
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Κειμένων ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("κείμενα");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                texts = ExcelFileService.GetExcelData<ItemTextsRecord>(excelData, firstLineInUse);
                if (texts.Count > 0)
                {
                    var softonetexts = softoneService.GetSqlTexts();
                    var differences = new List<ItemTextsRecord>();
                    foreach (var excelItem in texts)
                    {
                        var sqlItem = softonetexts.FirstOrDefault(x => string.Equals(x.ItemCode?.Trim(), excelItem.ItemCode?.Trim(), StringComparison.OrdinalIgnoreCase));
                        // Δεν υπάρχει καθόλου το προϊόν στη βάση
                        if (sqlItem == null)
                        {
                            differences.Add(excelItem);
                            continue;
                        }
                        var differentTranslations = new List<TextsTranslation>();
                        foreach (var excelTranslation in excelItem.Texts)
                        {
                            var sqlTranslation = sqlItem.Texts.FirstOrDefault(x => x.LanguageCode == excelTranslation.LanguageCode);
                            // Δεν υπάρχει η συγκεκριμένη γλώσσα
                            if (sqlTranslation == null)
                            {
                                differentTranslations.Add(excelTranslation);
                                continue;
                            }
                            // Υπάρχει αλλά έχει διαφορετικά δεδομένα
                            if (!string.Equals(sqlTranslation.LabelTitle?.Trim(),excelTranslation.LabelTitle?.Trim(),StringComparison.Ordinal) ||
                                !string.Equals(sqlTranslation.LabelDescription?.Trim(),excelTranslation.LabelDescription?.Trim(),StringComparison.Ordinal) ||
                                !string.Equals(sqlTranslation.EshopTitle?.Trim(),excelTranslation.EshopTitle?.Trim(),StringComparison.Ordinal) ||
                                !string.Equals(sqlTranslation.SmallDescription?.Trim(),excelTranslation.SmallDescription?.Trim(),StringComparison.Ordinal) ||
                                !string.Equals(sqlTranslation.FeaturesAndBenefits?.Trim(),excelTranslation.FeaturesAndBenefits?.Trim(),StringComparison.Ordinal) ||
                                !string.Equals(sqlTranslation.LongDescription?.Trim(),(excelTranslation.LongDescription == null?"": excelTranslation.LongDescription)?.Trim(),StringComparison.Ordinal))
                            {
                                differentTranslations.Add(excelTranslation);
                            }
                        }
                        // Προσθέτουμε το προϊόν μόνο εάν έχει κάποια διαφορά
                        if (differentTranslations.Count > 0)
                        {
                            differences.Add(new ItemTextsRecord
                            {
                                ItemCode = excelItem.ItemCode,
                                Texts = differentTranslations
                            });
                        }
                    }
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Κειμένων ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.SetItemTextsNew(differences, item_list);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //tags
                logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Tags ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                excelData = excelClient.ExportExcelData("tags");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                tags = ExcelFileService.GetExcelData<TagRecord>(excelData, firstLineInUse);
                if (tags.Count > 0)
                {
                    //var softonetags_list = softoneService.GetSqlTags();
                    //logs_remarks = logs_remarks + $"Εισαγωγή Tags ({tags.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                    //var execlog = softoneService.CreateTag(tags, item_list, softonetags_list);
                    //logs_remarks = logs_remarks + execlog + System.Environment.NewLine;

                    var softOneTags = softoneService.GetSqlTags();
                    var differences = new List<TagChange>();
                    // Excel: Tag Code -> ItemCodes
                    var excelGroups = tags.ToDictionary(x => x.Code, x => new HashSet<string>(x.Items?.Select(i => (i.ItemCode ?? "").Trim()) ?? new List<string>(), StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
                    // Database: Tag Code -> ItemCodes
                    var dbGroups = softOneTags.ToDictionary(x => x.Code, x=> new HashSet<string>(x.Items?.Select(i => (i.ItemCode ?? "").Trim()) ?? new List<string>(),StringComparer.OrdinalIgnoreCase),StringComparer.OrdinalIgnoreCase);
                    // Ελέγχουμε όλα τα Tags που υπάρχουν στο Excel
                    foreach (var excelTag in tags)
                    {
                        string tagCode = (excelTag.Code ?? "").Trim();
                        var excelItems = excelGroups[tagCode];
                        HashSet<string> dbItems;
                        if (!dbGroups.TryGetValue(tagCode, out dbItems))
                        {
                            dbItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }
                        // INSERT: Υπάρχει στο Excel, αλλά όχι στη βάση
                        foreach (var excelItem in excelTag.Items ?? new List<TagItems>())
                        {
                            string itemCode = (excelItem.ItemCode ?? "").Trim();
                            if (!dbItems.Contains(itemCode))
                            {
                                differences.Add(new TagChange
                                {
                                    TagCode = tagCode,
                                    ItemCode = itemCode,
                                    Action = "INSERT"
                                });
                            }
                        }
                        // DELETE: Υπάρχει στη βάση, αλλά όχι στο Excel
                        var dbTag = softOneTags.FirstOrDefault(x =>(x.Code ?? "").Trim().Equals(tagCode, StringComparison.OrdinalIgnoreCase));
                        if (dbTag != null)
                        {
                            foreach (var dbItem in dbTag.Items ?? new List<TagItems>())
                            {
                                string itemCode = (dbItem.ItemCode ?? "").Trim();
                                if (!excelItems.Contains(itemCode))
                                {
                                    differences.Add(new TagChange
                                    {
                                        TagCode = tagCode,
                                        ItemCode = itemCode,
                                        Action = "DELETE"
                                    });
                                }
                            }
                        }
                    }
                    if (differences.Count > 0)
                    {
                        var countins = differences.Where(x => x.Action == "INSERT" && x.TagCode == "12085").ToList();
                        var countdel = differences.Where(x => x.Action == "DELETE" && x.TagCode == "12085").ToList();
                        logs_remarks = logs_remarks + $"Εισαγωγή Tags ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.SetTags(differences,tags, item_list);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //nid
                excelData = excelClient.ExportExcelData("nid");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                nids = ExcelFileService.GetExcelData<NidRecord>(excelData, firstLineInUse);
                if (nids.Count > 0)
                {
                    logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου NIds ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
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
                        logs_remarks = logs_remarks + $"Εισαγωγή NIds ({result.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.SetNidNew(result, item_list);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //προτείνεται για
                excelData = excelClient.ExportExcelData("προτείνεται για");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }//recommended for
                recommendedfor = ExcelFileService.GetExcelData<RecommendedForRecord>(excelData, firstLineInUse);
                if (recommendedfor.Count > 0)
                {
                    logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Προτείνεται Για ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                    var softOneRecommendedFor = softoneService.GetRecommendedFor();

                    var differences = new List<RecommendedForChange>();
                    // Excel: ItemCode -> AttributeCodes
                    var excelGroups = recommendedfor.GroupBy(x => x.ItemCode).ToDictionary(g => g.Key,g => new HashSet<string>(g.Select(x => x.AttributeCode)));
                    // Database: ItemCode -> AttributeCodes
                    var dbGroups = softOneRecommendedFor.GroupBy(x => x.ItemCode).ToDictionary(g => g.Key,g => new HashSet<string>(g.Select(x => x.AttributeCode)));
                    // Ελέγχουμε όλα τα είδη που υπάρχουν στο Excel.
                    foreach (var excelItem in excelGroups)
                    {
                        string itemCode = excelItem.Key;
                        HashSet<string> excelAttributes = excelItem.Value;
                        HashSet<string> dbAttributes;
                        if (!dbGroups.TryGetValue(itemCode, out dbAttributes))
                        {
                            dbAttributes = new HashSet<string>();
                        }
                        // INSERT: Υπάρχει στο Excel, αλλά όχι στη βάση.
                        foreach (string attributeCode in excelAttributes)
                        {
                            if (!dbAttributes.Contains(attributeCode))
                            {
                                differences.Add(new RecommendedForChange
                                {
                                    ItemCode = itemCode,
                                    AttributeCode = attributeCode,
                                    Action = "INSERT"
                                });
                            }
                        }
                        // DELETE: Υπάρχει στη βάση, αλλά όχι στο Excel.
                        foreach (string attributeCode in dbAttributes)
                        {
                            if (!excelAttributes.Contains(attributeCode))
                            {
                                differences.Add(new RecommendedForChange
                                {
                                    ItemCode = itemCode,
                                    AttributeCode = attributeCode,
                                    Action = "DELETE"
                                });
                            }
                        }
                    }
                    if (differences.Count > 0)
                    {
                        var softOneAttributes = softoneService.GetSqlAttributeData();
                        logs_remarks = logs_remarks + $"Εισαγωγή Προτείνεται Για ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.SetRecommendedFor(differences, item_list, softOneAttributes);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //e-shop categories
                excelData = excelClient.ExportExcelData("e-shop categories");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                categories = ExcelFileService.GetExcelData<CategoriesData>(excelData, firstLineInUse);
                if (categories.Count > 0)
                {
                    logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Κατηγοριών ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                    var softOneCategories = softoneService.GetSqlCategories();
                    var differences = categories.Where(d => !softOneCategories.Any(s => 
                    s.Ord1 == d.Ord1 && s.Category1Code == (string.IsNullOrWhiteSpace(d.Category1Code) ? "" : d.Category1Code) && s.Category1Description == (string.IsNullOrWhiteSpace(d.Category1Description) ? "" : d.Category1Description) && s.Category1DescriptionEn == (string.IsNullOrWhiteSpace(d.Category1DescriptionEn) ? "" : d.Category1DescriptionEn)
                    && s.Ord2 == d.Ord2 && s.Category2Code == (string.IsNullOrWhiteSpace(d.Category2Code) ? "" : d.Category2Code) && s.Category2Description == (string.IsNullOrWhiteSpace(d.Category2Description) ? "" : d.Category2Description) && s.Category2DescriptionEn == (string.IsNullOrWhiteSpace(d.Category2DescriptionEn) ? "" : d.Category2DescriptionEn)
                    && s.Ord3 == d.Ord3 && s.Category3Code == (string.IsNullOrWhiteSpace(d.Category3Code) ? "" : d.Category3Code) && s.Category3Description == (string.IsNullOrWhiteSpace(d.Category3Description) ? "" : d.Category3Description) && s.Category3DescriptionEn == (string.IsNullOrWhiteSpace(d.Category3DescriptionEn) ? "" : d.Category3DescriptionEn)
                    && s.Ord4 == d.Ord4 && s.Category4Code == (string.IsNullOrWhiteSpace(d.Category4Code) ? "" : d.Category4Code) && s.Category4Description == (string.IsNullOrWhiteSpace(d.Category4Description) ? "" : d.Category4Description)&& s.Category4DescriptionEn == (string.IsNullOrWhiteSpace(d.Category4DescriptionEn) ? "" : d.Category4DescriptionEn)
                    )).ToList();
                    if (differences.Count > 0)
                    {
                        logs_remarks = logs_remarks + $"Εισαγωγή Αρχείου Κατηγοριών ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                        var execlog = softoneService.SetEshopCategories(differences);
                        logs_remarks = logs_remarks + execlog + System.Environment.NewLine;
                    }
                }
                //categories per product
                excelData = excelClient.ExportExcelData("categories per product");
                if (excelData.Count > 0)
                {
                    excelData.RemoveRange(0, numLinesToRemove);
                }
                categoriesPerProduct = ExcelFileService.GetExcelData<CategoriesPerProduct>(excelData, firstLineInUse);
                if (categoriesPerProduct.Count > 0)
                {
                    logs_remarks = logs_remarks + $"Ανάγνωση Αρχείου Κατηγοριών Ανά Προϊόν ({DateTime.Now:dd/MM/yyyy HH:mm:ss})" + System.Environment.NewLine;
                    var softOneCategoriesPerProduct = softoneService.GetSqlCategoriesPerProduct();
                    var differences = new List<CategoriesPerProductChange>();
                    // Excel: ProductCode -> CategoryCode -> IsDefault
                    var excelGroups = categoriesPerProduct.GroupBy(x => x.ProductCode).ToDictionary(g => g.Key,g => g.GroupBy(x => x.CategoryCode).ToDictionary(c => c.Key, c => c.First().IsDefault));
                    // Database: ProductCode -> CategoryCode -> IsDefault
                    var dbGroups = softOneCategoriesPerProduct.GroupBy(x => x.ProductCode).ToDictionary(g => g.Key,g => g.GroupBy(x => x.CategoryCode).ToDictionary(c => c.Key, c => c.First().IsDefault));

                    // Ελέγχουμε όλα τα προϊόντα που υπάρχουν στο Excel.
                    foreach (var excelItem in excelGroups)
                    {
                        string productCode = excelItem.Key;
                        var excelCategories = excelItem.Value;
                        Dictionary<string, int> dbCategories;
                        if (!dbGroups.TryGetValue(productCode, out dbCategories))
                        {
                            dbCategories = new Dictionary<string, int>();
                        }
                        // INSERT ή UPDATE
                        foreach (var excelCategory in excelCategories)
                        {
                            string categoryCode = excelCategory.Key;
                            int isDefault = excelCategory.Value;
                            if (!dbCategories.ContainsKey(categoryCode))
                            {
                                // INSERT
                                differences.Add(new CategoriesPerProductChange
                                {
                                    ProductCode = productCode,
                                    CategoryCode = categoryCode,
                                    IsDefault = isDefault,
                                    Action = "INSERT"
                                });
                            }
                            else if (dbCategories[categoryCode] != isDefault)
                            {
                                // UPDATE
                                differences.Add(new CategoriesPerProductChange
                                {
                                    ProductCode = productCode,
                                    CategoryCode = categoryCode,
                                    IsDefault = isDefault,
                                    Action = "UPDATE"
                                });
                            }
                        }
                        // DELETE
                        foreach (var dbCategory in dbCategories)
                        {
                            if (!excelCategories.ContainsKey(dbCategory.Key))
                            {
                                differences.Add(new CategoriesPerProductChange
                                {
                                    ProductCode = productCode,
                                    CategoryCode = dbCategory.Key,
                                    IsDefault = dbCategory.Value,
                                    Action = "DELETE"
                                });
                            }
                        }
                    }
                    if (differences.Count > 0)
                    {
                        var softOneCategories = softoneService.GetCodeIdCategories();
                        logs_remarks += $"Ενημέρωση Κατηγοριών ανά Προϊόν ({differences.Count} εγγραφές) ({DateTime.Now:dd/MM/yyyy HH:mm:ss})"+ Environment.NewLine;
                        var execlog = softoneService.SetCategoriesPerProduct(differences, item_list, softOneCategories);
                        logs_remarks += execlog + Environment.NewLine;
                    }
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
                                      VALUES ({XSupport.ConnectionInfo.CompanyId},2,'{filePath}','{logs_remarks.Replace("'", "")}','{XSupport.ConnectionInfo.ComputerName}',{XSupport.ConnectionInfo.UserId},'{DateTime.Now:yyyyMMdd HH:mm:ss}');";
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
