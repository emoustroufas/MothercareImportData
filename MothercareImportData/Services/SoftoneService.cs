using MothercareImportData.Models;
using Softone;
using System;
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
                            UNION ALL SELECT 'item' AS OBJ, MTRL AS ID, CODE, NAME FROM MTRL WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  AND SODTYPE = 51
                            UNION ALL SELECT 'supplier' AS OBJ, TRDR AS ID, CODE, NAME FROM TRDR WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} AND SODTYPE=12";
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


        /*
        public void CreateBrand(List<BrandRecord> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                var queryMaxId = $@"SELECT ISNULL(MAX(CCCBRAND), 0) AS MAXID FROM CCCBRAND";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                //var listMaxId = dsMaxId != DBNull.Value ? (object[])dsMaxId : null;
                //var maxid = listMaxId != null ? Convert.ToInt32(listMaxId[0]) : 0;
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                try
                {
                    foreach (var brand in exceldata)
                    {
                        var brandcode = brand.Code;
                        var brandname = brand.Description;
                        maxid += 1;
                        try
                        {
                            var updquery = $@"INSERT INTO CCCBRAND ( CODE, NAME, ISACTIVE, COMPANY)
                                              VALUES ('{brandcode.Replace("'", "")}', '{brandname.Replace("'", "")}', 1,{_xSupport.ConnectionInfo.CompanyId})";
                            _xSupport.ExecuteSQL(updquery);
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο Brand «{brandname}»." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
        */
        public void E_CreateCollection(List<string> exceldata)
        {
            if (exceldata.Count() > 0)
            {
                var queryMaxCode = $@"SELECT ISNULL(MAX(CAST(CODE AS INT)),0) AS MAXCODE FROM CCCECOLLECTION WHERE ISNUMERIC(CODE)=1 AND COMPANY= {_xSupport.ConnectionInfo.CompanyId}";
                var dsMaxCode = _xSupport.SQL(queryMaxCode, null);
                var maxcode = dsMaxCode != null ? Convert.ToInt32(dsMaxCode) : 0;
                try
                {
                    foreach (var exd in exceldata)
                    {
                        maxcode += 1;
                        var exdname = exd;
                        try
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCECOLLECTION"))
                            {
                                ImpObj.InsertData();
                                ImpObj.AddParam("EshopUpdateParam", "1");
                                ImpObj.GetTable("CCCECOLLECTION").Current["CODE"] = maxcode.ToString();
                                ImpObj.GetTable("CCCECOLLECTION").Current["NAME"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCECOLLECTION").Current["NAMEENG"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCECOLLECTION").Current["ISACTIVE"] = 1;
                                ImpObj.PostData();
                            }
                        }
                        catch (Exception ex)
                        {
                            _xSupport.Exception($"Πρόβλημα στο Collection «{exdname}» του Eshop." + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _xSupport.Exception(ex.Message);
                }
            }
        }
    }
}
