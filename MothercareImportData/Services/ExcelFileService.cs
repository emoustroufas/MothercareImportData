using MothercareImportData.Models;
using MothercareImportData.Utils;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Services
{
    internal class ExcelFileService
    {
        public static List<T>GetExcelData<T>(List<LiRow> rows, int firstLineInUse) where T : new()
        {
            var i = firstLineInUse;
            var result = new List<T>();
            if (typeof(T) == typeof(ItemMasterRecord))
            {
                // Handle ItemMasterRecord specific logic here
                foreach (var row in rows)
                {
                    var item = new ItemMasterRecord();
                    // Map row data to item properties here
                    if (row.Columns.Any())
                    {
                        item.Code = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        item.Name = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        item.TaxCode = FileUtil.InBounds(2, row.Columns) ? (row.Columns.Count > 2 ? row.Columns[2] : "") : "";
                        item.AssortmentDescription = FileUtil.InBounds(3, row.Columns) ? (row.Columns.Count > 3 ? row.Columns[3] : "") : "";
                        item.SupplierCode = FileUtil.InBounds(4, row.Columns) ? (row.Columns.Count > 4 ? row.Columns[4] : "") : "";
                        item.EnglishDescription = FileUtil.InBounds(5, row.Columns) ? (row.Columns.Count > 5 ? row.Columns[5] : "") : "";
                        item.Comments = FileUtil.InBounds(6, row.Columns) ? (row.Columns.Count > 6 ? row.Columns[6] : "") : "";
                        item.UnitOfMeasure = FileUtil.InBounds(7, row.Columns) ? (row.Columns.Count > 7 ? row.Columns[7] : "") : "";
                        item.PackageQuantity = Convert.ToInt32(FileUtil.InBounds(8, row.Columns) ? (row.Columns.Count > 8 ? row.Columns[8] : "0") : "0");
                        item.ComposedOfQuantity = Convert.ToInt32(FileUtil.InBounds(9, row.Columns) ? (row.Columns.Count > 9 ? row.Columns[9] : "0") : "0");
                        item.Division = Convert.ToInt32(FileUtil.InBounds(10, row.Columns) ? (row.Columns.Count > 10 ? row.Columns[10] : "0") : "0");
                        item.Department = Convert.ToInt32(FileUtil.InBounds(11, row.Columns) ? (row.Columns.Count > 11 ? row.Columns[11] : "0") : "0");
                        item.Subdept = Convert.ToInt32(FileUtil.InBounds(12, row.Columns) ? (row.Columns.Count > 12 ? row.Columns[12] : "0") : "0");
                        item.Class = Convert.ToInt32(FileUtil.InBounds(13, row.Columns) ? (row.Columns.Count > 13 ? row.Columns[13] : "0") : "0");
                        item.Year = Convert.ToInt32(FileUtil.InBounds(14, row.Columns) ? (row.Columns.Count > 14 ? row.Columns[14] : "0") : "0");
                        item.Season = Convert.ToInt32(FileUtil.InBounds(15, row.Columns) ? (row.Columns.Count > 15 ? row.Columns[15] : "0") : "0");
                        item.StatisticalYear = Convert.ToInt32(FileUtil.InBounds(16, row.Columns) ? (row.Columns.Count > 16 ? row.Columns[16] : "0") : "0");
                        item.StyleNo = FileUtil.InBounds(17, row.Columns) ? (row.Columns.Count > 17 ? row.Columns[17] : "") : "";
                        item.Size = FileUtil.InBounds(18, row.Columns) ? (row.Columns.Count > 18 ? row.Columns[18] : "") : "";
                        item.Color = FileUtil.InBounds(19, row.Columns) ? (row.Columns.Count > 19 ? row.Columns[19] : "") : "";
                        item.Brand = Convert.ToInt32(FileUtil.InBounds(20, row.Columns) ? (row.Columns.Count > 20 ? row.Columns[20] : "0") : "0");
                        item.House = Convert.ToInt32(FileUtil.InBounds(21, row.Columns) ? (row.Columns.Count > 21 ? row.Columns[21] : "0") : "0");
                        item.VatCategory = Convert.ToInt32(FileUtil.InBounds(22, row.Columns) ? (row.Columns.Count > 22 ? row.Columns[22] : "0") : "0");
                        item.Phase = Convert.ToInt32(FileUtil.InBounds(23, row.Columns) ? (row.Columns.Count > 23 ? row.Columns[23] : "0") : "0");
                        item.Seasonality = FileUtil.InBounds(24, row.Columns) ? (row.Columns.Count > 24 ? row.Columns[24] : "") : "";
                        item.ListUp = Convert.ToInt32(FileUtil.InBounds(25, row.Columns) ? (row.Columns.Count > 25 ? row.Columns[25] : "0") : "0");
                        item.Outlet = Convert.ToInt32(FileUtil.InBounds(26, row.Columns) ? (row.Columns.Count > 26 ? row.Columns[26] : "0") : "0");
                        item.NetWeight = Convert.ToSingle(FileUtil.InBounds(27, row.Columns) ? (row.Columns.Count > 27 ? row.Columns[27] : "0") : "0");
                        item.CountryOfOrigin = FileUtil.InBounds(28, row.Columns) ? (row.Columns.Count > 28 ? row.Columns[28] : "") : "";
                        item.Intrastat = FileUtil.InBounds(29, row.Columns) ? (row.Columns.Count > 29 ? row.Columns[29] : "") : "";
                        item.Status = Convert.ToInt32(FileUtil.InBounds(30, row.Columns) ? (row.Columns.Count > 30 ? row.Columns[30] : "0") : "0");
                        item.Collection = FileUtil.InBounds(31, row.Columns) ? (row.Columns.Count > 31 ? row.Columns[31] : "") : "";
                        item.CommercialCollection = FileUtil.InBounds(32, row.Columns) ? (row.Columns.Count > 32 ? row.Columns[32] : "") : "";
                        item.Bu = FileUtil.InBounds(33, row.Columns) ? (row.Columns.Count > 33 ? row.Columns[33] : "") : "";
                        item.ItemType = Convert.ToInt32(FileUtil.InBounds(34, row.Columns) ? (row.Columns.Count > 34 ? row.Columns[34] : "0") : "0");
                        item.AccountingType = FileUtil.InBounds(35, row.Columns) ? (row.Columns.Count > 35 ? row.Columns[35] : "") : "";
                        item.ImagePath = FileUtil.InBounds(36, row.Columns) ? (row.Columns.Count > 36 ? row.Columns[36] : "") : "";
                        item.RestockWithPackage = Convert.ToInt32(FileUtil.InBounds(37, row.Columns) ? (row.Columns.Count > 37 ? row.Columns[37] : "0") : "0");
                        item.WarrantyMonths = Convert.ToInt32(FileUtil.InBounds(38, row.Columns) ? (row.Columns.Count > 38 ? row.Columns[38] : "0") : "0");
                        item.EshopMasterCode = FileUtil.InBounds(39, row.Columns) ? (row.Columns.Count > 39 ? row.Columns[39] : "") : "";
                        item.Height = (float)Convert.ToDouble(FileUtil.InBounds(40, row.Columns) ? (row.Columns.Count > 40 ? row.Columns[40] : "0.0") : "0.0");
                        item.Length = (float)Convert.ToDouble(FileUtil.InBounds(41, row.Columns) ? (row.Columns.Count > 41 ? row.Columns[41] : "0.0") : "0.0");
                        item.Width = (float)Convert.ToDouble(FileUtil.InBounds(42, row.Columns) ? (row.Columns.Count > 42 ? row.Columns[42] : "0.0") : "0.0");
                        item.ItemCubeM = (float)Convert.ToDouble(FileUtil.InBounds(43, row.Columns) ? (row.Columns.Count > 43 ? row.Columns[43] : "0.0") : "0.0");
                        item.PhotoName = FileUtil.InBounds(44, row.Columns) ? (row.Columns.Count > 44 ? row.Columns[44] : "") : "";
                        item.WorkInProgressInGr = Convert.ToInt32(FileUtil.InBounds(45, row.Columns) ? (row.Columns.Count > 45 ? row.Columns[45] : "0") : "0");
                        item.ToBePublishedInGr = Convert.ToInt32(FileUtil.InBounds(46, row.Columns) ? (row.Columns.Count > 46 ? row.Columns[46] : "0") : "0");
                        item.ToBeUnpublishedInGr = Convert.ToInt32(FileUtil.InBounds(47, row.Columns) ? (row.Columns.Count > 47 ? row.Columns[47] : "0") : "0");
                        item.HasTranslation = Convert.ToInt32(FileUtil.InBounds(48, row.Columns) ? (row.Columns.Count > 48 ? row.Columns[48] : "0") : "0");
                        item.IsPublishedInGr = Convert.ToInt32(FileUtil.InBounds(49, row.Columns) ? (row.Columns.Count > 49 ? row.Columns[49] : "0") : "0");
                        item.ToBePublishedInSkroutz = Convert.ToInt32(FileUtil.InBounds(50, row.Columns) ? (row.Columns.Count > 50 ? row.Columns[50] : "0") : "0");

                        result.Add((T)(object)item);
                    }
                    i++;
                }
            }
            else if (typeof(T) == typeof(BarcodeRecord))
            {
                foreach (var row in rows)
                {
                    var barcode = new BarcodeRecord();
                    if (row.Columns.Any())
                    {
                        barcode.Barcode = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        barcode.ItemCode = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        barcode.ColorCode = FileUtil.InBounds(2, row.Columns) ? (row.Columns.Count > 2 ? row.Columns[2] : "") : "";
                        barcode.SizeCode = FileUtil.InBounds(3, row.Columns) ? (row.Columns.Count > 3 ? row.Columns[3] : "") : "";
                        result.Add((T)(object)barcode);
                    }
                    i++;
                }
            }
            else if (typeof(T) == typeof(DivisionRecord))
            {
                foreach (var row in rows)
                {
                    var division = new DivisionRecord();
                    if (row.Columns.Any())
                    {
                        division.Code = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        division.Description = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        result.Add((T)(object)division);
                    }
                    i++;
                }
            }
            else if (typeof(T) == typeof(DepartmentRecord))
            {
                foreach (var row in rows)
                {
                    var department = new DepartmentRecord();
                    if (row.Columns.Any())
                    {
                        department.Division = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        department.Code = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        department.Description = FileUtil.InBounds(2, row.Columns) ? (row.Columns.Count > 2 ? row.Columns[2] : "") : "";
                        result.Add((T)(object)department);
                    }
                    i++;
                }
            }
            else if (typeof(T) == typeof(SubdepartmentRecord))
            {
                foreach (var row in rows)
                {
                    var subdepartment = new SubdepartmentRecord();
                    if (row.Columns.Any())
                    {
                        subdepartment.Division = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        subdepartment.DepartmentCode = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        subdepartment.Code = FileUtil.InBounds(2, row.Columns) ? (row.Columns.Count > 2 ? row.Columns[2] : "") : "";
                        subdepartment.Description = FileUtil.InBounds(3, row.Columns) ? (row.Columns.Count > 3 ? row.Columns[3] : "") : "";
                        result.Add((T)(object)subdepartment);
                    }
                    i++;
                }

            }
            else if (typeof(T) == typeof(ClassRecord))
            {
                foreach (var row in rows)
                {
                    var classRecord = new ClassRecord();
                    if (row.Columns.Any())
                    {
                        classRecord.Division = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        classRecord.DepartmentCode = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        classRecord.SubdeptCode = FileUtil.InBounds(2, row.Columns) ? (row.Columns.Count > 2 ? row.Columns[2] : "") : "";
                        classRecord.Code = FileUtil.InBounds(3, row.Columns) ? (row.Columns.Count > 3 ? row.Columns[3] : "") : "";
                        classRecord.Description = FileUtil.InBounds(4, row.Columns) ? (row.Columns.Count > 4 ? row.Columns[4] : "") : "";
                        result.Add((T)(object)classRecord);
                    }
                }
            }
            else if (typeof(T) == typeof(BrandRecord))
            {
                foreach (var row in rows)
                {
                    var brand = new BrandRecord();
                    if (row.Columns.Any())
                    {
                        brand.Code = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        brand.Description = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        result.Add((T)(object)brand);
                    }
                }
            }
            else if (typeof(T) == typeof(CollectionRecord))
            {
                foreach (var row in rows)
                {
                    var collection = new CollectionRecord();
                    if (row.Columns.Any())
                    {
                        collection.Code = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        collection.Description = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        result.Add((T)(object)collection);
                    }
                }
            }
            else if (typeof(T) == typeof(CommercialCollectionRecord))
            {
                foreach (var row in rows)
                {
                    var collection = new CommercialCollectionRecord();
                    if (row.Columns.Any())
                    {
                        collection.Code = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        collection.Description = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        result.Add((T)(object)collection);
                    }
                }
            }
            else if (typeof(T) == typeof(BuRecord))
            {
                foreach (var row in rows)
                {
                    var bu = new BuRecord();
                    if (row.Columns.Any())
                    {
                        bu.Code = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        bu.Description = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        result.Add((T)(object)bu);
                    }
                }
            }
            else if (typeof(T) == typeof(ItemTypeRecord))
            {
                foreach (var row in rows)
                {
                    var itemType = new ItemTypeRecord();
                    if (row.Columns.Any())
                    {
                        itemType.Code = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        itemType.Description = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        result.Add((T)(object)itemType);
                    }
                }
            }
            else if (typeof(T) == typeof(StatusRecord))
            {
                foreach (var row in rows)
                {
                    var status = new StatusRecord();
                    if (row.Columns.Any())
                    {
                        status.Code = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        status.Description = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        result.Add((T)(object)status);
                    }
                }
            }
            else if (typeof(T) == typeof(AccountingTypeRecord))
            {
                foreach (var row in rows)
                {
                    var accountingType = new AccountingTypeRecord();
                    if (row.Columns.Any())
                    {
                        accountingType.Code = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        accountingType.Description = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        result.Add((T)(object)accountingType);
                    }

                }
            }
            else if (typeof(T) == typeof(SimilarItemRecord))
            {
                foreach (var row in rows)
                {
                    var similarItem = new SimilarItemRecord();
                    if (row.Columns.Any())
                    {
                        similarItem.ItemCode = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        similarItem.ReferenceItemCode = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        result.Add((T)(object)similarItem);
                    }
                }
            }
            else if (typeof(T) == typeof(AddOnRecord))
            {
                foreach (var row in rows)
                {
                    var addOn = new AddOnRecord();
                    if (row.Columns.Any())
                    {
                        addOn.AddonTypeCode = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        addOn.ItemCode = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        addOn.AddOnItemCode = FileUtil.InBounds(2, row.Columns) ? (row.Columns.Count > 2 ? row.Columns[2] : "") : "";
                        result.Add((T)(object)addOn);
                    }
                }
            }
            else if (typeof(T) == typeof(AttributeRecord))
            {
                foreach (var row in rows)
                {
                    var attribute = new AttributeRecord();
                    if (row.Columns.Any())
                    {
                        attribute.LanguageCode = Convert.ToInt32(FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "0") : "0");
                        attribute.ItemCode = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        attribute.Attribute0Code = FileUtil.InBounds(2, row.Columns) ? (row.Columns.Count > 2 ? row.Columns[2] : "") : "";
                        attribute.Attribute0Description = FileUtil.InBounds(3, row.Columns) ? (row.Columns.Count > 3 ? row.Columns[3] : "") : "";
                        attribute.Attribute1Code = FileUtil.InBounds(4, row.Columns) ? (row.Columns.Count > 4 ? row.Columns[4] : "") : "";
                        attribute.Attribute1Description = FileUtil.InBounds(5, row.Columns) ? (row.Columns.Count > 5 ? row.Columns[5] : "") : "";
                        attribute.FreeText = FileUtil.InBounds(6, row.Columns) ? (row.Columns.Count > 6 ? row.Columns[6] : "") : "";
                        result.Add((T)(object)attribute);
                    }
                }
            }
            else if (typeof(T) == typeof(TagRecord))
            { 
                foreach (var row in rows)
                {
                    var tag = new TagRecord();
                    if (row.Columns.Any())
                    {
                        tag.Code = FileUtil.InBounds(0, row.Columns) ? (row.Columns.Count > 0 ? row.Columns[0] : "") : "";
                        tag.Description = FileUtil.InBounds(1, row.Columns) ? (row.Columns.Count > 1 ? row.Columns[1] : "") : "";
                        tag.ItemCode = FileUtil.InBounds(2, row.Columns) ? (row.Columns.Count > 2 ? row.Columns[2] : "") : "";
                        tag.DateFrom = FileUtil.InBounds(3, row.Columns) ? (row.Columns.Count > 3 ? (DateTime?)Convert.ToDateTime(row.Columns[3]) : null) : null;
                        tag.DateTo = FileUtil.InBounds(4, row.Columns) ? (row.Columns.Count > 4 ? (DateTime?)Convert.ToDateTime(row.Columns[4]) : null) : null;
                        tag.Active = FileUtil.InBounds(5, row.Columns) ? (row.Columns.Count > 5 ? Convert.ToBoolean(row.Columns[5]) : false) : false;
                        result.Add((T)(object)tag);
                    }
                }
            }
                return result;
        }
    }
}
