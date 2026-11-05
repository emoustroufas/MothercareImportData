using MothercareImportData.Models;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using Softone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData
{
    [WorksOn("CCCMCECATEGORY")]
    public class McECategory : TXCode
    {
        public override object ExecCommand(int Cmd)
        {
            if (Cmd == 20261028) // Άνοιγμα της φόρμας για την εισαγωγή Κατηγοριών
            {
                XModule.SetProperty("MERGECHANGELOG", true);
                XModule.OpenSubForm("SfCategories", 1);
                XModule.CloseSubForm("SfCategories");
                XModule.SetProperty("MERGECHANGELOG", false);
                //1: Opens the sub form and fires the event “Before show form” 
                //-1: Closes the sub form and fires the “Accept” event 
                //-2: Closes the sub form and fires the “Cancel” event
            }
            else if (Cmd == 20261027) // Εκτέλεση της εντολής για την εισαγωγή Κατηγοριών
            {
                var vcatimp = XModule.GetTable("CCCVECATIMP");
                if (vcatimp != null)
                {
                    var categoriesData = new List<CategoriesData>();
                    for (int i = 0; i < vcatimp.Count; i++)
                    {
                        var catdata = new CategoriesData
                        {
                            Category1Code = vcatimp.GetAsString(i, "SPOMAD1CD"),
                            Category1Description = vcatimp.GetAsString(i, "SPOMAD1PERI"),
                            Category2Code = vcatimp.GetAsString(i, "SPOMAD2CD"),
                            Category2Description = vcatimp.GetAsString(i, "SPOMAD2PERI"),
                            Category3Code = vcatimp.GetAsString(i, "SPOMAD3CD"),
                            Category3Description = vcatimp.GetAsString(i, "SPOMAD3PERI"),
                            Category4Code = vcatimp.GetAsString(i, "SPOMAD4CD"),
                            Category4Description = vcatimp.GetAsString(i, "SPOMAD4PERI")
                        };
                        categoriesData.Add(catdata);
                    }
                    var sqlcategoriesData = GetSQLCategoriesData();
                    var newCategoriesData = categoriesData.Where(c => !sqlcategoriesData.Any(s => s.Category1Code == c.Category1Code
                    && s.Category2Code == c.Category2Code
                    && s.Category3Code == c.Category3Code
                    && s.Category4Code == c.Category4Code)).ToList();
                    var logs_remarks = "";
                    if (newCategoriesData.Any())
                    {
                        foreach (var newCategory in newCategoriesData)
                        {
                            var sqlcategorydata = GetCodeIdCategories();
                            var cat1code = newCategory.Category1Code;
                            var cat2code = newCategory.Category2Code;
                            var cat3code = newCategory.Category3Code;
                            var cat4code = newCategory.Category4Code;

                            var diff1 = sqlcategorydata.Where(c => c.Code == cat1code && c.CatLevel == 1).FirstOrDefault();
                            if (diff1 == null)
                            {
                                logs_remarks = CreateMcECategories(newCategory.Category1Code, newCategory.Category1Description, 1, 0);
                                sqlcategorydata = GetCodeIdCategories();
                            }
                            var diff2 = sqlcategorydata.Where(c => c.Code == cat2code && c.CatLevel == 2).FirstOrDefault();
                            if (diff2 == null)
                            {
                                var parentid = sqlcategorydata.Where(c => c.Code == cat1code && c.CatLevel == 1).FirstOrDefault()?.Id ?? 0;
                                logs_remarks = CreateMcECategories(newCategory.Category2Code, newCategory.Category2Description, 2, parentid);
                                sqlcategorydata = GetCodeIdCategories();
                            }
                            var diff3 = sqlcategorydata.Where(c => c.Code == cat3code && c.CatLevel == 3).FirstOrDefault();
                            if (diff3 == null)
                            {
                                var parentid = sqlcategorydata.Where(c => c.Code == cat2code && c.CatLevel == 2).FirstOrDefault()?.Id ?? 0;
                                logs_remarks = CreateMcECategories(newCategory.Category3Code, newCategory.Category3Description, 3, parentid);
                                sqlcategorydata = GetCodeIdCategories();
                            }
                            var diff4 = sqlcategorydata.Where(c => c.Code == cat4code && c.CatLevel == 4).FirstOrDefault();
                            if (diff4 == null)
                            {
                                var parentid = sqlcategorydata.Where(c => c.Code == cat3code && c.CatLevel == 3).FirstOrDefault()?.Id ?? 0;
                                logs_remarks = CreateMcECategories(newCategory.Category4Code, newCategory.Category4Description, 4, parentid);
                                sqlcategorydata = GetCodeIdCategories();
                            }
                        }
                        if (!string.IsNullOrEmpty(logs_remarks))
                        {
                            XSupport.Warning($"Υπήρξαν προβλήματα κατά την εισαγωγή των Κατηγοριών:\n{logs_remarks}");
                        }
                        else
                        {
                            XSupport.Warning("Η εισαγωγή των Κατηγοριών ολοκληρώθηκε με επιτυχία.");
                        }
                    }
                }
                XModule.CloseSubForm("SfCategories");
            }
            return base.ExecCommand(Cmd);
        }
        public List<SqlCategoryData> GetCodeIdCategories()
        {
            var data = new List<SqlCategoryData>();
            var query = $@"SELECT A.CCCMCECATEGORY AS ID,A.CODE,A.NAME,A.CATLEVEL FROM CCCMCECATEGORY A  WHERE A.COMPANY={XSupport.ConnectionInfo.CompanyId} ";
            using (var ds = XSupport.GetSQLDataSet(query, null))
            {
                try
                {
                    if (ds.Count > 0)
                    {
                        for (int i = 0; i < ds.Count; i++)
                        {
                            var catdata = new SqlCategoryData
                            {
                                Id = ds.GetAsInteger(i, "ID"),
                                Code = ds.GetAsString(i, "CODE"),
                                Name = ds.GetAsString(i, "NAME"),
                                CatLevel = ds.GetAsInteger(i, "CATLEVEL")
                            };
                            data.Add(catdata);
                        }
                    }
                    return data;
                }
                catch (Exception ex)
                {
                    return data;
                    throw new Exception(ex.Message);
                }
            }
        }
        public List<CategoriesData> GetSQLCategoriesData()
        {
            var categoriesData = new List<CategoriesData>();
            var query = $@"SELECT
                        -- Επίπεδο 1
                        ISNULL(CASE A.CATLEVEL WHEN 1 THEN A.CODE WHEN 2 THEN A2.CODE WHEN 3 THEN A3.CODE WHEN 4 THEN A4.CODE END,'') AS SPOMAD1CD,
                        ISNULL(CASE A.CATLEVEL WHEN 1 THEN A.NAME WHEN 2 THEN A2.NAME WHEN 3 THEN A3.NAME WHEN 4 THEN A4.NAME END,'') AS SPOMAD1PERI,
                        -- Επίπεδο 2
                        ISNULL(CASE WHEN A.CATLEVEL >= 2 THEN CASE A.CATLEVEL WHEN 2 THEN A.CODE WHEN 3 THEN A2.CODE WHEN 4 THEN A3.CODE END END,'') AS SPOMAD2CD,
                        ISNULL(CASE WHEN A.CATLEVEL >= 2 THEN CASE A.CATLEVEL WHEN 2 THEN A.NAME WHEN 3 THEN A2.NAME WHEN 4 THEN A3.NAME END END,'') AS SPOMAD2PERI,
                        -- Επίπεδο 3
                        ISNULL(CASE WHEN A.CATLEVEL >= 3 THEN CASE A.CATLEVEL WHEN 3 THEN A.CODE WHEN 4 THEN A2.CODE END END,'') AS SPOMAD3CD,
                        ISNULL(CASE WHEN A.CATLEVEL >= 3 THEN CASE A.CATLEVEL WHEN 3 THEN A.NAME WHEN 4 THEN A2.NAME END END,'') AS SPOMAD3PERI,
                        -- Επίπεδο 4
                        ISNULL(CASE WHEN A.CATLEVEL = 4 THEN A.CODE END,'') AS SPOMAD4CD,
                        ISNULL(CASE WHEN A.CATLEVEL = 4 THEN A.NAME END,'') AS SPOMAD4PERI
                        FROM CCCMCECATEGORY A
                        -- Γονέας του A
                        LEFT JOIN CCCMCECATEGORYPAR B ON B.CCCMCECATEGORY = A.CCCMCECATEGORY AND B.COMPANY = A.COMPANY
                        LEFT JOIN CCCMCECATEGORY A2 ON A2.CCCMCECATEGORY = B.PARENTID AND A2.COMPANY = A.COMPANY
                        -- Παππούς του A
                        LEFT JOIN CCCMCECATEGORYPAR B2 ON B2.CCCMCECATEGORY = A2.CCCMCECATEGORY AND B2.COMPANY = A2.COMPANY
                        LEFT JOIN CCCMCECATEGORY A3 ON A3.CCCMCECATEGORY = B2.PARENTID AND A3.COMPANY = A2.COMPANY
                        -- Προπάππους του A
                        LEFT JOIN CCCMCECATEGORYPAR B3 ON B3.CCCMCECATEGORY = A3.CCCMCECATEGORY AND B3.COMPANY = A3.COMPANY
                        LEFT JOIN CCCMCECATEGORY A4 ON A4.CCCMCECATEGORY = B3.PARENTID AND A4.COMPANY = A3.COMPANY
                        WHERE A.COMPANY = {XSupport.ConnectionInfo.CompanyId}
                        AND NOT EXISTS (SELECT 1 FROM CCCMCECATEGORYPAR CHILD WHERE CHILD.PARENTID = A.CCCMCECATEGORY AND CHILD.COMPANY = A.COMPANY)
                        ORDER BY SPOMAD1CD,SPOMAD2CD,SPOMAD3CD,SPOMAD4CD";
            using (var ds = XSupport.GetSQLDataSet(query, null))
            {
                try
                {
                    if (ds.Count > 0)
                    {
                        for (int i = 0; i < ds.Count; i++)
                        {
                            var catdata = new CategoriesData
                            {
                                Category1Code = ds.GetAsString(i, "SPOMAD1CD"),
                                Category1Description = ds.GetAsString(i, "SPOMAD1PERI"),
                                Category2Code = ds.GetAsString(i, "SPOMAD2CD"),
                                Category2Description = ds.GetAsString(i, "SPOMAD2PERI"),
                                Category3Code = ds.GetAsString(i, "SPOMAD3CD"),
                                Category3Description = ds.GetAsString(i, "SPOMAD3PERI"),
                                Category4Code = ds.GetAsString(i, "SPOMAD4CD"),
                                Category4Description = ds.GetAsString(i, "SPOMAD4PERI")
                            };
                            categoriesData.Add(catdata);
                        }
                    }
                    return categoriesData;
                }
                catch (Exception ex)
                {
                    return categoriesData;
                    throw new Exception(ex.Message);
                }
            }
        }
        public string CreateMcECategories(string code, string name,int catlevel,int parentid)
        {
            var logs_remarks = "";
            try
            {
                using (var ImpObj = XSupport.CreateModule("CCCMCECATEGORY"))
                {
                    ImpObj.InsertData();
                    ImpObj.GetTable("CCCMCECATEGORY").Current["CODE"] = code;
                    ImpObj.GetTable("CCCMCECATEGORY").Current["NAME"] = name;
                    ImpObj.GetTable("CCCMCECATEGORY").Current["ISACTIVE"] = 1;
                    ImpObj.GetTable("CCCMCECATEGORY").Current["CATLEVEL"] = catlevel;
                    if (parentid > 0)
                    {
                        using (var categoryparent = ImpObj.GetTable("CCCMCECATEGORYPAR"))
                        {
                            var recNo1 = -1;
                            for (var i = 0; i < categoryparent.Count; i++)
                            {
                                var recparentId = Convert.ToInt32(categoryparent[i, "PARENTID"]);
                                if (recparentId == parentid)
                                {
                                    recNo1 = i;
                                    break;
                                }
                            }
                            if (recNo1 == -1)
                            {
                                categoryparent.Current.Append();
                                categoryparent.Current["PARENTID"] = parentid;
                                categoryparent.Current.Post();
                            }
                        }
                    }
                    ImpObj.PostData();
                }
            }
            catch (Exception ex)
            {
                logs_remarks = logs_remarks + $"Πρόβλημα στην Κατηγορία Eshop «{code}-{name}»." + ex.Message + Environment.NewLine;
            }
            return logs_remarks;
        }
    }
}
