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
                            UNION ALL SELECT 'item' AS OBJ, MTRL AS ID, CODE, NAME FROM MTRL WHERE /*ISACTIVE = 1 AND*/ COMPANY = {_xSupport.ConnectionInfo.CompanyId}  AND SODTYPE = 51 AND ISNULL(CCCITEMCOMPANY,0)=2
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
        public void CreateUpdateItems(List<ItemMasterRecord> exceldata, List<SqlData> sqlData)
        {
            var logs_remarks = "";
            if (exceldata.Count > 0)
            {
                var item_list = sqlData.Where(x => x.Obj == "item").ToList();
                var theme_list = sqlData.Where(x => x.Obj == "theme").ToList();
                var division_list = sqlData.Where(x => x.Obj == "division").ToList();
                var department_list = sqlData.Where(x => x.Obj == "department").ToList();
                var subdepartment_list = sqlData.Where(x => x.Obj == "subdepartment").ToList();
                var class_list = sqlData.Where(x => x.Obj == "class").ToList();
                var size_list = sqlData.Where(x => x.Obj == "size").ToList();
                var color_list = sqlData.Where(x => x.Obj == "color").ToList();
                var brand_list = sqlData.Where(x => x.Obj == "brand").ToList();
                var intrastat_list = sqlData.Where(x => x.Obj == "intrastat").ToList();
                var season_list = sqlData.Where(x => x.Obj == "season").ToList();
                var collection_list = sqlData.Where(x => x.Obj == "collection").ToList();
                var vat_list = sqlData.Where(x => x.Obj == "vat").ToList();
                var busunit_list = sqlData.Where(x => x.Obj == "busunit").ToList();
                var itemtype_list = sqlData.Where(x => x.Obj == "itemtype").ToList();
                var accountingtype_list = sqlData.Where(x => x.Obj == "accountingtype").ToList();
                var country_list = sqlData.Where(x => x.Obj == "country").ToList();
                var supplier_list = sqlData.Where(x => x.Obj == "supplier").ToList();
                
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
                            //ItemObj.GetTable("MTRL").Current["AssortmentDescription"] = item.AssortmentDescription;
                            //ItemObj.GetTable("MTRL").Current["SupplierCode"] = item.SupplierCode;

                            ItemObj.GetTable("MTRL").Current["NAME1"] = item.EnglishDescription;
                            ItemObj.GetTable("MTRL").Current["REMARKS"] = item.Comments;
                            ItemObj.GetTable("MTRL").Current["MTRUNIT1"] = 101;
                            ItemObj.GetTable("MTRL").Current["MTRUNIT2"] = item.PackageQuantity > 0 ? 108 : 101;
                            ItemObj.GetTable("MTRL").Current["MTRUNIT3"] = 101;
                            ItemObj.GetTable("MTRL").Current["MTRUNIT4"] = 101;
                            ItemObj.GetTable("MTRL").Current["MU21"] = item.PackageQuantity > 0 ? Convert.ToDouble(item.PackageQuantity) : 1;
                            ItemObj.GetTable("MTRL").Current["MU31"] = Convert.ToDouble(1);
                            ItemObj.GetTable("MTRL").Current["MU41"] = Convert.ToDouble(1);
                            ItemObj.GetTable("MTRL").Current["MU12MODE"] = 1;
                            ItemObj.GetTable("MTRL").Current["MU13MODE"] = 1;
                            ItemObj.GetTable("MTRL").Current["MU14MODE"] = 1;
                            //ItemObj.GetTable("MTRL").Current["ComposedOfQuantity"] = item.ComposedOfQuantity;

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

                            var brandId = brand_list.Where(x => x.Code.Trim() == "MC" + item.Brand.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCBRAND"] = brandId;

                            //ItemObj.GetTable("MTRL").Current["House"] = item.House;

                            //ItemObj.GetTable("MTRL").Current["VatCategory"] = item.VatCategory;
                            ItemObj.GetTable("MTRL").Current["VAT"] = 1410;

                            ItemObj.GetTable("MTRL").Current["CCCFASI"] = item.Phase;
                            ItemObj.GetTable("MTRL").Current["CCCSEASONALITY"] = item.Seasonality;
                            ItemObj.GetTable("MTRL").Current["CCCLISTUP"] = item.ListUp;

                            //ItemObj.GetTable("MTRL").Current["OUTLET"] = item.Outlet;

                            ItemObj.GetTable("MTRL").Current["GWEIGHT"] = item.NetWeight;

                            var countryId = country_list.Where(x => x.Code.Trim() == item.CountryOfOrigin.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["COUNTRY"] = countryId;

                            var intrastatId = intrastat_list.Where(x => x.Code.Trim() == item.Intrastat.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["INTRASTAT"] = intrastatId;

                            ItemObj.GetTable("MTRL").Current["ISACTIVE"] = item.Status == 0 ? 1 : 0;

                            var collectionId = collection_list.Where(x => x.Code.Trim() == item.Collection.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTREXTRA").Current["UTBL04"] = collectionId;

                            //ItemObj.GetTable("MTRL").Current["CommercialCollection"] = item.CommercialCollection;

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
                            ItemObj.PostData();
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
