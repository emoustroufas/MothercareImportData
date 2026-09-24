using MothercareImportData.Models;
using NPOI.HSSF.Record;
using Org.BouncyCastle.Asn1.Cms;
using SixLabors.ImageSharp;
using Softone;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Services
{
    public class SoftoneService
    {
        private XSupport _xSupport;
        public SoftoneService(XSupport xSupport)
        {
            _xSupport = xSupport;
        }
        public List<SqlData> GetSqlData()
        {
            var sqldata = new List<SqlData>();
            var query = $@"SELECT 'theme' AS OBJ,MTRMANFCTR AS ID,CODE,NAME FROM MTRMANFCTR WHERE ISACTIVE=1 AND COMPANY={_xSupport.ConnectionInfo.CompanyId} 
                            UNION ALL SELECT 'division' AS OBJ, CCCDIVISION AS ID, CODE, NAME FROM CCCDIVISION WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  
                            UNION ALL SELECT 'department' AS OBJ, CCCDEPARTMENT AS ID, CODE, NAME FROM CCCDEPARTMENT WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  
                            UNION ALL SELECT 'subdept' AS OBJ, CCCSUBDEPT AS ID, CODE, NAME FROM CCCSUBDEPT WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  
                            UNION ALL SELECT 'class' AS OBJ, CCCCLASS AS ID, CODE, NAME FROM CCCCLASS WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  
                            UNION ALL SELECT 'size' AS OBJ, CCCSIZE AS ID, CODE, NAME FROM CCCSIZE WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} 
                            UNION ALL SELECT 'color' AS OBJ, CCCCOLOR AS ID, CODE, NAME FROM CCCCOLOR WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  
                            UNION ALL SELECT 'brand' AS OBJ, CCCBRAND AS ID, CODE, NAME FROM CCCBRAND WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} 
                            UNION ALL SELECT 'intrastat' AS OBJ, INTRASTAT AS ID, CODE, NAME FROM INTRASTAT WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} 
                            UNION ALL SELECT 'season' AS OBJ, MTRSEASON AS ID, CODE, NAME FROM MTRSEASON WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} 
                            UNION ALL SELECT 'collection' AS OBJ, UTBL04 AS ID, CODE, NAME FROM UTBL04 WHERE ISACTIVE = 1 AND ISNULL(CCCISMC,0)=1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} AND SODTYPE = 51
                            UNION ALL SELECT 'vat' AS OBJ, VAT AS ID, CAST(PERCNT AS VARCHAR) AS CODE, NAME FROM VAT WHERE ISACTIVE = 1 
                            UNION ALL SELECT 'busunit' AS OBJ, BUSUNITS AS ID, CODE, NAME FROM BUSUNITS WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}
                            UNION ALL SELECT 'itemtype' AS OBJ, MTRCATEGORY AS ID, CODE, NAME FROM MTRCATEGORY WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  AND SODTYPE = 51
                            UNION ALL SELECT 'accountingtype' AS OBJ, MTRACN AS ID, CODE, NAME FROM MTRACN WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  AND SODTYPE = 51
                            UNION ALL SELECT 'country' AS OBJ, COUNTRY AS ID, SHORTCUT AS CODE, NAME FROM COUNTRY WHERE ISACTIVE = 1 
                            UNION ALL SELECT 'item' AS OBJ, MTRL AS ID, CODE, NAME FROM MTRL WHERE COMPANY = {_xSupport.ConnectionInfo.CompanyId}  AND SODTYPE = 51 AND ISNULL(CCCITEMCOMPANY,0) IN (0,2)
                            UNION ALL SELECT 'supplier' AS OBJ, TRDR AS ID, CODE, NAME FROM TRDR WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} AND SODTYPE=12
                            UNION ALL SELECT 'sizeguide' AS OBJ, CCCSIZEGUIDE AS ID, CODE, NAME FROM CCCSIZEGUIDE WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}
                            UNION ALL SELECT 'seasonality' AS OBJ, CCCSEASONALITY AS ID, CODE, NAME FROM CCCSEASONALITY WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}
                            UNION ALL SELECT 'house' AS OBJ, CCCHOUSE AS ID, CODE, NAME FROM CCCHOUSE WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}
                            UNION ALL SELECT 'commercialcollection' AS OBJ, CCCCOMMERCIALCOLLECTION AS ID, CODE, NAME FROM CCCCOMMERCIALCOLLECTION WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}
                            ";
            using (var ds = _xSupport.GetSQLDataSet(query, null))
            {
                try
                {
                    if (ds.Count > 0)
                    {
                        for (int i = 0; i < ds.Count; i++)
                        {
                            var res = new SqlData
                            {
                                Obj = ds.GetAsString(i, "OBJ"),
                                Id = ds.GetAsInteger(i, "ID"),
                                Code = ds.GetAsString(i, "CODE"),
                                Name = ds.GetAsString(i, "NAME")
                            };
                            sqldata.Add(res);
                        }
                    }
                    return sqldata;
                }
                catch (Exception ex)
                {
                    return sqldata;
                    throw new Exception(ex.Message);
                }
            }
        }

        public List<AttributeRecord> GetSqlAttributeData()
        {
            var sqldata = new List<AttributeRecord>();
            try
            {
                //Attributes
                var attributelist = new List<AttributeRecord>();
                var queryAttributes = $@"SELECT MTRATTRIBUTE,CODE,NAME FROM MTRATTRIBUTE WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId} AND ISACTIVE=1";
                using (var dsAttributes = _xSupport.GetSQLDataSet(queryAttributes, null))
                {
                    try 
                    {
                        if (dsAttributes.Count > 0)
                        {
                            for (int i = 0; i < dsAttributes.Count; i++)
                            {
                                var mtrattributeId = dsAttributes.GetAsInteger(i, "MTRATTRIBUTE");
                                //Attribute Translation
                                var attributetranslationlist = new List<AttributeTranslation>();
                                var queryAttributeTranslation = $@"SELECT MTRATTRIBUTE,CCCLANGUAGE,TRANSLATION FROM CCCATTIBUTETRANSLATION 
                                                                    WHERE MTRATTRIBUTE={mtrattributeId} AND COMPANY={_xSupport.ConnectionInfo.CompanyId} 
                                                                    AND ISNULL(CCCLANGUAGE,0)<>0 AND DATATYPE=1";
                                using (var dsAttributeTranslation = _xSupport.GetSQLDataSet(queryAttributeTranslation, null))
                                {
                                    if (dsAttributeTranslation != null)
                                    {
                                        try
                                        {
                                            if (dsAttributeTranslation.Count > 0)
                                            {
                                                for (int j = 0; j < dsAttributeTranslation.Count; j++)
                                                {
                                                    var resAttributeTranslation = new AttributeTranslation
                                                    {
                                                        LanguageCode = dsAttributeTranslation.GetAsInteger(j, "CCCLANGUAGE"),
                                                        Description = dsAttributeTranslation.GetAsString(j, "TRANSLATION")
                                                    };
                                                    attributetranslationlist.Add(resAttributeTranslation);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new Exception(ex.Message);
                                        }
                                    }
                                }
                                //Attribute Values
                                var attributevaluelist = new List<AttributeValue>();
                                var queryAttributeValues = $@"SELECT MTRATTRIBUTE,MTRATTRIBUTELN,CODE,SOVALUE FROM MTRATTRIBUTELN 
                                                              WHERE MTRATTRIBUTE={mtrattributeId} AND COMPANY={_xSupport.ConnectionInfo.CompanyId} AND ISACTIVE=1";
                                using (var dsAttributeValues = _xSupport.GetSQLDataSet(queryAttributeValues, null))
                                {
                                    try
                                    {
                                        if (dsAttributeValues.Count > 0)
                                        {
                                            for (int v = 0; v < dsAttributeValues.Count; v++)
                                            {
                                                var mtrattributevalueId = dsAttributeValues.GetAsInteger(v, "MTRATTRIBUTELN");
                                                //Attribute Value Translation
                                                var attributevaluetranslationlist = new List<AttributeValueTranslation>();
                                                var queryAttributeValueTranslation = $@"SELECT MTRATTRIBUTE,MTRATTRIBUTELN,CCCLANGUAGE,TRANSLATION FROM CCCATTIBUTETRANSLATION 
                                                                                        WHERE MTRATTRIBUTE={mtrattributeId} AND MTRATTRIBUTELN={mtrattributevalueId} 
                                                                                        AND COMPANY={_xSupport.ConnectionInfo.CompanyId} AND ISNULL(CCCLANGUAGE,0)<>0 AND DATATYPE=2";
                                                using (var dsAttributeValueTranslation = _xSupport.GetSQLDataSet(queryAttributeValueTranslation, null))
                                                {
                                                    if (dsAttributeValueTranslation != null)
                                                    {
                                                        try
                                                        {
                                                            if (dsAttributeValueTranslation.Count > 0)
                                                            {
                                                                for (int t = 0; t < dsAttributeValueTranslation.Count; t++)
                                                                {
                                                                    var resAttributeValueTranslation = new AttributeValueTranslation
                                                                    {
                                                                        LanguageCode = dsAttributeValueTranslation.GetAsInteger(t, "CCCLANGUAGE"),
                                                                        Description = dsAttributeValueTranslation.GetAsString(t, "TRANSLATION")
                                                                    };
                                                                    attributevaluetranslationlist.Add(resAttributeValueTranslation);
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            throw new Exception(ex.Message);
                                                        }
                                                    }
                                                }
                                                var resAttributeValues = new AttributeValue
                                                {
                                                    SoftOneId = dsAttributeValues.GetAsInteger(v, "MTRATTRIBUTELN"),
                                                    Code = dsAttributeValues.GetAsString(v, "CODE"),
                                                    Translations = attributevaluetranslationlist
                                                };
                                                attributevaluelist.Add(resAttributeValues);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }
                                var res = new AttributeRecord
                                {
                                    SoftOneId = dsAttributes.GetAsInteger(i, "MTRATTRIBUTE"),
                                    Code = dsAttributes.GetAsString(i, "CODE"),
                                    Translations = attributetranslationlist,
                                    Values = attributevaluelist
                                };
                                attributelist.Add(res);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                sqldata = attributelist;
                return sqldata;
            }
            catch (Exception ex)
            {
                return sqldata;
                throw new Exception(ex.Message);
            }
        }
        public void CreateSizeGuide(List<SizeGuideRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                try
                {
                    foreach (var exd in exceldata)
                    {
                        var exdcode = exd.Code;
                        var exdname = exd.Description;
                        try
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCSIZEGUIDE"))
                            {
                                ImpObj.InsertData();
                                ImpObj.GetTable("CCCSIZEGUIDE").Current["CODE"] = exdcode.ToString();
                                ImpObj.GetTable("CCCSIZEGUIDE").Current["NAME"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCSIZEGUIDE").Current["ISACTIVE"] = 1;
                                ImpObj.PostData();
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο Μεγεθολόγιο «{exdname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateSeasonality(List<SeasonalityRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                try
                {
                    foreach (var exd in exceldata)
                    {
                        var exdcode = exd.Code;
                        var exdname = exd.Description;
                        try
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCSEASONALITY"))
                            {
                                ImpObj.InsertData();
                                ImpObj.GetTable("CCCSEASONALITY").Current["CODE"] = exdcode.ToString();
                                ImpObj.GetTable("CCCSEASONALITY").Current["NAME"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCSEASONALITY").Current["ISACTIVE"] = 1;
                                ImpObj.PostData();
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στη Εποχικότητα «{exdname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateHouse(List<HouseRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                try
                {
                    foreach (var exd in exceldata)
                    {
                        var exdcode = exd.Code;
                        var exdname = exd.Description;
                        try
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCHOUSE"))
                            {
                                ImpObj.InsertData();
                                ImpObj.GetTable("CCCHOUSE").Current["CODE"] = exdcode.ToString();
                                ImpObj.GetTable("CCCHOUSE").Current["NAME"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCHOUSE").Current["ISACTIVE"] = 1;
                                ImpObj.PostData();
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στον Οίκο «{exdname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateCommercialCollection(List<CommercialCollectionRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                try
                {
                    foreach (var exd in exceldata)
                    {
                        var exdcode = exd.Code;
                        var exdname = exd.Description;
                        try
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCCOMMERCIALCOLLECTION"))
                            {
                                ImpObj.InsertData();
                                ImpObj.GetTable("CCCCOMMERCIALCOLLECTION").Current["CODE"] = exdcode.ToString();
                                ImpObj.GetTable("CCCCOMMERCIALCOLLECTION").Current["NAME"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCCOMMERCIALCOLLECTION").Current["ISACTIVE"] = 1;
                                ImpObj.PostData();
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στον Οίκο «{exdname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void UpdateSupBarcodes(List<SupBarcodeRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                try 
                {
                    foreach (var exd in exceldata)
                    { 
                        var exdtaxcode = exd.TaxCode;
                        var exdsupbarcode = exd.SupBarcode;
                        try
                        {
                            var updquery = $@"UPDATE MTRL SET CCCSUPBARCODE='{exdsupbarcode}' WHERE CODE2='{exdtaxcode}' AND COMPANY={_xSupport.ConnectionInfo.CompanyId}";
                            _xSupport.ExecuteSQL(updquery);
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο Barcode Προμηθευτή «{exdsupbarcode}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateDivision(List<DivisionRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                try
                {
                    foreach (var exd in exceldata)
                    {
                        var exdcode = exd.Code;
                        var exdname = exd.Description;
                        try
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCDIVISION"))
                            {
                                ImpObj.InsertData();
                                ImpObj.GetTable("CCCDIVISION").Current["CODE"] = exdcode.ToString();
                                ImpObj.GetTable("CCCDIVISION").Current["NAME"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCDIVISION").Current["NAMEENG"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCDIVISION").Current["ISACTIVE"] = 1;
                                ImpObj.PostData();
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο division «{exdname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateDepartment(List<DepartmentRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                try
                {
                    foreach (var exd in exceldata)
                    {
                        var exdcode = exd.Code;
                        var exdname = exd.Description;
                        try
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCDEPARTMENT"))
                            {
                                ImpObj.InsertData();
                                ImpObj.GetTable("CCCDEPARTMENT").Current["CODE"] = exdcode.ToString();
                                ImpObj.GetTable("CCCDEPARTMENT").Current["NAME"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCDEPARTMENT").Current["NAMEENG"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCDEPARTMENT").Current["ISACTIVE"] = 1;
                                var divisionId = _xSupport.SQL($"SELECT CCCDIVISION FROM CCCDIVISION WHERE CODE='{exd.Division}' AND COMPANY={_xSupport.ConnectionInfo.CompanyId}", null);
                                if (divisionId != null)
                                {
                                    ImpObj.GetTable("CCCDEPARTMENT").Current["CCCDIVISION"] = divisionId;
                                }
                                ImpObj.PostData();
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο department «{exdname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateSubdepartment(List<SubdepartmentRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                try
                {
                    foreach (var exd in exceldata)
                    {
                        var exdcode = exd.Code;
                        var exdname = exd.Description;
                        try
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCSUBDEPT"))
                            {
                                ImpObj.InsertData();
                                ImpObj.GetTable("CCCSUBDEPT").Current["CODE"] = exd.DepartmentCode+"-"+exdcode;
                                ImpObj.GetTable("CCCSUBDEPT").Current["NAME"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCSUBDEPT").Current["NAMEENG"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCSUBDEPT").Current["ISACTIVE"] = 1;
                                var departmentId = _xSupport.SQL($"SELECT CCCDEPARTMENT FROM CCCDEPARTMENT WHERE CODE='{exd.DepartmentCode}' AND COMPANY={_xSupport.ConnectionInfo.CompanyId}", null);
                                if (departmentId != null)
                                {
                                    ImpObj.GetTable("CCCSUBDEPT").Current["CCCDEPARTMENT"] = departmentId;
                                }
                                ImpObj.PostData();
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο subdept «{exdname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateClass(List<ClassRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                try
                {
                    foreach (var exd in exceldata)
                    {
                        var exdcode = exd.Code;
                        var exdname = exd.Description;
                        try
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCCLASS"))
                            {
                                ImpObj.InsertData();
                                ImpObj.GetTable("CCCCLASS").Current["CODE"] = exd.DepartmentCode+"-"+exd.SubdeptCode+"-"+exdcode;
                                ImpObj.GetTable("CCCCLASS").Current["NAME"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCCLASS").Current["NAMEENG"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCCLASS").Current["ISACTIVE"] = 1;
                                var subdeptId = _xSupport.SQL($"SELECT CCCSUBDEPT FROM CCCSUBDEPT WHERE CODE='{exd.DepartmentCode+"-"+exd.SubdeptCode/*exd.SubdeptCode*/}' AND COMPANY={_xSupport.ConnectionInfo.CompanyId}", null);
                                if (subdeptId != null)
                                {
                                    ImpObj.GetTable("CCCCLASS").Current["CCCSUBDEPT"] = subdeptId;
                                }
                                ImpObj.PostData();
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο class «{exdname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateBrand(List<BrandRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                try
                {
                    foreach (var exd in exceldata)
                    {
                        var exdcode = "MC" + exd.Code;
                        var exdname = exd.Description;
                        try
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCBRAND"))
                            {
                                ImpObj.InsertData();
                                ImpObj.GetTable("CCCBRAND").Current["CODE"] = exdcode.ToString();
                                ImpObj.GetTable("CCCBRAND").Current["NAME"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCBRAND").Current["ISACTIVE"] = 1;
                                ImpObj.PostData();
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο brand «{exdname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateCollection(List<CollectionRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                var queryMaxId = $@"SELECT ISNULL(MAX(UTBL04), 0) AS MAXID FROM UTBL04 WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId} AND SODTYPE=51";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                //var listMaxId = dsMaxId != DBNull.Value ? (object[])dsMaxId : null;
                //var maxid = listMaxId != null ? Convert.ToInt32(listMaxId[0]) : 0;
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                try
                {
                    foreach (var collection in exceldata)
                    {
                        maxid += 1;
                        var collectioncode = collection.Code;
                        var collectionname = collection.Description;
                        try
                        {
                            var updquery = $@"INSERT INTO UTBL04 (UTBL04, CODE, NAME, SODTYPE, ISACTIVE, COMPANY,CCCISMC)
                                              VALUES ({maxid},'{collectioncode}','{collectionname.Replace("'", "")}',51,1,{_xSupport.ConnectionInfo.CompanyId},1)";
                            _xSupport.ExecuteSQL(updquery);
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στη Συλλογή «{collectionname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateBusinessUnit(List<BuRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                var queryMaxId = $@"SELECT ISNULL(MAX(BUSUNITS), 0) AS MAXID FROM BUSUNITS";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                try
                {
                    foreach (var bu in exceldata)
                    {
                        var bucode = "MC" + bu.Code;
                        var buname = bu.Description;
                        maxid += 1;
                        try
                        {
                            var updquery = $@"INSERT INTO BUSUNITS ( BUSUNITS,CODE, NAME, ISACTIVE, COMPANY)
                                              VALUES ({maxid},'{bucode.Replace("'", "")}', '{buname.Replace("'", "")}', 1,{_xSupport.ConnectionInfo.CompanyId})";
                            _xSupport.ExecuteSQL(updquery);
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο BU «{buname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateItemType(List<ItemTypeRecord> exceldata)
        {
            try
            {
                var queryMaxId = $@"SELECT ISNULL(MAX(MTRCATEGORY), 0) AS MAXID FROM MTRCATEGORY WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId} AND SODTYPE=51";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;

                foreach (var exd in exceldata)
                {
                    var exdcode = "MC" + exd.Code;
                    var exdname = exd.Description;
                    maxid += 1;

                    try
                    {
                        using (var ImpObj = _xSupport.CreateModule("ITECATEGORY"))
                        {
                            ImpObj.InsertData();
                            ImpObj.GetTable("MTRCATEGORY").Current["MTRCATEGORY"] = maxid;
                            ImpObj.GetTable("MTRCATEGORY").Current["CODE"] = exdcode;
                            ImpObj.GetTable("MTRCATEGORY").Current["NAME"] = exdname.Replace("'", "");
                            ImpObj.GetTable("MTRCATEGORY").Current["ISACTIVE"] = 1;
                            ImpObj.PostData();
                        }
                    }
                    catch (Exception ex)
                    {
                        _xSupport.Exception($"Πρόβλημα στον Τύπο Είδους «{exdname}»." + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _xSupport.Exception(ex.Message);
            }
        }
        public void CreateAccountingType(List<AccountingTypeRecord> exceldata)
        {
            try
            {
                var queryMaxId = $@"SELECT ISNULL(MAX(MTRACN), 0) AS MAXID FROM MTRACN WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId} AND SODTYPE=51";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;

                foreach (var exd in exceldata)
                {
                    var exdcode = "MC" + exd.Code;
                    var exdname = exd.Description;
                    maxid += 1;

                    try
                    {
                        using (var ImpObj = _xSupport.CreateModule("ITEMGL"))
                        {
                            ImpObj.InsertData();
                            ImpObj.GetTable("MTRACN").Current["MTRACN"] = maxid;
                            ImpObj.GetTable("MTRACN").Current["CODE"] = exdcode;
                            ImpObj.GetTable("MTRACN").Current["NAME"] = exdname.Replace("'", "");
                            ImpObj.GetTable("MTRACN").Current["ISACTIVE"] = 1;
                            ImpObj.PostData();
                        }
                    }
                    catch (Exception ex)
                    {
                        _xSupport.Exception($"Πρόβλημα στον Τύπο Λογιστικής «{exdname}»." + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _xSupport.Exception(ex.Message);
            }
        }
        public void CreateIntrastat(List<string> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                var queryMaxId = $@"SELECT ISNULL(MAX(INTRASTAT), 0) AS MAXID FROM INTRASTAT WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId}";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                try
                {
                    foreach (var intrastat in exceldata)
                    {
                        maxid += 1;
                        var intrastatcode = intrastat.Substring(0, 8);
                        try
                        {
                            var updquery = $@"INSERT INTO INTRASTAT (INTRASTAT, CODE, NAME, ISACTIVE, COMPANY)
                                              VALUES ({maxid},'{intrastatcode}','{intrastat.Replace("'", "")}',1,{_xSupport.ConnectionInfo.CompanyId})";
                            _xSupport.ExecuteSQL(updquery);
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο Intrastat «{intrastatcode}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateTheme(List<string> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                var queryMaxId = $@"SELECT ISNULL(MAX(MTRMANFCTR), 0) AS MAXID FROM MTRMANFCTR WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId}";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                try
                {
                    foreach (var theme in exceldata)
                    {
                        maxid += 1;
                        var themecode = "MC"+ maxid;
                        var themename = theme;
                        try
                        {
                            var updquery = $@"INSERT INTO MTRMANFCTR (COMPANY,MTRMANFCTR,CODE,NAME,ISACTIVE)
                                              VALUES ({_xSupport.ConnectionInfo.CompanyId},{maxid}, '{themecode}','{themename.Replace("'", "")}',1)";
                            _xSupport.ExecuteSQL(updquery);
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο Θέμα «{themename}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void ImportBarcode(List<BarcodeRecord> exceldata, List<SqlData> item_list)
        { 
            if (exceldata.Count() > 0)
            {
                try
                {
                    foreach (var barcode in exceldata)
                    {
                        try
                        {
                            using (var ItemObj = _xSupport.CreateModule("ITEM;Items Mothercare"))
                            {
                                var mtrl_list = item_list.Where(x => x.Code.Trim() == barcode.ItemCode.Trim()).FirstOrDefault();
                                var mtrl = mtrl_list != null ? mtrl_list.Id : 0;
                                var mname = mtrl_list != null ? mtrl_list.Name : "";
                                if (mtrl>0)
                                {
                                    ItemObj.LocateData(mtrl);
                                    //Πίνακας MTRSUBSTITUTE
                                    using (var mtrsubstitute = ItemObj.GetTable("MTRSUBSTITUTE"))
                                    {
                                        var recNo1 = mtrsubstitute.Find("CODE", barcode.Barcode);
                                        if (recNo1 == -1)
                                        {
                                            mtrsubstitute.Current.Append();
                                            mtrsubstitute.Current["CODE"] = barcode.Barcode;
                                            mtrsubstitute.Current["NAME"] = mname;
                                            mtrsubstitute.Current["QTY1"] = Convert.ToDouble(1);
                                            //mtrsubstitute.Current["QTY2"] = Convert.ToDouble(1);
                                            mtrsubstitute.Current.Post();
                                        }
                                    }
                                    ItemObj.PostData();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο Barcode «{barcode.Barcode}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void SetSimilarItems(List<SimilarItemRecord> exceldata, List<SqlData> item_list)
        {
            if (exceldata.Count() > 0)
            {
                try
                {
                    var grouped = exceldata.GroupBy(r => r.ItemCode).Select(g => new SimilarItemGroup
                    {
                        ItemCode = g.Key,
                        ReferenceItemCodes = g.Select(r => r.ReferenceItemCode).ToList()
                    }).ToList();
                    foreach (var similar in grouped)
                    {
                        try
                        {
                            var mtrl_list = item_list.Where(x => x.Code.Trim() == similar.ItemCode.Trim()).FirstOrDefault();
                            var mtrl = mtrl_list != null ? mtrl_list.Id : 0;
                            var mname = mtrl_list != null ? mtrl_list.Name : "";
                            var similarCodes = similar.ReferenceItemCodes.Where(code => item_list.Any(item => item.Code.Trim() == code.Trim())).ToList();

                            if (mtrl > 0 && similarCodes.Count > 0)
                            {
                                using (var ItemObj = _xSupport.CreateModule("ITEM;Items Mothercare"))
                                {
                                    ItemObj.LocateData(mtrl);
                                    //Πίνακας CCCSIMILARITEMS
                                    using (var mtrsimilar = ItemObj.GetTable("CCCSIMILARITEMS"))
                                    {
                                        foreach (var code in similarCodes)
                                        {
                                            var similarItem_list = item_list.Where(x => x.Code.Trim() == code.Trim()).FirstOrDefault();
                                            var similarItem = similarItem_list != null ? similarItem_list.Id : 0;
                                            var recNo1 = mtrsimilar.Find("SIMMTRL", similarItem);
                                            if (recNo1 == -1)
                                            {
                                                mtrsimilar.Current.Append();
                                                mtrsimilar.Current["SIMMTRL"] = similarItem;
                                                mtrsimilar.Current["SIMILARITY"] = Convert.ToDouble(100) ;
                                                mtrsimilar.Current.Post();
                                            }
                                        }
                                    }
                                    ItemObj.PostData();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο Όμοιο Είδος «{similar.ItemCode}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        public void CreateUpdateAttributes(List<AttributeRecord> attributes)
        {
            if (attributes.Count() > 0)
            {
                foreach (var attr in attributes)
                {
                    try
                    {
                        using (var AttributeObj = _xSupport.CreateModule("MTRATTRIBUTE;Attributes"))
                        {
                            var attributeId = attr.SoftOneId;
                            if (attributeId > 0)
                            {
                                AttributeObj.LocateData(attributeId);
                                var name = attr.Translations.Any() ? attr.Translations.OrderBy(x => x.LanguageCode).FirstOrDefault().Description : (AttributeObj.GetTable("MTRATTRIBUTE").Current["NAME"] !="" ? AttributeObj.GetTable("MTRATTRIBUTE").Current["NAME"] : attr.Code);
                                AttributeObj.GetTable("MTRATTRIBUTE").Current["NAME"] = name;
                                if (attr.Translations.Count > 0)
                                {
                                    foreach (var trns in attr.Translations)
                                    {
                                        //Πίνακας ATTIBUTETRANSH
                                        using (var attributetrnsh = AttributeObj.GetTable("ATTIBUTETRANSH"))
                                        {
                                            var recNo1 = attributetrnsh.Find("MTRATTRIBUTE;CCCLANGUAGE", attributeId, trns.LanguageCode);
                                            if (recNo1 != -1)
                                            {
                                                attributetrnsh.Current.Append();
                                                //attributetrnsh.Current["MTRATTRIBUTE"] = attributeId;
                                                attributetrnsh.Current["CCCLANGUAGE"] = trns.LanguageCode;
                                                attributetrnsh.Current["TRANSLATION"] = trns.Description;
                                                //attributetrnsh.Current.Post();
                                            }
                                            else
                                            {
                                                attributetrnsh.Current["CCCLANGUAGE"] = trns.LanguageCode;
                                                attributetrnsh.Current["TRANSLATION"] = trns.Description;
                                                //attributetrnsh.Current.Post();
                                            }
                                            attributetrnsh.Current.Post();
                                        }
                                    }
                                }
                                if (attr.Values.Count>0)
                                {
                                    foreach (var val in attr.Values)
                                    {
                                        var attributelnId = val.SoftOneId;
                                        using (var mtrattributeln = AttributeObj.GetTable("MTRATTRIBUTELN"))
                                        {
                                            if (val.SoftOneId > 0)
                                            {
                                                var recNo1 = mtrattributeln.Find("MTRATTRIBUTE;MTRATTRIBUTELN;CODE", attributeId, attributelnId, val.Code);
                                                if (recNo1 != -1)
                                                {
                                                    //mtrattributeln.Current["CODE"] = val.Code;
                                                    var nameln = val.Translations.Any() ? val.Translations.OrderBy(x => x.LanguageCode).FirstOrDefault().Description : val.Code;
                                                    mtrattributeln.Current["SOVALUE"] = nameln;
                                                    //mtrattributeln.Current.Post();
                                                }
                                            }
                                            else
                                            {
                                                mtrattributeln.Current.Append();
                                                mtrattributeln.Current["CODE"] = val.Code;
                                                mtrattributeln.Current["SOVALUE"] = val.Translations.Any() ? val.Translations.OrderBy(x => x.LanguageCode).FirstOrDefault().Description : val.Code;
                                                //mtrattributeln.Current.Post();
                                            }

                                            if (val.Translations.Count>0)
                                            {
                                                foreach (var trnsln in val.Translations.OrderBy(x=>x.LanguageCode))
                                                {
                                                    //Πίνακας ATTIBUTETRANSLN
                                                    using (var attributetrnsln = AttributeObj.GetTable("ATTIBUTETRANSLN"))
                                                    {
                                                        var recNo1 = attributetrnsln.Find("MTRATTRIBUTE;MTRATTRIBUTELN;CCCLANGUAGE", attributeId, attributelnId, trnsln.LanguageCode);
                                                        if (recNo1 != -1)
                                                        {
                                                            attributetrnsln.Current["CCCLANGUAGE"] = trnsln.LanguageCode;
                                                            attributetrnsln.Current["TRANSLATION"] = trnsln.Description;
                                                            //attributetrnsh.Current.Post();
                                                        }
                                                        else
                                                        {
                                                            attributetrnsln.Current.Append();
                                                            attributetrnsln.Current["CCCLANGUAGE"] = trnsln.LanguageCode;
                                                            attributetrnsln.Current["TRANSLATION"] = trnsln.Description;
                                                            //attributetrnsh.Current.Post();
                                                        }
                                                        attributetrnsln.Current.Post();
                                                    }
                                                }
                                            }
                                            mtrattributeln.Current.Post();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                AttributeObj.InsertData();
                                AttributeObj.GetTable("MTRATTRIBUTE").Current["CODE"] = attr.Code;
                                var name = attr.Translations.Any() ? attr.Translations.OrderBy(x => x.LanguageCode).FirstOrDefault().Description : attr.Code;
                                AttributeObj.GetTable("MTRATTRIBUTE").Current["NAME"] = name;
                                if (attr.Translations.Count > 0)
                                {
                                    foreach (var trns in attr.Translations)
                                    {
                                        //Πίνακας ATTIBUTETRANSH
                                        using (var attributetrnsh = AttributeObj.GetTable("ATTIBUTETRANSH"))
                                        {
                                            attributetrnsh.Current.Append();
                                            attributetrnsh.Current["CCCLANGUAGE"] = trns.LanguageCode;
                                            attributetrnsh.Current["TRANSLATION"] = trns.Description;
                                            attributetrnsh.Current.Post();
                                        }
                                    }
                                }
                                if (attr.Values.Count > 0)
                                {
                                    foreach (var val in attr.Values)
                                    {
                                        //Πίνακας ATTIBUTETRANSH
                                        using (var mtrattributeln = AttributeObj.GetTable("MTRATTRIBUTELN"))
                                        {
                                                mtrattributeln.Current.Append();
                                                mtrattributeln.Current["CODE"] = val.Code;
                                                mtrattributeln.Current["SOVALUE"] = val.Translations.Any() ? val.Translations.OrderBy(x => x.LanguageCode).FirstOrDefault().Description : val.Code;
                                            if (val.Translations.Count > 0)
                                            {
                                                foreach (var trnsln in val.Translations.OrderBy(x=>x.LanguageCode))
                                                {
                                                    //Πίνακας ATTIBUTETRANSLN
                                                    using (var attributetrnsln = AttributeObj.GetTable("ATTIBUTETRANSLN"))
                                                    {
                                                        attributetrnsln.Current.Append();
                                                        attributetrnsln.Current["CCCLANGUAGE"] = trnsln.LanguageCode;
                                                        attributetrnsln.Current["TRANSLATION"] = trnsln.Description;
                                                        attributetrnsln.Current.Post();
                                                    }
                                                }
                                            }
                                            mtrattributeln.Current.Post();
                                        }
                                    }
                                }
                            }
                            AttributeObj.PostData();
                        }
                    }
                    catch (Exception ex)
                    {
                        _xSupport.Exception($"Πρόβλημα στο Attribute «{attr.Code}»." + ex.Message);
                    }
                }
            }
        }
        public void CreateUpdateItems(List<ItemMasterRecord> exceldata, List<SqlData> sqlData)
        {
            var logs_remarks = "";
            if (exceldata.Count > 0)
            {
                var item_list = sqlData.Where(x => x.Obj == "item").ToList();
                var theme_list = sqlData.Where(x => x.Obj == "theme").ToList();
                var division_list = sqlData.Where(x => x.Obj == "division").ToList();
                var department_list = sqlData.Where(x => x.Obj == "department").ToList();
                var subdepartment_list = sqlData.Where(x => x.Obj == "subdept").ToList();
                var class_list = sqlData.Where(x => x.Obj == "class").ToList();
                var size_list = sqlData.Where(x => x.Obj == "size").ToList();
                var color_list = sqlData.Where(x => x.Obj == "color").ToList();
                var brand_list = sqlData.Where(x => x.Obj == "brand").ToList();
                var intrastat_list = sqlData.Where(x => x.Obj == "intrastat").ToList();
                var season_list = sqlData.Where(x => x.Obj == "season").ToList();
                var collection_list = sqlData.Where(x => x.Obj == "collection").ToList();
                var commercialcollection_list = sqlData.Where(x => x.Obj == "commercialcollection").ToList();
                var vat_list = sqlData.Where(x => x.Obj == "vat").ToList();
                var busunit_list = sqlData.Where(x => x.Obj == "busunit").ToList();
                var itemtype_list = sqlData.Where(x => x.Obj == "itemtype").ToList();
                var accountingtype_list = sqlData.Where(x => x.Obj == "accountingtype").ToList();
                var country_list = sqlData.Where(x => x.Obj == "country").ToList();
                var supplier_list = sqlData.Where(x => x.Obj == "supplier").ToList();
                var sizeguide_list = sqlData.Where(x => x.Obj == "sizeguide").ToList();
                var seasonality_list = sqlData.Where(x => x.Obj == "seasonality").ToList();
                var house_list = sqlData.Where(x => x.Obj == "house").ToList();

                using (var ItemObj = _xSupport.CreateModule("ITEM;Items Mothercare"))
                {
                    var counter = 0;
                    foreach (var item in exceldata)
                    {
                        counter++;
                        try
                        {
                            var mtrl_list = item_list.Where(x => x.Code.Trim() == item.Code.Trim()).FirstOrDefault();
                            var mtrl = mtrl_list != null ? mtrl_list.Id : 0;
                            if (mtrl == 0)
                            {
                                ItemObj.InsertData();
                                ItemObj.GetTable("MTRL").Current["CODE"] = item.Code;
                                ItemObj.GetTable("MTRL").Current["CCCITEMCOMPANY"] = 2; //0=Όλοι, 1=Dpam, 2=Mothercare
                            }
                            else
                            {
                                ItemObj.LocateData(mtrl);
                                var itemcompany = Convert.ToInt32(ItemObj.GetTable("MTRL").Current["CCCITEMCOMPANY"] != DBNull.Value ? ItemObj.GetTable("MTRL").Current["CCCITEMCOMPANY"] : 0);
                                if (itemcompany == 1)
                                {
                                    ItemObj.GetTable("MTRL").Current["CCCITEMCOMPANY"] = 0; //0=Όλοι, 1=Dpam, 2=Mothercare
                                }
                            }
                            ItemObj.GetTable("MTRL").Current["NAME"] = item.Name;

                            ItemObj.GetTable("MTRL").Current["CODE2"] = item.TaxCode;
                            ItemObj.GetTable("MTRL").Current["CCCASSORTMENTDESCR"] = item.AssortmentDescription;
                            ItemObj.GetTable("MTRL").Current["CCCSUPPLIERCODE"] = item.SupplierCode;

                            ItemObj.GetTable("MTRL").Current["NAME1"] = item.EnglishDescription;
                            ItemObj.GetTable("MTRL").Current["REMARKS"] = item.Comments;
                            var mtrunit = 0;
                            switch(item.UnitOfMeasure)
                            {
                                case 1 /*ΤΕΜΑΧΙΟ*/ : mtrunit = 101 /*Τεμ.*/; break;
                                case 2 /*ΜΕΤΡΟ*/ : mtrunit = 120 /*Μέτρα*/; break;
                                case 4 /*ΚΙΛΑ*/: mtrunit = 150 /*Κιλά*/; break;
                                case 6 /*Κ.ΜΕΤΡΑ*/: mtrunit = 140 /*Κ.Μέτ*/; break;
                                default: mtrunit = 101; break;
                            }
                            ItemObj.GetTable("MTRL").Current["MTRUNIT1"] = mtrunit;
                            ItemObj.GetTable("MTRL").Current["MTRUNIT2"] = item.PackageQuantity > 0 ? 108 : 101;
                            ItemObj.GetTable("MTRL").Current["MTRUNIT3"] = mtrunit;
                            ItemObj.GetTable("MTRL").Current["MTRUNIT4"] = mtrunit;
                            ItemObj.GetTable("MTRL").Current["MU21"] = item.PackageQuantity > 0 ? Convert.ToDouble(item.PackageQuantity) : 1;
                            ItemObj.GetTable("MTRL").Current["MU31"] = Convert.ToDouble(1);
                            ItemObj.GetTable("MTRL").Current["MU41"] = Convert.ToDouble(1);
                            ItemObj.GetTable("MTRL").Current["MU12MODE"] = 1;
                            ItemObj.GetTable("MTRL").Current["MU13MODE"] = 1;
                            ItemObj.GetTable("MTRL").Current["MU14MODE"] = 1;
                            ItemObj.GetTable("MTRL").Current["CCCCOMPOSEDOFQTY"] = item.ComposedOfQuantity;

                            var divisionId = division_list.Where(x => x.Code.Trim() == item.Division.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCDIVISION"] = divisionId;

                            var departmentId = department_list.Where(x => x.Code.Trim() == item.Department.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCDEPARTMENT"] = departmentId;

                            var subdeptId = subdepartment_list.Where(x => x.Code.Trim() == item.Department.ToString().Trim() + "-" + item.Subdept.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCSUBDEPT"] = subdeptId;
                            var classId = class_list.Where(x => x.Code.Trim() == item.Department.ToString().Trim() + "-" + item.Subdept.ToString().Trim() + "-" + item.Class.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCCLASS"] = classId;

                            ItemObj.GetTable("MTRL").Current["CCCYEAR"] = item.Year;
                            ItemObj.GetTable("MTRL").Current["CCCQUARTER"] = item.Season;
                            ItemObj.GetTable("MTRL").Current["CCCCURYEAR"] = item.StatisticalYear;

                            var mtrmanufacturerId = theme_list.Where(x => x.Name.Trim() == item.StyleNo.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["MTRMANFCTR"] = mtrmanufacturerId;

                            //var sizeId = 0;//item.Size;
                            //ItemObj.GetTable("MTRL").Current["CCCSIZE"] = sizeId;
                            //var colorId = 0;//item.Color;
                            //ItemObj.GetTable("MTRL").Current["CCCCOLOR"] = colorId;

                            var sizeguideId = sizeguide_list.Where(x => x.Code.Trim() == item.SizeGuide.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCSIZEGUIDE"] = sizeguideId;

                            var brandId = brand_list.Where(x => x.Code.Trim() == "MC" + item.Brand.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCBRAND"] = brandId;

                            var houseId = house_list.Where(x => x.Code.Trim() == item.House.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCHOUSE"] = houseId;

                            var vat = 0;
                            switch (item.VatCategory)
                            {
                                case 1 /*Υψηλός Συντελεστής*/ : vat = 1410 /*24*/; break;
                                case 2 /*Χαμηλός Συντελεστής*/ : vat = 1060 /*6*/; break;
                                case 3 /*Μεσαίος Συντελεστής*/: vat = 1131 /*13*/; break;
                                case 4 /*Μηδενικός Συντελεστής*/: vat = 0 /*0*/; break;
                                default: vat = 1410; break;
                            }
                            ItemObj.GetTable("MTRL").Current["VAT"] = vat;

                            ItemObj.GetTable("MTRL").Current["CCCFASI"] = item.Phase;

                            var seasonalityId = seasonality_list.Where(x => x.Code.Trim() == item.Seasonality.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCSEASONALITY"] = seasonalityId;

                            ItemObj.GetTable("MTRL").Current["CCCLISTUP"] = item.ListUp;

                            if (item.Outlet != null)
                            {
                                ItemObj.GetTable("MTRL").Current["CCCOUTLET"] = item.Outlet;
                            }

                            ItemObj.GetTable("MTRL").Current["GWEIGHT"] = item.NetWeight;

                            var countryId = country_list.Where(x => x.Code.Trim() == item.CountryOfOrigin.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["COUNTRY"] = countryId;

                            var intrastatId = intrastat_list.Where(x => x.Code.Trim() == item.Intrastat.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["INTRASTAT"] = intrastatId;

                            ItemObj.GetTable("MTRL").Current["ISACTIVE"] = 1;

                            ItemObj.GetTable("MTRL").Current["CCCMSSTATUS"] = item.Status;

                            var collectionId = collection_list.Where(x => x.Code.Trim() == item.Collection.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTREXTRA").Current["UTBL04"] = collectionId;

                            var commercialcollectionId = commercialcollection_list.Where(x => x.Code.Trim() == item.CommercialCollection.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCCOMMERCIALCOLLECTION"] = commercialcollectionId;

                            var buId = busunit_list.Where(x => x.Code.Trim() == "MC" + item.Bu.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["BUSUNITS"] = buId;

                            var itemtypeId = itemtype_list.Where(x => x.Code.Trim() == "MC" + item.ItemType.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["MTRCATEGORY"] = itemtypeId;

                            var accountingtypeId = accountingtype_list.Where(x => x.Code.Trim() == "MC" + item.AccountingType.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["MTRACN"] = accountingtypeId;

                            ItemObj.GetTable("MTREXTRA").Current["VARCHAR01"] = item.ImagePath;
                            ItemObj.GetTable("MTREXTRA").Current["BOOL02"] = item.RestockWithPackage;
                            ItemObj.GetTable("MTRL").Current["CCCLIGUARANTYMONTHS"] = item.WarrantyMonths;
                            ItemObj.GetTable("MTRL").Current["CCCESHOPMASTERCODE"] = item.EshopMasterCode;
                            ItemObj.GetTable("MTRL").Current["CCCHEIGHT"] = item.Height;
                            ItemObj.GetTable("MTRL").Current["CCCLENGTH"] = item.Length;
                            ItemObj.GetTable("MTRL").Current["CCCWIDTH"] = item.Width;
                            ItemObj.GetTable("MTRL").Current["VOLUME"] = item.ItemCubeM;
                            ItemObj.GetTable("MTREXTRA").Current["VARCHAR02"] = item.PhotoName;
                            ItemObj.GetTable("MTRL").Current["CCCWORKINPROGRESSINGR"] = item.WorkInProgressInGr; 
                            ItemObj.GetTable("MTRL").Current["CCCTOBEPUBLISHEDINGR"] = item.ToBePublishedInGr;
                            ItemObj.GetTable("MTRL").Current["CCCTOBEUNPUBLISHEDINGR"] = item.ToBeUnpublishedInGr;
                            ItemObj.GetTable("MTRL").Current["CCCHASTRANSLATION"] = item.HasTranslation;
                            ItemObj.GetTable("MTRL").Current["CCCISPUBLISHEDINGR"] = item.IsPublishedInGr;
                            ItemObj.GetTable("MTRL").Current["CCCTOBEPUBLISHEDINSKROUTZ"] = item.ToBePublishedInSkroutz;
                            ItemObj.GetTable("MTRL").Current["CCCTOBEPUBLISHEDINPUBLIC"] = item.ToBePublishedInPublic;


                            ////Πίνακας MTRSUBSTITUTE
                            //using (var mtrsubstitute = ItemObj.GetTable("MTRSUBSTITUTE"))
                            //{
                            //    mtrsubstitute.Current.Append();
                            //    mtrsubstitute.Current["CODE"] = item.code1;
                            //    mtrsubstitute.Current["NAME"] = item.name;
                            //    mtrsubstitute.Current["QTY1"] = Convert.ToDouble(1);
                            //    mtrsubstitute.Current["QTY2"] = Convert.ToDouble(1);
                            //    mtrsubstitute.Current.Post();
                            //}
                            ////Πίνακας MTRSUPCODE
                            //var supplier_id = supplier_list.Where(x => x.Code.ToUpper() == item.mtrsupcode.ToUpper()).FirstOrDefault()?.Id ?? 0;
                            //if (supplier_id > 0 && item.vendorno != "")
                            //{
                            //    using (var mtrsupcode = ItemObj.GetTable("MTRSUPCODE"))
                            //    {
                            //        mtrsupcode.Current.Append();
                            //        mtrsupcode.Current["TRDR"] = supplier_id;
                            //        var supcode = (item.theme + "_" + item.color + "_" + item.size.Replace(",", "."));
                            //        //var supcode = (item.vendorno + "_" + mtrl.ToString());
                            //        supcode = supcode.Substring(0, (supcode.Length >= 50 ? 50 : supcode.Length));
                            //        mtrsupcode.Current["MTRSUPCODE"] = supcode;
                            //        mtrsupcode.Current["CCCVENDORSIZE"] = item.suppliersize;
                            //        mtrsupcode.Current["CCCVENDORCODE"] = item.vendorno;
                            //        mtrsupcode.Current.Post();
                            //    }
                            //}
                            var newId =ItemObj.PostData();
                            newId = newId < 0 ? mtrl : newId;

                            //Images
                            if (item.ImagePath != "" && newId>0)
                            {
                                var queryimage = $@"SELECT REFOBJID,SOSOURCE,LNUM,LINENUM,DBWHOUSED,SODATA,DEFXTRDOC,XDOCTYPE,SOMD FROM XTRDOCDATA 
                                                    WHERE REFOBJID={newId} AND SOSOURCE = 51 AND LNUM=0 AND SOFNAME ='{item.ImagePath}' ";
                                using (var ds = _xSupport.GetSQLDataSet(queryimage, null))
                                { 
                                    try
                                    {
                                        if (ds.Count == 0)
                                        {
                                            var execsql = "";
                                            execsql = $"DELETE FROM XTRDOCDATA WHERE REFOBJID={newId} AND SOSOURCE = 51; ";
                                            execsql = execsql + $@"INSERT INTO XTRDOCDATA (REFOBJID,SOSOURCE,LNUM,LINENUM,DBWHOUSED,NAME,SOFNAME,DEFXTRDOC,XDOCTYPE,SOMD) 
                                                                    VALUES ({newId},51 ,0 ,1 ,0 ,'{item.PhotoName}','{item.ImagePath}' ,0 ,0 ,0); ";
                                            _xSupport.ExecuteSQL(execsql);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο είδος με Κωδικό «{item.Code}». " + ex.Message);
                        }
                    }
                }
            }
        }
    }
}
