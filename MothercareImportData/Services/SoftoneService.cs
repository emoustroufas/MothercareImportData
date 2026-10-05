using MothercareImportData.Models;
using NPOI.HSSF.Record;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using Org.BouncyCastle.Asn1.Cms;
using SixLabors.ImageSharp;
using Softone;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace MothercareImportData.Services
{
    public class SoftoneService
    {
        private XSupport _xSupport;
        private XModule _xModule;
        public SoftoneService(XSupport xSupport, XModule xModule)
        {
            _xSupport = xSupport;
            _xModule = xModule;
        }
        public void MarkStage(int number)
        {
            var results = _xModule.GetTable("RESULTS");
            //results.Current.Edit(0);
            results.Current["STAGE"] = number;
            results.Resync();
            _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 0, results.TablePtr);
            results.Current["STAGE"] = 4; //Χρειάζεται ένα στάδιο που δεν υπάρχει για να μην ξαναγράφει
        }
        public void ProgressNotify(int prmode, int prvalue)
        {
            var results = _xModule.GetTable("RESULTS");
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
            _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 0, results.TablePtr);
            //X.EXEC('CODE:ModuleIntf.SENDRESPONSE', X.MODULE, 0, RESULTS);
            results.Current["STAGE"] = 4;
        }
        public List<SqlData> GetSqlData()
        {
            var sqldata = new List<SqlData>();
            var query = $@"SELECT 'theme' AS OBJ,MTRMANFCTR AS ID,CODE,NAME, NULL AS FLG1 FROM MTRMANFCTR WHERE ISACTIVE=1 AND COMPANY={_xSupport.ConnectionInfo.CompanyId} 
                            UNION ALL SELECT 'division' AS OBJ, CCCDIVISION AS ID, CODE, NAME, NULL AS FLG1 FROM CCCDIVISION WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  
                            UNION ALL SELECT 'department' AS OBJ, CCCDEPARTMENT AS ID, CODE, NAME, NULL AS FLG1 FROM CCCDEPARTMENT WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  
                            UNION ALL SELECT 'subdept' AS OBJ, CCCSUBDEPT AS ID, CODE, NAME, NULL AS FLG1 FROM CCCSUBDEPT WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  
                            UNION ALL SELECT 'class' AS OBJ, CCCCLASS AS ID, CODE, NAME, NULL AS FLG1 FROM CCCCLASS WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  
                            UNION ALL SELECT 'size' AS OBJ, CCCMCSIZE AS ID, CODE, NAME, CCCSIZEGUIDE AS FLG1 FROM CCCMCSIZE WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} 
                            UNION ALL SELECT 'color' AS OBJ, CCCCOLOR AS ID, CODE, NAME, NULL AS FLG1 FROM CCCCOLOR WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  
                            UNION ALL SELECT 'brand' AS OBJ, CCCBRAND AS ID, CODE, NAME, NULL AS FLG1 FROM CCCBRAND WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} 
                            UNION ALL SELECT 'intrastat' AS OBJ, INTRASTAT AS ID, CODE, NAME, NULL AS FLG1 FROM INTRASTAT WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} 
                            UNION ALL SELECT 'season' AS OBJ, MTRSEASON AS ID, CODE, NAME, NULL AS FLG1 FROM MTRSEASON WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} 
                            UNION ALL SELECT 'collection' AS OBJ, UTBL04 AS ID, CODE, NAME, NULL AS FLG1 FROM UTBL04 WHERE ISACTIVE = 1 AND ISNULL(CCCISMC,0)=1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} AND SODTYPE = 51
                            UNION ALL SELECT 'vat' AS OBJ, VAT AS ID, CAST(PERCNT AS VARCHAR) AS CODE, NAME, NULL AS FLG1 FROM VAT WHERE ISACTIVE = 1 
                            UNION ALL SELECT 'busunit' AS OBJ, BUSUNITS AS ID, CODE, NAME, NULL AS FLG1 FROM BUSUNITS WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}
                            UNION ALL SELECT 'itemtype' AS OBJ, MTRCATEGORY AS ID, CODE, NAME, NULL AS FLG1 FROM MTRCATEGORY WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  AND SODTYPE = 51
                            UNION ALL SELECT 'accountingtype' AS OBJ, MTRACN AS ID, CODE, NAME, NULL AS FLG1 FROM MTRACN WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}  AND SODTYPE = 51
                            UNION ALL SELECT 'country' AS OBJ, COUNTRY AS ID, SHORTCUT AS CODE, NAME, NULL AS FLG1 FROM COUNTRY WHERE ISACTIVE = 1 
                            UNION ALL SELECT 'item' AS OBJ, MTRL AS ID, ISNULL(CCCMCOLDCODE,CODE) AS CODE, NAME, NULL AS FLG1 FROM MTRL WHERE COMPANY = {_xSupport.ConnectionInfo.CompanyId}  AND SODTYPE = 51 AND ISNULL(CCCITEMCOMPANY,0) IN (0,2)
                            UNION ALL SELECT 'supplier' AS OBJ, TRDR AS ID, CODE, NAME, NULL AS FLG1 FROM TRDR WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId} AND SODTYPE=12
                            UNION ALL SELECT 'sizeguide' AS OBJ, CCCSIZEGUIDE AS ID, CODE, NAME, NULL AS FLG1 FROM CCCSIZEGUIDE WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}
                            UNION ALL SELECT 'seasonality' AS OBJ, CCCSEASONALITY AS ID, CODE, NAME, NULL AS FLG1 FROM CCCSEASONALITY WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}
                            UNION ALL SELECT 'house' AS OBJ, CCCHOUSE AS ID, CODE, NAME, NULL AS FLG1 FROM CCCHOUSE WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}
                            UNION ALL SELECT 'commercialcollection' AS OBJ, CCCCOMMERCIALCOLLECTION AS ID, CODE, NAME, NULL AS FLG1 FROM CCCCOMMERCIALCOLLECTION WHERE ISACTIVE = 1 AND COMPANY = {_xSupport.ConnectionInfo.CompanyId}
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
                                Name = ds.GetAsString(i, "NAME"),
                                Flg1 = ds.GetAsInteger(i, "FLG1"),
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
        public List<BarcodeRecord> GetSqlBarcodes()
        {
            var barcodes = new List<BarcodeRecord>();
            var query = $@"SELECT MS.CODE AS BARCODE,ISNULL(M.CCCMCOLDCODE,M.CODE) AS CODE FROM MTRL M INNER JOIN MTRSUBSTITUTE MS ON MS.MTRL=M.MTRL WHERE M.COMPANY={_xSupport.ConnectionInfo.CompanyId} AND M.SODTYPE=51 AND M.CCCITEMCOMPANY IN (0,2)";
            using (var ds = _xSupport.GetSQLDataSet(query, null))
            {
                try
                {
                    if (ds.Count > 0)
                    {
                        for (int i = 0; i < ds.Count; i++)
                        {
                            var res = new BarcodeRecord
                            {
                                Barcode = ds.GetAsString(i, "BARCODE"),
                                ItemCode = ds.GetAsString(i, "CODE")
                            };
                            barcodes.Add(res);
                        }
                    }
                    return barcodes;
                }
                catch (Exception ex)
                {
                    return barcodes;
                    throw new Exception(ex.Message);
                }
            }
        }
        public List<SimilarItemRecord> GetSqlSimilarItems()
        { 
            var similaritems = new List<SimilarItemRecord>();
            var query = $@"SELECT ISNULL(M1.CCCMCOLDCODE,M1.CODE) AS ITEMCODE,ISNULL(M2.CCCMCOLDCODE,M2.CODE) AS REFERENCEITEMCODE FROM CCCSIMILARITEMS S
                            LEFT JOIN MTRL M1 ON M1.MTRL =S.MTRL LEFT JOIN MTRL M2 ON M2.MTRL = S.SIMMTRL
                            WHERE S.COMPANY={_xSupport.ConnectionInfo.CompanyId} AND M2.CODE IS NOT NULL AND M1.CODE IS NOT NULL";
            using (var ds = _xSupport.GetSQLDataSet(query, null))
            {
                try
                {
                    if (ds.Count > 0)
                    {
                        for (int i = 0; i < ds.Count; i++)
                        {
                            var res = new SimilarItemRecord
                            {
                                ItemCode = ds.GetAsString(i, "ITEMCODE"),
                                ReferenceItemCode = ds.GetAsString(i, "REFERENCEITEMCODE")
                            };
                            similaritems.Add(res);
                        }
                    }
                    return similaritems;
                }
                catch (Exception ex)
                {
                    return similaritems;
                    throw new Exception(ex.Message);
                }
            }
        }
        public string CreateDivision(List<DivisionRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στο Division «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateDepartment(List<DepartmentRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στο Department «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateSubdepartment(List<SubdepartmentRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    var exdcode = exd.Code;
                    var exdname = exd.Description;
                    try
                    {
                        using (var ImpObj = _xSupport.CreateModule("CCCSUBDEPT"))
                        {
                            ImpObj.InsertData();
                            ImpObj.GetTable("CCCSUBDEPT").Current["CODE"] = exd.DepartmentCode + "-" + exdcode;
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στο Subdepartment «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateClass(List<ClassRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    var exdcode = exd.Code;
                    var exdname = exd.Description;
                    try
                    {
                        using (var ImpObj = _xSupport.CreateModule("CCCCLASS"))
                        {
                            ImpObj.InsertData();
                            ImpObj.GetTable("CCCCLASS").Current["CODE"] = exd.DepartmentCode + "-" + exd.SubdeptCode + "-" + exdcode;
                            ImpObj.GetTable("CCCCLASS").Current["NAME"] = exdname.Replace("'", "");
                            ImpObj.GetTable("CCCCLASS").Current["NAMEENG"] = exdname.Replace("'", "");
                            ImpObj.GetTable("CCCCLASS").Current["ISACTIVE"] = 1;
                            var subdeptId = _xSupport.SQL($"SELECT CCCSUBDEPT FROM CCCSUBDEPT WHERE CODE='{exd.DepartmentCode + "-" + exd.SubdeptCode/*exd.SubdeptCode*/}' AND COMPANY={_xSupport.ConnectionInfo.CompanyId}", null);
                            if (subdeptId != null)
                            {
                                ImpObj.GetTable("CCCCLASS").Current["CCCSUBDEPT"] = subdeptId;
                            }
                            ImpObj.PostData();
                        }
                    }
                    catch (Exception ex)
                    {
                        logs_remarks = logs_remarks + $"Πρόβλημα στη Class «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateBrand(List<BrandRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στη Brand «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateCollection(List<CollectionRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                var queryMaxId = $@"SELECT ISNULL(MAX(UTBL04), 0) AS MAXID FROM UTBL04 WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId} AND SODTYPE=51";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                //var listMaxId = dsMaxId != DBNull.Value ? (object[])dsMaxId : null;
                //var maxid = listMaxId != null ? Convert.ToInt32(listMaxId[0]) : 0;
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var collection in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στη Συλλογή «{collectionname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateCommercialCollection(List<CommercialCollectionRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στην Εμπορική Συλλογή «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateBusinessUnit(List<BuRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                var queryMaxId = $@"SELECT ISNULL(MAX(BUSUNITS), 0) AS MAXID FROM BUSUNITS";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var bu in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στο BU «{buname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateItemType(List<ItemTypeRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                var queryMaxId = $@"SELECT ISNULL(MAX(MTRCATEGORY), 0) AS MAXID FROM MTRCATEGORY WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId} AND SODTYPE=51";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στον Τύπο Είδους «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateAccountingType(List<AccountingTypeRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                var queryMaxId = $@"SELECT ISNULL(MAX(MTRACN), 0) AS MAXID FROM MTRACN WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId} AND SODTYPE=51";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στον Τύπο Λογιστικής «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateSizeGuide(List<SizeGuideRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στο Μεγεθολόγιο «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        ////////////////////////////////////////
        public string UpdateSizeAttributes(List<SizeRecord> exceldata, List<SqlData> size_list, List<SqlData> sizeguide_list)
        {
            var logs_remarks = "";
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    var exdcode = exd.Code;
                    var exdname = exd.Description;
                    var sizeguidecode = exd.SizeGuideCode;
                    var attr29vcode = exd.Attribute29ValueCode;
                    var attr51vcode = exd.Attribute51ValueCode;
                    var sizeguideId = sizeguide_list.Where(x => x.Code.Trim() == sizeguidecode.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                    var orderbyno = exd.OrderByNo;
                    var sizeId = size_list.Where(x => x.Code.Trim() == exdcode.Trim()).FirstOrDefault()?.Id ?? null;
                    try
                    {
                        if (sizeId > 0)
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCMCSIZE"))
                            {
                                ImpObj.LocateData(sizeId);
                                ImpObj.GetTable("CCCMCSIZE").Current["NAME"] = exdname.Replace("'", "");
                                ImpObj.GetTable("CCCMCSIZE").Current["ORDERBY"] = orderbyno;
                                ImpObj.GetTable("CCCMCSIZE").Current["CCCSIZEGUIDE"] = sizeguideId;
                                ImpObj.GetTable("CCCMCSIZE").Current["ATTRIBUTE29VALCODE"] = attr29vcode;
                                ImpObj.GetTable("CCCMCSIZE").Current["ATTRIBUTE51VALCODE"] = attr51vcode;
                                ImpObj.PostData();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logs_remarks = logs_remarks + $"Πρόβλημα στο Μέγεθος «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }


        ////////////////////////////////////////
        public string CreateSize(List<SizeRecord> exceldata, List<SqlData> sizeguide_list)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    var exdcode = exd.Code;
                    var exdname = exd.Description;
                    var sizeguidecode = exd.SizeGuideCode;
                    var attr29vcode = exd.Attribute29ValueCode;
                    var attr51vcode = exd.Attribute51ValueCode;
                    var sizeguideId = sizeguide_list.Where(x => x.Code.Trim() == sizeguidecode.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                    var orderbyno = exd.OrderByNo;
                    try
                    {
                        using (var ImpObj = _xSupport.CreateModule("CCCMCSIZE"))
                        {
                            ImpObj.InsertData();
                            ImpObj.GetTable("CCCMCSIZE").Current["CODE"] = exdcode.ToString();
                            ImpObj.GetTable("CCCMCSIZE").Current["NAME"] = exdname.Replace("'", "");
                            ImpObj.GetTable("CCCMCSIZE").Current["ORDERBY"] = orderbyno;
                            ImpObj.GetTable("CCCMCSIZE").Current["CCCSIZEGUIDE"] = sizeguideId;
                            ImpObj.GetTable("CCCMCSIZE").Current["ISACTIVE"] = 1;
                            ImpObj.GetTable("CCCMCSIZE").Current["ATTRIBUTE29VALCODE"] = attr29vcode;
                            ImpObj.GetTable("CCCMCSIZE").Current["ATTRIBUTE51VALCODE"] = attr51vcode;
                            ImpObj.PostData();
                        }
                    }
                    catch (Exception ex)
                    {
                        logs_remarks = logs_remarks + $"Πρόβλημα στο Μέγεθος «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateSeasonality(List<SeasonalityRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στη Εποχικότητα «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateHouse(List<HouseRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
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
                        logs_remarks = logs_remarks + $"Πρόβλημα στον Οίκο «{exdname}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateUpdateItems(List<ItemMasterRecord> exceldata, List<SqlData> sqlData)
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

                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;

                using (var ItemObj = _xSupport.CreateModule("ITEM;Items Mothercare"))
                {
                    foreach (var item in exceldata)
                    {
                        counter++;
                        ItemObj.SetFieldEditor("ITEM.MTRMANFCTR", null);
                        ItemObj.SetFieldEditor("ITEM.MTRACN", null);
                        ItemObj.SetFieldEditor("ITEM.COUNTRY", null);
                        ItemObj.SetFieldEditor("ITEM.MTRCATEGORY", null);

                        ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                        try
                        {
                            var mtrl_list = item_list.Where(x => x.Code.Trim() == item.Code.Trim()).FirstOrDefault();
                            var mtrl = mtrl_list != null ? mtrl_list.Id : 0;
                            if (mtrl == 0)
                            {
                                ItemObj.InsertData();
                                ItemObj.GetTable("MTRL").Current["CODE"] = "MC"+item.Code;
                                ItemObj.GetTable("MTRL").Current["CCCMCOLDCODE"] = item.Code;
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
                            switch (item.UnitOfMeasure)
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
                            if (item.Division.ToString() != "" && divisionId == null)
                            {
                                logs_remarks = logs_remarks + $"Το Division με Κωδικό «{item.Division}» δεν υπάρχει στο Softone για το είδος με κωδικό «{item.Code}»." + Environment.NewLine;
                            }
                            var departmentId = department_list.Where(x => x.Code.Trim() == item.Department.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCDEPARTMENT"] = departmentId;
                            if (item.Department.ToString() !="" && departmentId == null)
                            {
                                logs_remarks = logs_remarks + $"Το Department με Κωδικό «{item.Department}» δεν υπάρχει στο Softone για το είδος με κωδικό «{item.Code}»." + Environment.NewLine;
                            }
                            var subdeptId = subdepartment_list.Where(x => x.Code.Trim() == item.Department.ToString().Trim() + "-" + item.Subdept.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCSUBDEPT"] = subdeptId;
                            if (item.Subdept.ToString() != "" && subdeptId == null)
                            {
                                logs_remarks = logs_remarks + $"Το Subdept με Κωδικό «{item.Subdept}» δεν υπάρχει στο Softone για το είδος με κωδικό « {item.Code} »." + Environment.NewLine;
                            }
                            var classId = class_list.Where(x => x.Code.Trim() == item.Department.ToString().Trim() + "-" + item.Subdept.ToString().Trim() + "-" + item.Class.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCCLASS"] = classId;
                            if (item.Class.ToString() != "" && classId == null)
                            {
                                logs_remarks = logs_remarks + $"Η Class με Κωδικό «{item.Class}» δεν υπάρχει στο Softone για το είδος με κωδικό « {item.Code} »." + Environment.NewLine;
                            }
                            ItemObj.GetTable("MTRL").Current["CCCYEAR"] = item.Year;
                            ItemObj.GetTable("MTRL").Current["CCCQUARTER"] = item.Season;
                            ItemObj.GetTable("MTRL").Current["CCCCURYEAR"] = item.StatisticalYear;
                            var mtrmanufacturerId = theme_list.Where(x => x.Name.Trim() == item.StyleNo.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["MTRMANFCTR"] = mtrmanufacturerId;
                            if (item.StyleNo.ToString() != "" && mtrmanufacturerId == null)
                            {
                                logs_remarks = logs_remarks + $"Το StyleNo με Κωδικό «{item.StyleNo}» δεν υπάρχει στο Softone για το είδος με κωδικό « {item.Code} »." + Environment.NewLine;
                            }
                            //var colorId = 0;//item.Color;
                            //ItemObj.GetTable("MTRL").Current["CCCCOLOR"] = colorId;
                            var sizeguideId = sizeguide_list.Where(x => x.Code.Trim() == item.SizeGuide.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCSIZEGUIDE"] = sizeguideId;
                            if (item.SizeGuide.ToString() != "" && sizeguideId == null)
                            {
                                logs_remarks = logs_remarks + $"Το Μεγεθολόγιο με Κωδικό «{item.SizeGuide}» δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
                            var sizeId = size_list.Where(x => x.Code.Trim() == item.Size.ToString().Trim() && x.Flg1 == sizeguideId).FirstOrDefault()?.Id ?? null;//item.Size;
                            ItemObj.GetTable("MTRL").Current["CCCMCSIZE"] = sizeId;
                            if (item.Size.ToString() != "" && sizeId == null)
                            {
                                logs_remarks = logs_remarks + $"Το Μέγεθος με Κωδικό «{item.Size}» για το Μεγεθολόγιο με Κωδικό «{item.SizeGuide}», δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
                            var brandId = brand_list.Where(x => x.Code.Trim() == "MC" + item.Brand.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCBRAND"] = brandId;
                            if (item.Brand.ToString() != "" && brandId == null)
                            {
                                logs_remarks = logs_remarks + $"Το Brand με Κωδικό «{item.Brand}» δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
                            var houseId = house_list.Where(x => x.Code.Trim() == item.House.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCHOUSE"] = houseId;
                            if (item.House.ToString() != "" && houseId == null)
                            {
                                logs_remarks = logs_remarks + $"Ο Οίκος με Κωδικό «{item.House}» δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
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
                            if ((item.Seasonality.ToString() != "" && item.Seasonality.ToString() != "0") && seasonalityId == null)
                            {
                                logs_remarks = logs_remarks + $"Η Εποχικότητα με Κωδικό «{item.Seasonality}» δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
                            ItemObj.GetTable("MTRL").Current["CCCLISTUP"] = item.ListUp;
                            DateTime nulldate = new DateTime(1899, 12, 30);
                            if (item.Outlet != null)
                            {
                                ItemObj.GetTable("MTRL").Current["CCCOUTLET"] = item.Outlet <= nulldate ? (DateTime?)null : item.Outlet;
                            }
                            ItemObj.GetTable("MTRL").Current["GWEIGHT"] = item.NetWeight;
                            var countryId = country_list.Where(x => x.Code.Trim() == item.CountryOfOrigin.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["COUNTRY"] = countryId;
                            if (item.CountryOfOrigin.ToString() != "" && countryId == null)
                            {
                                logs_remarks = logs_remarks + $"Η Χώρα Προέλευσης με Κωδικό «{item.CountryOfOrigin}» δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
                            var intrastatId = intrastat_list.Where(x => x.Code.Trim() == item.Intrastat.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["INTRASTAT"] = intrastatId;
                            if (item.Intrastat.ToString() != "" && intrastatId == null)
                            {
                                logs_remarks = logs_remarks + $"Το Intrastat με Κωδικό «{item.Intrastat}» δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
                            ItemObj.GetTable("MTRL").Current["ISACTIVE"] = 1;
                            ItemObj.GetTable("MTRL").Current["CCCMSSTATUS"] = item.Status;
                            var collectionId = collection_list.Where(x => x.Code.Trim() == item.Collection.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTREXTRA").Current["UTBL04"] = collectionId;
                            if (item.Collection.ToString() != "" && collectionId == null)
                            {
                                logs_remarks = logs_remarks + $"Η Συλλογή με Κωδικό «{item.Collection}» δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
                            var commercialcollectionId = commercialcollection_list.Where(x => x.Code.Trim() == item.CommercialCollection.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["CCCCOMMERCIALCOLLECTION"] = commercialcollectionId;
                            if (item.CommercialCollection.ToString() != "" && commercialcollectionId == null)
                            {
                                logs_remarks = logs_remarks + $"Η Εμπ. Συλλογή με Κωδικό «{item.CommercialCollection}» δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
                            var buId = busunit_list.Where(x => x.Code.Trim() == "MC" + item.Bu.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["BUSUNITS"] = buId;
                            if (item.Bu.ToString() != "" && buId == null)
                            {
                                logs_remarks = logs_remarks + $"Το BU με Κωδικό «{item.Bu}» δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
                            var itemtypeId = itemtype_list.Where(x => x.Code.Trim() == "MC" + item.ItemType.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["MTRCATEGORY"] = itemtypeId;
                            if (item.ItemType.ToString() != "" && itemtypeId == null)
                            {
                                logs_remarks = logs_remarks + $"Ο Τύπος Είδους με Κωδικό «{item.ItemType}» δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
                            var accountingtypeId = accountingtype_list.Where(x => x.Code.Trim() == "MC" + item.AccountingType.ToString().Trim()).FirstOrDefault()?.Id ?? null;
                            ItemObj.GetTable("MTRL").Current["MTRACN"] = accountingtypeId;
                            if (item.AccountingType.ToString() != "" && accountingtypeId == null)
                            {
                                logs_remarks = logs_remarks + $"Ο Τύπος για λογιστική με Κωδικό «{item.AccountingType}» δεν υπάρχει στο Softone για το είδος με κωδικό «  {item.Code}  »." + Environment.NewLine;
                            }
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
                            ////if (data.eDescription != "")
                            ////{
                            ////    var htmldescription = "<html><head><meta http-equiv=" + "\"Content - Type\"" + " content =" + "\"text / html; charset = windows - 1253\"" + " ><title></title><style></style></head><body>" +
                            ////        data.eDescription.ToString() + "</body></html>";
                            ////    ItemObj.GetTable("MTRL").Current["CCCHTMLGRDESCRIPTIONTXT"] = htmldescription;
                            ////    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(htmldescription);
                            ////    ItemObj.GetTable("MTRL").Current["CCCHTMLGRDESCRIPTION"] = bytes;
                            ////}
                            ItemObj.SetFieldEditor("ITEM.MTRMANFCTR", "MTRMANFCTR");
                            ItemObj.SetFieldEditor("ITEM.MTRACN", "ITEMGL");
                            ItemObj.SetFieldEditor("ITEM.COUNTRY", "COUNTRY");
                            ItemObj.SetFieldEditor("ITEM.MTRCATEGORY", "ITECATEGORY");
                            var newId = ItemObj.PostData();
                            newId = newId < 0 ? mtrl : newId;
                            //Images
                            if (item.ImagePath != "" && newId > 0)
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
                                        logs_remarks = logs_remarks + $"Πρόβλημα στην εικόνα «{item.PhotoName}» του είδους με Κωδικό «{item.Code}»." + ex.Message + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logs_remarks = logs_remarks + $"Πρόβλημα στο είδος με Κωδικό «{item.Code}»." + ex.Message + Environment.NewLine;
                        }
                    }
                    MarkStage(2);
                    MarkStage(3);
                    MarkStage(0);
                    ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                    var resultsprocess = "result1,result2,result3";
                    _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
                }
            }
            return logs_remarks;
        }
        public string UpdateSupBarcodes(List<SupBarcodeRecord> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var exd in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    var exdtaxcode = exd.TaxCode;
                    var exdsupbarcode = exd.SupBarcode;
                    try
                    {
                        var updquery = $@"UPDATE MTRL SET CCCSUPBARCODE='{exdsupbarcode}' WHERE CODE2='{exdtaxcode}' AND COMPANY={_xSupport.ConnectionInfo.CompanyId}";
                        _xSupport.ExecuteSQL(updquery);
                    }
                    catch (Exception ex)
                    {
                        logs_remarks = logs_remarks + $"Πρόβλημα στο Barcode Προμηθευτή «{exdsupbarcode}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string SetSimilarItems(List<SimilarItemRecord> exceldata, List<SqlData> item_list)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                var grouped = exceldata.GroupBy(r => r.ItemCode).Select(g => new SimilarItemGroup
                {
                    ItemCode = g.Key,
                    ReferenceItemCodes = g.Select(r => r.ReferenceItemCode).ToList()
                }).ToList();
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, grouped.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var similar in grouped)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    try
                    {
                        var mtrl_list = item_list.Where(x => x.Code.Trim() == similar.ItemCode.Trim()).FirstOrDefault();
                        var mtrl = mtrl_list != null ? mtrl_list.Id : 0;
                        var mname = mtrl_list != null ? mtrl_list.Name : "";
                        var similarCodes = similar.ReferenceItemCodes.Where(code => item_list.Any(item => item.Code.Trim() == code.Trim())).ToList();

                        if (mtrl > 0 && similarCodes.Count > 0)
                        {
                            var queryMaxId = $@"SELECT ISNULL(MAX(LINENUM),0) AS MAXID FROM CCCSIMILARITEMS WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId} AND MTRL={mtrl}";
                            var dsMaxId = _xSupport.SQL(queryMaxId, null);
                            var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                            foreach (var code in similarCodes)
                            {
                                //05/10/2026
                                var similarItem_list = item_list.Where(x => x.Code.Trim() == code.Trim()).FirstOrDefault();
                                var similarItem = similarItem_list != null ? similarItem_list.Id : 0;
                                if (similarItem == 0)
                                {
                                    logs_remarks = logs_remarks + $"Δεν υπάρχει το όμοιο είδος «{similar.ItemCode}» για το είδος «{code}»." + Environment.NewLine;
                                    continue;
                                }
                                else
                                {
                                    maxid += 1;
                                    var updquery = $@"INSERT INTO CCCSIMILARITEMS (COMPANY,MTRL,LINENUM,SIMMTRL,SIMILARITY)
                                                      VALUES ({_xSupport.ConnectionInfo.CompanyId},{mtrl},{maxid},{similarItem},{Convert.ToDouble(100)});";
                                    _xSupport.ExecuteSQL(updquery);
                                }
                            }
                            //using (var ItemObj = _xSupport.CreateModule("ITEM;Items Mothercare"))
                            //{
                            //    ItemObj.LocateData(mtrl);
                            //    //Πίνακας CCCSIMILARITEMS
                            //    using (var mtrsimilar = ItemObj.GetTable("CCCSIMILARITEMS"))
                            //    {
                            //        foreach (var code in similarCodes)
                            //        {
                            //            var similarItem_list = item_list.Where(x => x.Code.Trim() == code.Trim()).FirstOrDefault();
                            //            var similarItem = similarItem_list != null ? similarItem_list.Id : 0;
                            //            var recNo1 = mtrsimilar.Find("SIMMTRL", similarItem);
                            //            if (recNo1 == -1)
                            //            {
                            //                mtrsimilar.Current.Append();
                            //                mtrsimilar.Current["SIMMTRL"] = similarItem;
                            //                mtrsimilar.Current["SIMILARITY"] = Convert.ToDouble(100);
                            //                mtrsimilar.Current.Post();
                            //            }
                            //        }
                            //    }
                            //    ItemObj.PostData();
                            //}
                        }
                        else
                        {
                            if (mtrl == 0)
                            {
                                logs_remarks = logs_remarks + $"Δεν υπάρχει το είδος «{similar.ItemCode}»." + Environment.NewLine;
                            }
                            var notexistsimilarcodeslist = similar.ReferenceItemCodes.Where(code => !item_list.Any(item => item.Code.Trim() == code.Trim())).ToList();
                            if (notexistsimilarcodeslist.Count > 0)
                            {
                                var notexistsimilarcodes = String.Join(",", notexistsimilarcodeslist);
                                logs_remarks = logs_remarks + $"Δεν υπάρχουν οι κωδικοί ({notexistsimilarcodes}) για το είδος «{similar.ItemCode}»." + Environment.NewLine;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logs_remarks = logs_remarks + $"Πρόβλημα στο Όμοιο Είδος «{similar.ItemCode}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string ImportBarcode(List<BarcodeRecord> exceldata, List<SqlData> item_list)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var barcode in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    try
                    {
                        var mtrl_list = item_list.Where(x => x.Code.Trim() == barcode.ItemCode.Trim()).FirstOrDefault();
                        var mtrl = mtrl_list != null ? mtrl_list.Id : 0;
                        var mname = mtrl_list != null ? mtrl_list.Name : "";
                        if (mtrl > 0)
                        {
                            //05/10/2026
                            var queryMaxId = $@"SELECT ISNULL(MAX(LINENUM),0) AS MAXID FROM MTRSUBSTITUTE WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId} AND MTRL={mtrl}";
                            var dsMaxId = _xSupport.SQL(queryMaxId, null);
                            var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                            maxid += 1;
                            var updquery = $@"INSERT INTO MTRSUBSTITUTE (COMPANY,MTRL,LINENUM,CODE,NAME,QTY1,QTY2,ISACTIVE,INSDATE)
                                              VALUES ({_xSupport.ConnectionInfo.CompanyId},{mtrl},{maxid},'{barcode.Barcode}','{mname.Replace("'", "΄")}',1,1,1,GETDATE());";
                            _xSupport.ExecuteSQL(updquery);
                        }
                        else
                        {
                            logs_remarks = logs_remarks + $"Δεν υπάρχει το είδος «{barcode.ItemCode}» με Barcode «{barcode.Barcode}»." + Environment.NewLine;
                        }
                        //using (var ItemObj = _xSupport.CreateModule("ITEM;Items Mothercare"))
                        //{
                        //    var mtrl_list = item_list.Where(x => x.Code.Trim() == barcode.ItemCode.Trim()).FirstOrDefault();
                        //    var mtrl = mtrl_list != null ? mtrl_list.Id : 0;
                        //    var mname = mtrl_list != null ? mtrl_list.Name : "";
                        //    if (mtrl > 0)
                        //    {
                        //        ItemObj.LocateData(mtrl);
                        //        //Πίνακας MTRSUBSTITUTE
                        //        using (var mtrsubstitute = ItemObj.GetTable("MTRSUBSTITUTE"))
                        //        {
                        //            var recNo1 = mtrsubstitute.Find("CODE", barcode.Barcode);
                        //            if (recNo1 == -1)
                        //            {
                        //                mtrsubstitute.Current.Append();
                        //                mtrsubstitute.Current["CODE"] = barcode.Barcode;
                        //                mtrsubstitute.Current["NAME"] = mname;
                        //                mtrsubstitute.Current["QTY1"] = Convert.ToDouble(1);
                        //                //mtrsubstitute.Current["QTY2"] = Convert.ToDouble(1);
                        //                mtrsubstitute.Current.Post();
                        //            }
                        //        }
                        //        ItemObj.PostData();
                        //    }
                        //    else
                        //    {
                        //        logs_remarks = logs_remarks + $"Δεν υπάρχει το είδος «{barcode.ItemCode}» με Barcode «{barcode.Barcode}»." + Environment.NewLine;
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                        logs_remarks = logs_remarks + $"Πρόβλημα στο Barcode «{barcode.Barcode}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateIntrastat(List<string> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                var queryMaxId = $@"SELECT ISNULL(MAX(INTRASTAT), 0) AS MAXID FROM INTRASTAT WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId}";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                foreach (var intrastat in exceldata)
                {
                    var intrastatcode = intrastat.Length>8 ? intrastat.Substring(0, 8) : intrastat;
                    if (intrastatcode != "")
                    {
                        counter++;
                        ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                        maxid += 1;
                        try
                        {
                            var updquery = $@"INSERT INTO INTRASTAT (INTRASTAT, CODE, NAME, ISACTIVE, COMPANY)
                                              VALUES ({maxid},'{intrastatcode}','{intrastat.Replace("'", "")}',1,{_xSupport.ConnectionInfo.CompanyId})";
                            _xSupport.ExecuteSQL(updquery);
                        }
                        catch (Exception ex)
                        {
                            logs_remarks = logs_remarks + $"Πρόβλημα στο Intrastat «{intrastatcode}»." + ex.Message + Environment.NewLine;
                        }
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string CreateTheme(List<string> exceldata)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                var queryMaxId = $@"SELECT ISNULL(MAX(MTRMANFCTR), 0) AS MAXID FROM MTRMANFCTR WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId}";
                var dsMaxId = _xSupport.SQL(queryMaxId, null);
                var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;

                foreach (var theme in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    maxid += 1;
                    var themecode = "MC" + maxid;
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
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------------//Eshop//-------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------//
        public List<ProductAttributeRecord> GetSqlProductAttributeData()
        {
            var sqldata = new List<ProductAttributeRecord>();
            try
            {
                var query = $@"SELECT M.MTRL,
                                TH.CCCLANGUAGE AS CN_LANG_CD,
                                ISNULL(M.CCCMCOLDCODE,M.CODE) AS AP_EIDH_CD,
                                TH.TRANSLATION,
                                MA.CODE AS AP_ATTR0_CD,
                                MAL.CODE AS AP_ATTR1_CD,
                                ISNULL(FT.FREETXT,'') AS FREE_TEXT
                                FROM MTRL M 
                                LEFT JOIN MTRLATTRIBUTES MAS ON MAS.MTRL=M.MTRL
                                LEFT JOIN CCCATTIBUTETRANSLATION TH ON TH.MTRATTRIBUTE=MAS.MTRATTRIBUTE AND TH.DATATYPE=1
                                LEFT JOIN MTRATTRIBUTE MA ON MA.MTRATTRIBUTE=MAS.MTRATTRIBUTE
                                LEFT JOIN MTRATTRIBUTELN MAL ON MAL.MTRATTRIBUTE =MAS.MTRATTRIBUTE AND MAL.MTRATTRIBUTELN=MAS.MTRATTRIBUTELN
                                LEFT JOIN CCCATTRIBUTEFREETEXT FT ON FT.MTRATTRIBUTE = MAS.MTRATTRIBUTE AND FT.CCCLANGUAGE=TH.CCCLANGUAGE AND FT.MTRL=M.MTRL
                                WHERE M.COMPANY={_xSupport.ConnectionInfo.CompanyId} AND TH.CCCLANGUAGE IS NOT NULL
                                GROUP BY M.MTRL,TH.CCCLANGUAGE,ISNULL(M.CCCMCOLDCODE,M.CODE),TH.TRANSLATION,MA.CODE,MAL.CODE,ISNULL(FT.FREETXT,'')
                                ORDER BY M.MTRL,MA.CODE,TH.CCCLANGUAGE,MAL.CODE";
                    //$@"SELECT M.MTRL,TH.CCCLANGUAGE AS CN_LANG_CD,
                    //        ISNULL(M.CCCMCOLDCODE,M.CODE) AS AP_EIDH_CD,
                    //        ISNULL(TH.TRANSLATION,'') AS TRANSLATION,
                    //        ISNULL(MA.CODE,'') AS AP_ATTR0_CD,
                    //        ISNULL(MAL.CODE,'') AS AP_ATTR1_CD,
                    //        ISNULL(FT.FREETXT,'') AS FREE_TEXT
                    //        FROM MTRL M
                    //        LEFT JOIN MTRLATTRIBUTES MAS ON MAS.MTRL=M.MTRL
                    //        LEFT JOIN CCCATTIBUTETRANSLATION TH ON TH.MTRATTRIBUTE=MAS.MTRATTRIBUTE AND TH.DATATYPE=1
                    //        LEFT JOIN MTRATTRIBUTE MA ON MA.MTRATTRIBUTE=MAS.MTRATTRIBUTE
                    //        LEFT JOIN MTRATTRIBUTELN MAL ON MAL.MTRATTRIBUTE =MAS.MTRATTRIBUTE AND MAL.MTRATTRIBUTELN=MAS.MTRATTRIBUTELN
                    //        LEFT JOIN CCCATTRIBUTEFREETEXT FT ON FT.MTRATTRIBUTE = MAS.MTRATTRIBUTE AND FT.CCCLANGUAGE=TH.CCCLANGUAGE --AND FT.MTRATTRIBUTELN=MAL.MTRATTRIBUTELN
                    //        WHERE M.COMPANY={_xSupport.ConnectionInfo.CompanyId} AND TH.CCCLANGUAGE IS NOT NULL";
                using (var ds = _xSupport.GetSQLDataSet(query, null))
                {
                    try
                    {
                        if (ds.Count > 0)
                        {
                            for (int i = 0; i < ds.Count; i++)
                            {
                                var res = new ProductAttributeRecord
                                {
                                    LanguageCode = ds.GetAsInteger(i, "CN_LANG_CD"),
                                    ProductCode = ds.GetAsString(i, "AP_EIDH_CD"),
                                    AttributeCode = ds.GetAsString(i, "AP_ATTR0_CD"),
                                    AttributeValueCode = ds.GetAsString(i, "AP_ATTR1_CD"),
                                    FreeText = ds.GetAsString(i, "FREE_TEXT")
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
            catch (Exception ex)
            {
                return sqldata;
                throw new Exception(ex.Message);
            }
        }
        public List<AttributeRecord> GetSqlAttributeData()
        {
            var sqldata = new List<AttributeRecord>();
            try
            {
                var query = $@"SELECT A.MTRATTRIBUTE, A.CODE AS AttributeCode, 
                            AT.CCCLANGUAGE AS AttributeLanguageCode,AT.TRANSLATION AS AttributeTranslation,
                            V.MTRATTRIBUTELN,V.CODE AS ValueCode,
                            VT.CCCLANGUAGE AS ValueLanguageCode,VT.TRANSLATION AS ValueTranslation
                            FROM MTRATTRIBUTE A
                            LEFT JOIN CCCATTIBUTETRANSLATION AT ON AT.MTRATTRIBUTE = A.MTRATTRIBUTE AND AT.COMPANY = A.COMPANY AND ISNULL(AT.CCCLANGUAGE, 0) <> 0 AND AT.DATATYPE = 1
                            LEFT JOIN MTRATTRIBUTELN V ON V.MTRATTRIBUTE = A.MTRATTRIBUTE AND V.COMPANY = A.COMPANY AND V.ISACTIVE = 1
                            LEFT JOIN CCCATTIBUTETRANSLATION VT ON VT.MTRATTRIBUTE = V.MTRATTRIBUTE AND VT.MTRATTRIBUTELN = V.MTRATTRIBUTELN AND VT.COMPANY = V.COMPANY AND ISNULL(VT.CCCLANGUAGE, 0) <> 0 AND VT.DATATYPE = 2
                            WHERE A.COMPANY = {_xSupport.ConnectionInfo.CompanyId} AND A.ISACTIVE = 1
                            ORDER BY A.MTRATTRIBUTE,V.MTRATTRIBUTELN,AT.CCCLANGUAGE,VT.CCCLANGUAGE";
                using (var ds = _xSupport.GetSQLDataSet(query, null))
                {
                    for (int i = 0; i < ds.Count; i++)
                    {
                        int attributeId = ds.GetAsInteger(i, "MTRATTRIBUTE");
                        string attributeCode = ds.GetAsString(i, "AttributeCode");
                        // ==========================================
                        // Attribute
                        // ==========================================
                        var attribute = sqldata.FirstOrDefault(x => x.SoftOneId == attributeId);
                        if (attribute == null)
                        {
                            attribute = new AttributeRecord
                            {
                                SoftOneId = attributeId,
                                Code = attributeCode
                            };
                            sqldata.Add(attribute);
                        }
                        // ==========================================
                        // Attribute Translation
                        // ==========================================
                        int attributeLanguage = ds.GetAsInteger(i, "AttributeLanguageCode");
                        string attributeDescription = ds.GetAsString(i, "AttributeTranslation");
                        if (attributeLanguage != 0 && !attribute.Translations.Any(x => x.LanguageCode == attributeLanguage) && attributeDescription != "")
                        {
                            attribute.Translations.Add(
                                new AttributeTranslation
                                {
                                    LanguageCode = attributeLanguage,
                                    Description = attributeDescription
                                });
                        }
                        // ==========================================
                        // Attribute Values
                        // ==========================================
                        int valueId = ds.GetAsInteger(i, "MTRATTRIBUTELN");
                        if (valueId == 0)
                            continue;
                        string valueCode = ds.GetAsString(i, "ValueCode");
                        var attributeValue = attribute.Values.FirstOrDefault(x => x.SoftOneId == valueId);
                        if (attributeValue == null)
                        {
                            attributeValue = new AttributeValue
                            {
                                SoftOneId = valueId,
                                Code = valueCode
                            };
                            attribute.Values.Add(attributeValue);
                        }
                        // ==========================================
                        // Attribute Value Translation
                        // ==========================================
                        int valueLanguage = ds.GetAsInteger(i, "ValueLanguageCode");
                        string valueDescription = ds.GetAsString(i, "ValueTranslation");
                        if (valueLanguage != 0 && !attributeValue.Translations.Any(x => x.LanguageCode == valueLanguage) && valueDescription != "")
                        {
                            attributeValue.Translations.Add(
                                new AttributeValueTranslation
                                {
                                    LanguageCode = valueLanguage,
                                    Description = valueDescription
                                });
                        }
                    }
                }
                //var test = sqldata.OrderBy(x => x.Code);
                return sqldata;
            }
            catch (Exception ex)
            {
                return sqldata;
                throw new Exception(ex.Message);
            }
        }
        //public List<AttributeRecord> GetSqlAttributeData()
        //{
        //    var sqldata = new List<AttributeRecord>();
        //    try
        //    {
        //        //Attributes
        //        var attributelist = new List<AttributeRecord>();
        //        var queryAttributes = $@"SELECT MTRATTRIBUTE,CODE,NAME FROM MTRATTRIBUTE WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId} AND ISACTIVE=1";
        //        using (var dsAttributes = _xSupport.GetSQLDataSet(queryAttributes, null))
        //        {
        //            try
        //            {
        //                if (dsAttributes.Count > 0)
        //                {
        //                    for (int i = 0; i < dsAttributes.Count; i++)
        //                    {
        //                        var mtrattributeId = dsAttributes.GetAsInteger(i, "MTRATTRIBUTE");
        //                        //Attribute Translation
        //                        var attributetranslationlist = new List<AttributeTranslation>();
        //                        var queryAttributeTranslation = $@"SELECT MTRATTRIBUTE,CCCLANGUAGE,TRANSLATION FROM CCCATTIBUTETRANSLATION 
        //                                                            WHERE MTRATTRIBUTE={mtrattributeId} AND COMPANY={_xSupport.ConnectionInfo.CompanyId} 
        //                                                            AND ISNULL(CCCLANGUAGE,0)<>0 AND DATATYPE=1";
        //                        using (var dsAttributeTranslation = _xSupport.GetSQLDataSet(queryAttributeTranslation, null))
        //                        {
        //                            if (dsAttributeTranslation != null)
        //                            {
        //                                try
        //                                {
        //                                    if (dsAttributeTranslation.Count > 0)
        //                                    {
        //                                        for (int j = 0; j < dsAttributeTranslation.Count; j++)
        //                                        {
        //                                            var resAttributeTranslation = new AttributeTranslation
        //                                            {
        //                                                LanguageCode = dsAttributeTranslation.GetAsInteger(j, "CCCLANGUAGE"),
        //                                                Description = dsAttributeTranslation.GetAsString(j, "TRANSLATION")
        //                                            };
        //                                            attributetranslationlist.Add(resAttributeTranslation);
        //                                        }
        //                                    }
        //                                }
        //                                catch (Exception ex)
        //                                {
        //                                    throw new Exception(ex.Message);
        //                                }
        //                            }
        //                        }
        //                        //Attribute Values
        //                        var attributevaluelist = new List<AttributeValue>();
        //                        var queryAttributeValues = $@"SELECT MTRATTRIBUTE,MTRATTRIBUTELN,CODE,SOVALUE FROM MTRATTRIBUTELN 
        //                                                      WHERE MTRATTRIBUTE={mtrattributeId} AND COMPANY={_xSupport.ConnectionInfo.CompanyId} AND ISACTIVE=1";
        //                        using (var dsAttributeValues = _xSupport.GetSQLDataSet(queryAttributeValues, null))
        //                        {
        //                            try
        //                            {
        //                                if (dsAttributeValues.Count > 0)
        //                                {
        //                                    for (int v = 0; v < dsAttributeValues.Count; v++)
        //                                    {
        //                                        var mtrattributevalueId = dsAttributeValues.GetAsInteger(v, "MTRATTRIBUTELN");
        //                                        //Attribute Value Translation
        //                                        var attributevaluetranslationlist = new List<AttributeValueTranslation>();
        //                                        var queryAttributeValueTranslation = $@"SELECT MTRATTRIBUTE,MTRATTRIBUTELN,CCCLANGUAGE,TRANSLATION FROM CCCATTIBUTETRANSLATION 
        //                                                                                WHERE MTRATTRIBUTE={mtrattributeId} AND MTRATTRIBUTELN={mtrattributevalueId} 
        //                                                                                AND COMPANY={_xSupport.ConnectionInfo.CompanyId} AND ISNULL(CCCLANGUAGE,0)<>0 AND DATATYPE=2";
        //                                        using (var dsAttributeValueTranslation = _xSupport.GetSQLDataSet(queryAttributeValueTranslation, null))
        //                                        {
        //                                            if (dsAttributeValueTranslation != null)
        //                                            {
        //                                                try
        //                                                {
        //                                                    if (dsAttributeValueTranslation.Count > 0)
        //                                                    {
        //                                                        for (int t = 0; t < dsAttributeValueTranslation.Count; t++)
        //                                                        {
        //                                                            var resAttributeValueTranslation = new AttributeValueTranslation
        //                                                            {
        //                                                                LanguageCode = dsAttributeValueTranslation.GetAsInteger(t, "CCCLANGUAGE"),
        //                                                                Description = dsAttributeValueTranslation.GetAsString(t, "TRANSLATION")
        //                                                            };
        //                                                            attributevaluetranslationlist.Add(resAttributeValueTranslation);
        //                                                        }
        //                                                    }
        //                                                }
        //                                                catch (Exception ex)
        //                                                {
        //                                                    throw new Exception(ex.Message);
        //                                                }
        //                                            }
        //                                        }
        //                                        var resAttributeValues = new AttributeValue
        //                                        {
        //                                            SoftOneId = dsAttributeValues.GetAsInteger(v, "MTRATTRIBUTELN"),
        //                                            Code = dsAttributeValues.GetAsString(v, "CODE"),
        //                                            Translations = attributevaluetranslationlist
        //                                        };
        //                                        attributevaluelist.Add(resAttributeValues);
        //                                    }
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                throw new Exception(ex.Message);
        //                            }
        //                        }
        //                        var res = new AttributeRecord
        //                        {
        //                            SoftOneId = dsAttributes.GetAsInteger(i, "MTRATTRIBUTE"),
        //                            Code = dsAttributes.GetAsString(i, "CODE"),
        //                            Translations = attributetranslationlist,
        //                            Values = attributevaluelist
        //                        };
        //                        attributelist.Add(res);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception(ex.Message);
        //            }
        //        }
        //        sqldata = attributelist;
        //        return sqldata;
        //    }
        //    catch (Exception ex)
        //    {
        //        return sqldata;
        //        throw new Exception(ex.Message);
        //    }
        //}
        public string CreateUpdateAttributes(List<AttributeRecord> attributes)
        {
            var logs_remarks = "";
            if (attributes.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο
                ProgressNotify(2, attributes.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var attr in attributes)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    try
                    {
                        using (var AttributeObj = _xSupport.CreateModule("MTRATTRIBUTE;Attributes"))
                        {
                            var attributeId = attr.SoftOneId;
                            if (attributeId > 0)
                            {
                                AttributeObj.LocateData(attributeId);

                                var dbName =Convert.ToString(AttributeObj.GetTable("MTRATTRIBUTE").Current["NAME"]);
                                var name = attr.Translations.Any() ? attr.Translations.OrderBy(x => x.LanguageCode).FirstOrDefault().Description
                                                                    : !string.IsNullOrWhiteSpace(dbName) ? dbName : attr.Code;
                                //var name = attr.Translations.Any() ? attr.Translations.OrderBy(x => x.LanguageCode).FirstOrDefault().Description : 
                                //    (AttributeObj.GetTable("MTRATTRIBUTE").Current["NAME"] != "" ? AttributeObj.GetTable("MTRATTRIBUTE").Current["NAME"] : attr.Code);
                                AttributeObj.GetTable("MTRATTRIBUTE").Current["NAME"] = name;
                                if (attr.Translations.Count > 0)
                                {
                                    foreach (var trns in attr.Translations)
                                    {
                                        //Πίνακας ATTIBUTETRANSH
                                        using (var attributetrnsh = AttributeObj.GetTable("ATTIBUTETRANSH"))
                                        {
                                            var recNo1 = -1;
                                            for (var i = 0; i < attributetrnsh.Count; i++)
                                            {
                                                var recattributeId = Convert.ToInt32(attributetrnsh[i, "MTRATTRIBUTE"]);
                                                var reclang = Convert.ToInt32(attributetrnsh[i, "CCCLANGUAGE"]);
                                                if (reclang == trns.LanguageCode && recattributeId == attributeId)
                                                {
                                                    recNo1 = i;
                                                    break;
                                                }
                                            }
                                            attributetrnsh.Current.Edit(recNo1);
                                            //var recNo1 = attributetrnsh.Find("MTRATTRIBUTE;CCCLANGUAGE", attributeId, trns.LanguageCode);
                                            if (recNo1 != -1)
                                            {
                                                //attributetrnsh.Current["MTRATTRIBUTE"] = attributeId;
                                                attributetrnsh.Current["CCCLANGUAGE"] = trns.LanguageCode;
                                                attributetrnsh.Current["TRANSLATION"] = trns.Description;
                                                //attributetrnsh.Current.Post();
                                            }
                                            else
                                            {
                                                attributetrnsh.Current.Append();
                                                attributetrnsh.Current["CCCLANGUAGE"] = trns.LanguageCode;
                                                attributetrnsh.Current["TRANSLATION"] = trns.Description;
                                                //attributetrnsh.Current.Post();
                                            }
                                            attributetrnsh.Current.Post();
                                        }
                                    }
                                }
                                if (attr.Values.Count > 0)
                                {
                                    foreach (var val in attr.Values)
                                    {
                                        var attributelnId = val.SoftOneId;
                                        using (var mtrattributeln = AttributeObj.GetTable("MTRATTRIBUTELN"))
                                        {
                                            if (val.SoftOneId > 0)
                                            {
                                                var recNo1 = -1;
                                                for (var i = 0; i < mtrattributeln.Count; i++)
                                                { 

                                                    var recattributeId = Convert.ToInt32(mtrattributeln[i, "MTRATTRIBUTE"]);
                                                    var recattributelnId = Convert.ToInt32(mtrattributeln[i, "MTRATTRIBUTELN"]);
                                                    var reccode = Convert.ToString(mtrattributeln[i, "CODE"]);
                                                    if (recattributeId == attributeId /*&& recattributelnId == attributelnId*/ && reccode == val.Code)
                                                    {
                                                        recNo1 = i;
                                                        break;
                                                    }
                                                }
                                                mtrattributeln.Current.Edit(recNo1);
                                                //var recNo1 = mtrattributeln.Find("MTRATTRIBUTE;MTRATTRIBUTELN;CODE", attributeId, attributelnId, val.Code);
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

                                            if (val.Translations.Count > 0)
                                            {
                                                foreach (var trnsln in val.Translations.OrderBy(x => x.LanguageCode))
                                                {
                                                    //Πίνακας ATTIBUTETRANSLN
                                                    using (var attributetrnsln = AttributeObj.GetTable("ATTIBUTETRANSLN"))
                                                    {
                                                        //var test = attributetrnsln.JSON();
                                                        var recNo1 = -1;
                                                        for (var i = 0; i < attributetrnsln.Count; i++)
                                                        {
                                                            var recattributeId = Convert.ToInt32(attributetrnsln[i, "MTRATTRIBUTE"]);
                                                            var recattributelnId = Convert.ToInt32(attributetrnsln[i, "MTRATTRIBUTELN"]);
                                                            var reclang = Convert.ToInt32(attributetrnsln[i, "CCCLANGUAGE"]);
                                                            if (reclang == trnsln.LanguageCode && recattributeId == attributeId && recattributelnId == attributelnId)
                                                            {
                                                                recNo1 = i;
                                                                break;
                                                            }
                                                        }
                                                        attributetrnsln.Current.Edit(recNo1);
                                                        //var recNo1 = attributetrnsln.Find("MTRATTRIBUTE;MTRATTRIBUTELN;CCCLANGUAGE", attributeId, attributelnId, trnsln.LanguageCode);
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
                                        if (val.Translations.Where(x => x.Description != "").ToList().Count > 0)
                                        {
                                            //Πίνακας ATTIBUTETRANSH
                                            using (var mtrattributeln = AttributeObj.GetTable("MTRATTRIBUTELN"))
                                            {
                                                mtrattributeln.Current.Append();
                                                mtrattributeln.Current["CODE"] = val.Code;
                                                mtrattributeln.Current["SOVALUE"] = val.Translations.Any() ? val.Translations.OrderBy(x => x.LanguageCode).FirstOrDefault().Description : val.Code;
                                                if (val.Translations.Count > 0)
                                                {
                                                    foreach (var trnsln in val.Translations.OrderBy(x => x.LanguageCode))
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
                            }
                            AttributeObj.PostData();
                        }
                    }
                    catch (Exception ex)
                    {
                        logs_remarks = logs_remarks + $"Πρόβλημα στο Attribute «{attr.Code}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }

        public string CreateUpdateProductAttributesNew(List<ProductAttributeRecord> productAttributes, List<SqlData> sqldata, List<AttributeRecord> attributes)
        {
            var logs_remarks = "";
            // Implementation for creating or updating product attributes
            List<ProductAttributesGrouped> groupedResult = productAttributes.GroupBy(x => x.ProductCode).Select(productGroup => new ProductAttributesGrouped
            {
                ProductCode = productGroup.Key,
                Attributes = productGroup
            .GroupBy(x => x.AttributeCode).Select(attributeGroup => new ProductAttributeGrouped
            {
                AttributeCode = attributeGroup.Key,
                Values = attributeGroup
                    .GroupBy(x => x.AttributeValueCode)
                    .Select(valueGroup => new ProductAttributeValueGrouped
                    {
                        AttributeValueCode = valueGroup.Key,
                        Translations = valueGroup
                            .Where(x => !string.IsNullOrWhiteSpace(x.FreeText))
                            .GroupBy(x => x.LanguageCode)
                            .Select(languageGroup => new ProductAttributeTranslation
                            {
                                LanguageCode = languageGroup.Key,
                                Description = languageGroup.First().FreeText
                            }).ToList()
                    }).ToList()
            }).ToList()
            }).ToList();
            if (groupedResult.Count > 0)
            {
                ProgressNotify(1, 1);
                MarkStage(1);
                ProgressNotify(2, groupedResult.Count);
                var counter = 0;
                foreach (var productItem in groupedResult)
                {
                    counter++;
                    ProgressNotify(3, counter);
                    var productcode = productItem.ProductCode;
                    // Βρίσκουμε το MTRL του είδους
                    var mtrl = sqldata.Where(x => x.Obj == "item" && x.Code.Trim() == productcode.Trim()).FirstOrDefault()?.Id ?? 0;
                    if (mtrl <= 0)
                    {
                        logs_remarks += $"Το Είδος «{productcode}» δεν βρέθηκε στο Softone." + Environment.NewLine;
                        continue;
                    }
                    var attributesitem = productItem.Attributes;
                    if (attributesitem == null || attributesitem.Count == 0)
                        continue;
                    foreach (var attribute in attributesitem)
                    {
                        try 
                        {
                            // ============================================
                            // ATTRIBUTE
                            // ============================================
                            var attribute_list = attributes.FirstOrDefault(x => x.Code.Trim().Equals(attribute.AttributeCode.Trim(), StringComparison.OrdinalIgnoreCase));
                            var attributeId = attribute_list != null ? attribute_list.SoftOneId : 0;
                            if (attributeId <= 0)
                            {
                                logs_remarks +=$"Το Attribute «{attribute.AttributeCode}» δεν βρέθηκε στο Softone για το είδος «{productcode}»."+ Environment.NewLine;
                                continue;
                            }
                            // ============================================
                            // ATTRIBUTE VALUE
                            // ============================================
                            var attributevalueId = 0;
                            var attributevalues = attribute.Values.FirstOrDefault();
                            if (attributevalues != null && attribute_list.Values != null)
                            {
                                var attributevalue_list = attribute_list.Values.FirstOrDefault(x => x.Code.Trim().Equals(attributevalues.AttributeValueCode.Trim(), StringComparison.OrdinalIgnoreCase));
                                if (attributevalue_list != null)
                                {
                                    attributevalueId = attributevalue_list.SoftOneId;
                                }
                            }
                            // ============================================
                            // MTRLATTRIBUTES
                            // INSERT / UPDATE
                            // ============================================
                            var attributeValueSql = attributevalueId > 0 ? attributevalueId.ToString() : "NULL";
                            var updquery = $@"IF EXISTS (SELECT 1 FROM MTRLATTRIBUTES WHERE MTRL = {mtrl} AND MTRATTRIBUTE = {attributeId})
                                            BEGIN
                                                UPDATE MTRLATTRIBUTES SET MTRATTRIBUTELN = {attributeValueSql} WHERE MTRL = {mtrl} AND MTRATTRIBUTE = {attributeId}
                                            END
                                            ELSE
                                            BEGIN
                                                DECLARE @LineNum INT;
                                                SELECT @LineNum = ISNULL(MAX(LINENUM), 0) + 1 FROM MTRLATTRIBUTES WITH (UPDLOCK, HOLDLOCK) WHERE MTRL = {mtrl};
                                                INSERT INTO MTRLATTRIBUTES (COMPANY,LINENUM,MTRL,MTRATTRIBUTE,MTRATTRIBUTELN) 
                                                VALUES({_xSupport.ConnectionInfo.CompanyId},@LineNum,{mtrl},{attributeId},{attributeValueSql})
                                            END";
                            _xSupport.ExecuteSQL(updquery);
                            // ============================================
                            // CCCATTRIBUTEFREETEXT
                            // INSERT / UPDATE
                            // ============================================
                            var freetextTranslations = attributevalues != null ? attributevalues.Translations : null;
                            if (freetextTranslations != null)
                            {
                                foreach (var trns in freetextTranslations.OrderBy(x => x.LanguageCode))
                                {
                                    var description = trns.Description ?? "";
                                    // Ασφάλεια για apostrophe
                                    description = description.Replace("'", "''");
                                    var languageCode = trns.LanguageCode;
                                    var freeTextQuery = $@"IF EXISTS (SELECT 1 FROM CCCATTRIBUTEFREETEXT WHERE MTRL = {mtrl} AND MTRATTRIBUTE = {attributeId} AND CCCLANGUAGE = {languageCode})
                                                            BEGIN
                                                                UPDATE CCCATTRIBUTEFREETEXT SET FREETXT = '{description.Replace("'", "΄")}' WHERE MTRL = {mtrl} AND MTRATTRIBUTE = {attributeId} AND CCCLANGUAGE = {languageCode}
                                                            END
                                                            ELSE
                                                            BEGIN
                                                                DECLARE @LineNum INT;
                                                                SELECT @LineNum = ISNULL(MAX(LINENUM), 0) + 1 FROM CCCATTRIBUTEFREETEXT WITH (UPDLOCK, HOLDLOCK) WHERE MTRL = {mtrl};
                                                                INSERT INTO CCCATTRIBUTEFREETEXT (COMPANY,LINENUM,MTRL,MTRATTRIBUTE,CCCLANGUAGE,FREETXT) 
                                                                VALUES({_xSupport.ConnectionInfo.CompanyId},@LineNum,{mtrl},{attributeId},{languageCode},'{description.Replace("'", "΄")}')
                                                            END";
                                    _xSupport.ExecuteSQL(freeTextQuery);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logs_remarks += $"Πρόβλημα στο Είδος «{productcode}» και στο Attribute «{attribute.AttributeCode}». " + ex.Message + Environment.NewLine;
                        }
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        //public string CreateUpdateProductAttributes(List<ProductAttributeRecord> productAttributes, List<SqlData> sqldata, List<AttributeRecord> attributes)
        //{
        //    var logs_remarks = "";
        //    // Implementation for creating or updating product attributes
        //    List<ProductAttributesGrouped> groupedResult = productAttributes.GroupBy(x => x.ProductCode).Select(productGroup => new ProductAttributesGrouped
        //    {
        //        ProductCode = productGroup.Key,
        //        Attributes = productGroup
        //    .GroupBy(x => x.AttributeCode).Select(attributeGroup => new ProductAttributeGrouped
        //    {
        //        AttributeCode = attributeGroup.Key,
        //        Values = attributeGroup
        //            .GroupBy(x => x.AttributeValueCode)
        //            .Select(valueGroup => new ProductAttributeValueGrouped
        //            {
        //                AttributeValueCode = valueGroup.Key,
        //                Translations = valueGroup
        //                    .Where(x => !string.IsNullOrWhiteSpace(x.FreeText))
        //                    .GroupBy(x => x.LanguageCode)
        //                    .Select(languageGroup => new ProductAttributeTranslation
        //                    {
        //                        LanguageCode = languageGroup.Key,
        //                        Description = languageGroup.First().FreeText
        //                    }).ToList()
        //            }).ToList()
        //    }).ToList()
        //    }).ToList();

        //    if (groupedResult.Count > 0)
        //    {
        //        ProgressNotify(1, 1); //Ξεκινά την μπάρα
        //        MarkStage(1); //Γράφει στάδιο					
        //        ProgressNotify(2, groupedResult.Count); //Χωρίζει την μπάρα σε κομμάτια
        //        var counter = 0;
        //        foreach (var arrtibuteItem in groupedResult)
        //        {
        //            counter++;
        //            ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
        //            var productcode = arrtibuteItem.ProductCode;
        //            var mtrl = sqldata.Where(x => x.Obj == "item" && x.Code.Trim() == productcode.Trim()).FirstOrDefault()?.Id ?? 0;
        //            var attributesitem = arrtibuteItem.Attributes;
        //            if (mtrl > 0 && attributesitem.Count > 0)
        //            {
        //                using (var ItemObj = _xSupport.CreateModule("ITEM;Items Mothercare"))
        //                {
        //                    try
        //                    {
        //                        ItemObj.LocateData(mtrl);
        //                        using (var mtrattributes = ItemObj.GetTable("MTRLATTRIBUTES"))
        //                        {
        //                            foreach (var attribute in attributesitem)
        //                            {
        //                                try
        //                                {
        //                                    var attribute_list = attributes.Where(x => x.Code.Trim() == attribute.AttributeCode.Trim()).FirstOrDefault();
        //                                    var attributeId = attribute_list != null ? attribute_list.SoftOneId : 0;
        //                                    if (attributeId > 0)
        //                                    {
        //                                        var attributevalues = attribute.Values.FirstOrDefault();
        //                                        var attributevalue_list = attribute_list.Values.Where(x => x.Code.Trim() == attributevalues.AttributeValueCode.Trim()).FirstOrDefault();
        //                                        var attributevalueId = attributevalue_list != null ? attributevalue_list.SoftOneId : 0;
        //                                        var recNo1 = -1;
        //                                        for (var i = 0; i < mtrattributes.Count; i++)
        //                                        {
        //                                            var recattributeId = Convert.ToInt32(mtrattributes[i, "MTRATTRIBUTE"]);
        //                                            if (recattributeId == attributeId)
        //                                            {
        //                                                recNo1 = i;
        //                                                break;
        //                                            }
        //                                        }
        //                                        mtrattributes.Current.Edit(recNo1);
        //                                        //var recNo1 = mtrattributes.Find("MTRATTRIBUTE", attributeId);
        //                                        if (recNo1 != -1)
        //                                        {
        //                                            if (Convert.ToInt32(mtrattributes.Current["MTRATTRIBUTELN"] != DBNull.Value ? mtrattributes.Current["MTRATTRIBUTELN"] : 0) != attributevalueId)
        //                                            {
        //                                                if (attributevalueId > 0)
        //                                                {
        //                                                    mtrattributes.Current["MTRATTRIBUTELN"] = attributevalueId;
        //                                                }
        //                                                else
        //                                                {
        //                                                    mtrattributes.Current["MTRATTRIBUTELN"] = null;
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            mtrattributes.Current.Append();
        //                                            mtrattributes.Current["MTRATTRIBUTE"] = attributeId;
        //                                            if (attributevalueId > 0)
        //                                            {
        //                                                mtrattributes.Current["MTRATTRIBUTELN"] = attributevalueId;
        //                                            }
        //                                        }
        //                                        var freetexttranslations = attributevalues.Translations;
        //                                        //attributevalue_list.Translations;
        //                                        foreach (var trns in freetexttranslations.OrderBy(x => x.LanguageCode))
        //                                        {
        //                                            //Πίνακας ATTIBUTETRANSLN
        //                                            using (var attributefreetext = ItemObj.GetTable("CCCATTRIBUTEFREETEXT"))
        //                                            {
        //                                                var recTrNo1 = -1;
        //                                                for (var i = 0; i < attributefreetext.Count; i++)
        //                                                {
        //                                                    var recattributeId = Convert.ToInt32(attributefreetext[i, "MTRATTRIBUTE"]);
        //                                                    var reclang = Convert.ToInt32(attributefreetext[i, "CCCLANGUAGE"]);
        //                                                    var tets = attributefreetext.CreateDataTable(true).AsEnumerable().ToList();
        //                                                    if (reclang == trns.LanguageCode && recattributeId == attributeId)
        //                                                    {
        //                                                        recTrNo1 = i;
        //                                                        break;
        //                                                    }
        //                                                }
        //                                                attributefreetext.Current.Edit(recTrNo1);
        //                                                var recTrNo22222 = attributefreetext.Find("MTRATTRIBUTE;CCCLANGUAGE", attributeId, trns.LanguageCode);
        //                                                if (recTrNo1 != -1)
        //                                                {
        //                                                    attributefreetext.Current["CCCLANGUAGE"] = trns.LanguageCode;
        //                                                    attributefreetext.Current["FREETXT"] = trns.Description;
        //                                                }
        //                                                else
        //                                                {
        //                                                    attributefreetext.Current.Append();
        //                                                    attributefreetext.Current["CCCLANGUAGE"] = trns.LanguageCode;
        //                                                    attributefreetext.Current["FREETXT"] = trns.Description;
        //                                                }
        //                                                attributefreetext.Current.Post();
        //                                            }
        //                                        }
        //                                        //
        //                                        mtrattributes.Current.Post();
        //                                        //prepei na diagrafo auta pou den vrisko?
        //                                    }
        //                                    else
        //                                    {
        //                                        logs_remarks = logs_remarks + $"Το Attribute «{attribute.AttributeCode}» δεν βρέθηκε στο Softone για το είδος «{productcode}»." + Environment.NewLine;
        //                                    }
        //                                }
        //                                catch (Exception ex)
        //                                {
        //                                    logs_remarks = logs_remarks + $"Πρόβλημα στο Είδος «{productcode}» και στο Attribute «{attribute.AttributeCode}»." + ex.Message + Environment.NewLine;
        //                                }
        //                            }
        //                        }
        //                        ItemObj.PostData();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        logs_remarks = logs_remarks + $"Πρόβλημα στο Είδος «{productcode}»" + ex.Message + Environment.NewLine;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                logs_remarks = logs_remarks + $"Το Είδος «{productcode}» δεν βρέθηκε στο Softone." + Environment.NewLine;
        //            }
        //        }
        //        MarkStage(2);
        //        MarkStage(3);
        //        MarkStage(0);
        //        ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
        //        var resultsprocess = "result1,result2,result3";
        //        _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
        //    }
        //    return logs_remarks;
        //}
        public List<NidRecord> GetNids()
        {
            var sqldata = new List<NidRecord>();
            try
            {
                var query = "SELECT M.MTRL,I.STORE,ISNULL(M.CCCMCOLDCODE,M.CODE) AS CODE,I.NID,I.INSDATE FROM MTRL M INNER JOIN CCCNID I ON I.MTRL=M.MTRL";
                using (var ds = _xSupport.GetSQLDataSet(query, null))
                {
                    try
                    {
                        if (ds.Count > 0)
                        {
                            for (int i = 0; i < ds.Count; i++)
                            {
                                var itemCode = ds.GetAsString(i, "CODE");
                                var storeId = ds.GetAsInteger(i, "STORE");
                                var nid = ds.GetAsInteger(i, "NID");
                                var insdate = ds.GetAsDateTime(i, "INSDATE");
                                NidRecord nidRecord = sqldata.FirstOrDefault(x => x.ItemCode == itemCode);
                                if (nidRecord == null)
                                {
                                    nidRecord = new NidRecord
                                    {
                                        ItemCode = itemCode,
                                        NidStores = new List<NidStores>()
                                    };
                                    sqldata.Add(nidRecord);
                                }
                                // Προσθέτουμε το NidStores στον NidRecord
                                if (!nidRecord.NidStores.Any(x => x.StoreId == storeId))
                                {
                                    nidRecord.NidStores.Add(new NidStores
                                    {
                                        StoreId = storeId,
                                        Nid = nid,
                                        InsDate = insdate
                                    });
                                }
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
            catch (Exception ex)
            {
                return sqldata;
                throw new Exception(ex.Message);
            }
        }
        public List<AddOnRecord> GetSqlAddOns()
        { 
            var addons = new List<AddOnRecord>();
            var query = $@"SELECT ISNULL(M1.CCCMCOLDCODE,M1.CODE) AS ITEMCODE,ISNULL(M2.CCCMCOLDCODE,M2.CODE) AS ADDONITEMCODE FROM CCCADDONS A
                    LEFT JOIN MTRL M1 ON M1.MTRL =A.MTRL LEFT JOIN MTRL M2 ON M2.MTRL = A.ADDONMTRL
                    WHERE A.COMPANY={_xSupport.ConnectionInfo.CompanyId} AND M2.CODE IS NOT NULL AND M1.CODE IS NOT NULL";
            using (var ds = _xSupport.GetSQLDataSet(query, null))
            {
                try
                {
                    if (ds.Count > 0)
                    {
                        for (int i = 0; i < ds.Count; i++)
                        {
                            var res = new AddOnRecord
                            {
                                ItemCode = ds.GetAsString(i, "ITEMCODE"),
                                AddOnItemCode = ds.GetAsString(i, "ADDONITEMCODE")
                            };
                            addons.Add(res);
                        }
                    }
                    return addons;
                }
                catch (Exception ex)
                {
                    return addons;
                    throw new Exception(ex.Message);
                }
            }
        }
        public string SetAddOns(List<AddOnRecord> exceldata, List<SqlData> item_list)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                var grouped = exceldata.GroupBy(r => r.ItemCode).Select(g => new AddOnGroup
                {
                    ItemCode = g.Key,
                    AddOnItemCodes = g.Select(r => r.AddOnItemCode).ToList()
                }).ToList();

                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, grouped.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var addon in grouped)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    try
                    {
                        var mtrl_list = item_list.Where(x => x.Code.Trim() == addon.ItemCode.Trim()).FirstOrDefault();
                        var mtrl = mtrl_list != null ? mtrl_list.Id : 0;
                        var mname = mtrl_list != null ? mtrl_list.Name : "";
                        var addonitemcodes = addon.AddOnItemCodes.Where(code => item_list.Any(item => item.Code.Trim() == code.Trim())).ToList();

                        if (mtrl > 0 && addonitemcodes.Count > 0)
                        {
                            var queryMaxId = $@"SELECT ISNULL(MAX(LINENUM),0) AS MAXID FROM CCCADDONS WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId} AND MTRL={mtrl}";
                            var dsMaxId = _xSupport.SQL(queryMaxId, null);
                            var maxid = dsMaxId != null ? Convert.ToInt32(dsMaxId) : 0;
                            foreach (var code in addonitemcodes)
                            {
                                //05/10/2026
                                var addon_list = item_list.Where(x => x.Code.Trim() == code.Trim()).FirstOrDefault();
                                var addonItem = addon_list != null ? addon_list.Id : 0;
                                if (addonItem == 0)
                                {
                                    logs_remarks = logs_remarks + $"Δεν υπάρχει το Add On είδος «{code}» για το είδος «{addon.ItemCode}»." + Environment.NewLine;
                                    continue;
                                }
                                else
                                {
                                    maxid += 1;
                                    var updquery = $@"INSERT INTO CCCADDONS (COMPANY,MTRL,LINENUM,ADDONMTRL)
                                               VALUES ({_xSupport.ConnectionInfo.CompanyId},{mtrl},{maxid},{addonItem});";
                                    _xSupport.ExecuteSQL(updquery);
                                }
                            }
                            //using (var ItemObj = _xSupport.CreateModule("ITEM;Items Mothercare"))
                            //{
                            //    ItemObj.LocateData(mtrl);
                            //    using (var mtraddon = ItemObj.GetTable("CCCADDONS"))
                            //    {
                            //        foreach (var code in addonitemcodes)
                            //        {
                            //            var addon_list = item_list.Where(x => x.Code.Trim() == code.Trim()).FirstOrDefault();
                            //            var addonItem = addon_list != null ? addon_list.Id : 0;
                            //            var recNo1 = mtraddon.Find("ADDONMTRL", addonItem);
                            //            if (recNo1 == -1)
                            //            {
                            //                mtraddon.Current.Append();
                            //                mtraddon.Current["ADDONMTRL"] = addonItem;
                            //                mtraddon.Current.Post();
                            //            }
                            //        }
                            //    }
                            //    ItemObj.PostData();
                            //}
                        }
                        else
                        {
                            if (mtrl == 0)
                            {
                                logs_remarks = logs_remarks + $"Το Είδος «{addon.ItemCode}» δεν βρέθηκε στο Softone." + Environment.NewLine;
                            }
                            var notexistaddoncodeslist = addon.AddOnItemCodes.Where(code => !item_list.Any(item => item.Code.Trim() == code.Trim())).ToList();
                            if (notexistaddoncodeslist.Count > 0)
                            {
                                var notexistsimilarcodes = String.Join(",", notexistaddoncodeslist);
                                logs_remarks = logs_remarks + $"Δεν υπάρχουν οι κωδικοί add on ({notexistsimilarcodes}) για το είδος «{addon.ItemCode}»." + Environment.NewLine;
                            }
                            //logs_remarks = logs_remarks + $"Το Είδος «{addon.ItemCode}» δεν βρέθηκε." + Environment.NewLine;
                        }
                    }
                    catch (Exception ex)
                    {
                        logs_remarks = logs_remarks + $"Πρόβλημα στο Όμοιο Είδος «{addon.ItemCode}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public List<ItemTextsRecord> GetSqlTexts()
        {
            var texts = new List<ItemTextsRecord>();
            var query = $@"SELECT M.MTRL,ISNULL(M.CCCMCOLDCODE,M.CODE) AS CODE,
                            ISNULL(CCCLANGUAGE,'') AS CCCLANGUAGE,
                            ISNULL(ESHOPTITLE,0) AS ESHOPTITLE,
                            ISNULL(SMALLDESCR,'') AS SMALLDESCR,
                            ISNULL(BIGDESCR,'') AS BIGDESCR,
                            ISNULL(LABALTITLE,'') AS LABALTITLE,
                            ISNULL(LABALDESCR,'') AS LABALDESCR,
                            ISNULL(FEATURESBENEFITSTXT,'') AS FEATURESBENEFITSTXT
                            FROM MTRL M
                            LEFT JOIN CCCDESCRIPTIONS D ON D.MTRL=M.MTRL
                            WHERE M.COMPANY={_xSupport.ConnectionInfo.CompanyId} AND M.SODTYPE = 51 AND ISNULL(M.CCCITEMCOMPANY,0) IN (0,2)
                            AND (ISNULL(CCCLANGUAGE,0)<>0 OR ISNULL(ESHOPTITLE,'')<>'' OR ISNULL(SMALLDESCR,'')<>'' OR ISNULL(BIGDESCR,'')<>'' 
                            OR ISNULL(LABALTITLE,'')<>'' OR ISNULL(LABALDESCR,'')<>'' OR ISNULL(FEATURESBENEFITSTXT,'')<>'')";
            using (var ds = _xSupport.GetSQLDataSet(query, null))
            {
                try
                {
                    if (ds.Count > 0)
                    {
                        var grouped = Enumerable.Range(0, ds.Count).GroupBy(i => Convert.ToString(ds[i, "CODE"]).Trim());
                        foreach (var productGroup in grouped)
                        {
                            var firstIndex = productGroup.First();
                            var item = new ItemTextsRecord
                            {
                                ItemCode = Convert.ToString(ds[firstIndex, "CODE"]).Trim(),
                                Texts = new List<TextsTranslation>()
                            };
                            foreach (var index in productGroup)
                            {
                                var languageCode = Convert.ToInt32(ds[index, "CCCLANGUAGE"]);
                                item.Texts.Add(new TextsTranslation
                                {
                                    LanguageCode = languageCode,
                                    EshopTitle = Convert.ToString(ds[index, "ESHOPTITLE"]),
                                    SmallDescription = Convert.ToString(ds[index, "SMALLDESCR"]),
                                    LongDescription = Convert.ToString(ds[index, "BIGDESCR"]),
                                    LabelTitle = Convert.ToString(ds[index, "LABALTITLE"]),
                                    LabelDescription = Convert.ToString(ds[index, "LABALDESCR"]),
                                    FeaturesAndBenefits = Convert.ToString(ds[index, "FEATURESBENEFITSTXT"])
                                });
                            }
                            texts.Add(item);
                        }
                    }
                    return texts;
                }
                catch (Exception ex)
                {
                    return texts;
                    throw new Exception(ex.Message);
                }
            }
        }
        public string SetItemTextsNew(List<ItemTextsRecord> exceldata, List<SqlData> item_list)
        {
            var logs_remarks = "";
            if (exceldata == null || exceldata.Count == 0)
                return logs_remarks;
            ProgressNotify(1, 1);
            MarkStage(1);
            ProgressNotify(2, exceldata.Count);
            var counter = 0;
            foreach (var text in exceldata)
            {
                counter++;
                ProgressNotify(3, counter);
                try
                {
                    // Βρίσκουμε το MTRL
                    var mtrlItem = item_list.FirstOrDefault(x => x.Code.Trim().Equals(text.ItemCode.Trim(), StringComparison.OrdinalIgnoreCase));
                    var mtrl = mtrlItem != null ? mtrlItem.Id : 0;
                    if (mtrl <= 0)
                    {
                        logs_remarks += $"Το Είδος «{text.ItemCode}» δεν βρέθηκε στο Softone." + Environment.NewLine;
                        continue;
                    }
                    foreach (var trns in text.Texts)
                    {
                        try 
                        {
                            var textFeaturesAndBenefits = trns.FeaturesAndBenefits ?? "";
                            var isHtml = Regex.IsMatch(textFeaturesAndBenefits, @"<\s*(html|body|p|div|span|br|strong|b|i|em|ul|ol|li|table|tr|td|a|img|h[1-6])\b[^>]*>", RegexOptions.IgnoreCase);
                            if (!isHtml && textFeaturesAndBenefits !="")
                            {
                                textFeaturesAndBenefits = System.Net.WebUtility.HtmlEncode(textFeaturesAndBenefits);
                                textFeaturesAndBenefits = textFeaturesAndBenefits.Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");
                                textFeaturesAndBenefits = $"<p>{textFeaturesAndBenefits}</p>";
                            }
                            // Escape apostrophes για SQL
                            var eshopTitle = (trns.EshopTitle ?? "").Replace("'", "''");
                            var smallDescription = (trns.SmallDescription ?? "").Replace("'", "''");
                            var longDescription = (trns.LongDescription ?? "").Replace("'", "''");
                            var labelTitle = (trns.LabelTitle ?? "").Replace("'", "''");
                            var labelDescription = (trns.LabelDescription ?? "").Replace("'", "''");
                            var featuresBenefits = textFeaturesAndBenefits.Replace("'", "''");
                            var languageCode = trns.LanguageCode;
                            // =====================================================
                            // CCCDESCRIPTIONS
                            // INSERT / UPDATE
                            // =====================================================
                            var updquery = $@"SET XACT_ABORT ON;
                                            BEGIN TRANSACTION;
                                            BEGIN TRY
                                                IF EXISTS (SELECT 1 FROM CCCDESCRIPTIONS WHERE MTRL = {mtrl} AND CCCLANGUAGE = {languageCode})
                                                BEGIN
                                                    UPDATE CCCDESCRIPTIONS
                                                    SET ESHOPTITLE = '{eshopTitle}', SMALLDESCR = '{smallDescription}', BIGDESCR = '{longDescription}', LABALTITLE = '{labelTitle}', LABALDESCR = '{labelDescription}', FEATURESBENEFITSTXT = '{featuresBenefits}'
                                                    WHERE MTRL = {mtrl} AND CCCLANGUAGE = {languageCode};
                                                END
                                                ELSE
                                                BEGIN
                                                    INSERT INTO CCCDESCRIPTIONS (COMPANY,MTRL,CCCLANGUAGE,ESHOPTITLE,SMALLDESCR,BIGDESCR,LABALTITLE,LABALDESCR,FEATURESBENEFITSTXT)
                                                    VALUES ({_xSupport.ConnectionInfo.CompanyId},{mtrl},{languageCode},'{eshopTitle}','{smallDescription}','{longDescription}','{labelTitle}','{labelDescription}','{featuresBenefits}');
                                                END;
                                                COMMIT TRANSACTION;
                                            END TRY
                                            BEGIN CATCH
                                                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                                                THROW;
                                            END CATCH;";
                            _xSupport.ExecuteSQL(updquery);
                        }
                        catch (Exception ex)
                        {
                            logs_remarks += $"Πρόβλημα στη γλώσσα «{trns.LanguageCode}», στο κείμενο Είδους «{text.ItemCode}». " + ex.Message + Environment.NewLine;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logs_remarks += $"Πρόβλημα στο κείμενο Είδους «{text.ItemCode}». " + ex.Message + Environment.NewLine;
                }
            }
            MarkStage(2);
            MarkStage(3);
            MarkStage(0);
            ProgressNotify(0, 0);
            var resultsprocess = "result1,result2,result3";
            _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE",_xModule.Handle,1,0,resultsprocess);
            return logs_remarks;
        }
        public string SetItemTexts(List<ItemTextsRecord> exceldata, List<SqlData> item_list)
        {
            var logs_remarks = "";
            if (exceldata.Count() > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var text in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    try
                    {
                        var mtrl_list = item_list.Where(x => x.Code.Trim() == text.ItemCode.Trim()).FirstOrDefault();
                        var mtrl = mtrl_list != null ? mtrl_list.Id : 0;
                        var mname = mtrl_list != null ? mtrl_list.Name : "";
                        if (mtrl > 0)
                        {
                            using (var ItemObj = _xSupport.CreateModule("ITEM;Items Mothercare"))
                            {
                                ItemObj.LocateData(mtrl);
                                using (var descriptions = ItemObj.GetTable("CCCDESCRIPTIONS"))
                                {
                                    foreach (var trns in text.Texts)
                                    {
                                        var recTrNo1 = -1;
                                        for (var i = 0; i < descriptions.Count; i++)
                                        {
                                            var testlang = Convert.ToInt32(descriptions[i, "CCCLANGUAGE"]);
                                            if (testlang == trns.LanguageCode)
                                            {
                                                recTrNo1 = i;
                                                break;
                                            }
                                        }
                                        descriptions.Current.Edit(recTrNo1);
                                        //var recTrNo1 = descriptions.Find("MTRL;CCCLANGUAGE",mtrl, trns.LanguageCode);
                                        if (recTrNo1 != -1)
                                        {
                                            descriptions.Current["CCCLANGUAGE"] = trns.LanguageCode;
                                            descriptions.Current["ESHOPTITLE"] = trns.EshopTitle;
                                            descriptions.Current["SMALLDESCR"] = trns.SmallDescription;
                                            descriptions.Current["BIGDESCR"] = trns.LongDescription;
                                            descriptions.Current["LABALTITLE"] = trns.LabelTitle;
                                            descriptions.Current["LABALDESCR"] = trns.LabelDescription;
                                            var textfeaturesandbenefits = trns.FeaturesAndBenefits;
                                            var ishtml = Regex.IsMatch(textfeaturesandbenefits, @"<\s*(html|body|p|div|span|br|strong|b|i|em|ul|ol|li|table|tr|td|a|img|h[1-6])\b[^>]*>", RegexOptions.IgnoreCase);
                                            if (!ishtml)
                                            {
                                                textfeaturesandbenefits = System.Net.WebUtility.HtmlEncode(textfeaturesandbenefits);
                                                textfeaturesandbenefits = textfeaturesandbenefits.Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");
                                                textfeaturesandbenefits = $"<p>{textfeaturesandbenefits}</p>";
                                            }
                                            var vBlobField = _xModule.Exec("CODE:ModuleIntf.GetField", descriptions.TablePtr, "FEATURESBENEFITSHTML");
                                            var vBlobPointer = _xModule.Exec("CODE:SOHTMLDOC.CreateDoc", 1, textfeaturesandbenefits);
                                            _xModule.Exec("CODE:SOHTMLDOC.SaveDoctoBlob", vBlobPointer, vBlobField);
                                            descriptions.Current["FEATURESBENEFITSTXT"] = textfeaturesandbenefits;
                                        }
                                        else
                                        {
                                            descriptions.Current.Append();
                                            descriptions.Current["CCCLANGUAGE"] = trns.LanguageCode;
                                            descriptions.Current["ESHOPTITLE"] = trns.EshopTitle;
                                            descriptions.Current["SMALLDESCR"] = trns.SmallDescription;
                                            descriptions.Current["BIGDESCR"] = trns.LongDescription;
                                            descriptions.Current["LABALTITLE"] = trns.LabelTitle;
                                            descriptions.Current["LABALDESCR"] = trns.LabelDescription;
                                            var textfeaturesandbenefits = trns.FeaturesAndBenefits;
                                            var ishtml = Regex.IsMatch(textfeaturesandbenefits, @"<\s*(html|body|p|div|span|br|strong|b|i|em|ul|ol|li|table|tr|td|a|img|h[1-6])\b[^>]*>", RegexOptions.IgnoreCase);
                                            if (!ishtml)
                                            {
                                                textfeaturesandbenefits = System.Net.WebUtility.HtmlEncode(textfeaturesandbenefits);
                                                textfeaturesandbenefits = textfeaturesandbenefits.Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");
                                                textfeaturesandbenefits = $"<p>{textfeaturesandbenefits}</p>";
                                            }
                                            var vBlobField = _xModule.Exec("CODE:ModuleIntf.GetField", descriptions.TablePtr, "FEATURESBENEFITSHTML");
                                            var vBlobPointer = _xModule.Exec("CODE:SOHTMLDOC.CreateDoc", 1, textfeaturesandbenefits);
                                            _xModule.Exec("CODE:SOHTMLDOC.SaveDoctoBlob", vBlobPointer, vBlobField);
                                            descriptions.Current["FEATURESBENEFITSTXT"] = textfeaturesandbenefits;
                                        }
                                        descriptions.Current.Post();
                                    }
                                }
                                ItemObj.PostData();
                            }
                        }
                        else
                        {
                            logs_remarks = logs_remarks + $"Το Είδος «{text.ItemCode}» δεν βρέθηκε στο Softone." + Environment.NewLine;
                        }
                    }
                    catch (Exception ex)
                    {
                        logs_remarks = logs_remarks + $"Πρόβλημα στο κείμενο Είδους «{text.ItemCode}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα						 
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public List<TagRecord> GetSqlTags()
        {
            var tags = new List<TagRecord>();
            var query = $@"SELECT CCCTAGS,CODE,NAME,FROMDATE,TODATE,ISACTIVE FROM CCCTAGS WHERE COMPANY={_xSupport.ConnectionInfo.CompanyId}";
            using (var ds = _xSupport.GetSQLDataSet(query, null))
            {
                try
                {
                    if (ds.Count > 0)
                    {
                        for (int i = 0; i < ds.Count; i++)
                        {
                            var tadId = ds.GetAsInteger(i, "CCCTAGS");
                            var items = new List<TagItems>();
                            var queryln = $@"SELECT L.MTRL,M.CODE FROM CCCTAGSNL L INNER JOIN MTRL M ON M.MTRL=L.MTRL WHERE L.COMPANY={_xSupport.ConnectionInfo.CompanyId} AND L.CCCTAGS={tadId}";
                            using (var dsln = _xSupport.GetSQLDataSet(queryln, null))
                            {
                                if (dsln.Count > 0)
                                {
                                    for (int j = 0; j < dsln.Count; j++)
                                    {
                                        var resitem = new TagItems
                                        {
                                            ItemCode = dsln.GetAsString(j, "CODE")
                                        };
                                        items.Add(resitem);
                                    }
                                }
                            }
                            var res = new TagRecord
                            {
                                SoftoneId = ds.GetAsInteger(i, "CCCTAGS"),
                                Code = ds.GetAsString(i, "CODE"),
                                Description = ds.GetAsString(i, "NAME"),
                                DateFrom = ds.GetAsDateTime(i, "FROMDATE"),
                                DateTo = ds.GetAsDateTime(i, "TODATE"),
                                Active = ds.GetAsInteger(i, "ISACTIVE") == 1 ? true : false,
                                Items = items
                            };
                            tags.Add(res);
                        }
                    }
                    return tags;
                }
                catch (Exception ex)
                {
                    return tags;
                    throw new Exception(ex.Message);
                }
            }
        }
        public string CreateTag(List<TagRecord> exceldata, List<SqlData> item_list, List<TagRecord> softonetags_list)
        {
            var logs_remarks = "";
            if (exceldata.Count > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο					
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var tag in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    var code = tag.Code;
                    var name = tag.Description;
                    var fromdate = tag.DateFrom;
                    var todate = tag.DateTo;
                    var isactive = tag.Active;
                    var itemlist = tag.Items;
                    var checkitems = itemlist.Where(x => item_list.Any(z => z.Code.Trim() == x.ItemCode.Trim())).ToList();
                    var tagID = softonetags_list.Where(x => x.Code.Trim() == code.Trim()).FirstOrDefault()?.SoftoneId ?? 0;
                    if (checkitems.Count > 0)
                    {
                        var itemnotfound = itemlist.Where(x => !item_list.Any(z => z.Code.Trim() == x.ItemCode.Trim())).ToList();
                        if (itemnotfound.Count > 0)
                        {
                            var notfoundcodes = string.Join(", ", itemnotfound.Select(x => x.ItemCode));
                            logs_remarks = logs_remarks + $"Το Tag «{code}» έχει μη έγκυρα είδη: {notfoundcodes}." + Environment.NewLine;
                        }
                        try
                        {
                            using (var ImpObj = _xSupport.CreateModule("CCCTAGS"))
                            {
                                if (tagID > 0)
                                {
                                    ImpObj.LocateData(tagID);
                                }
                                else
                                {
                                    ImpObj.InsertData();
                                    ImpObj.GetTable("CCCTAGS").Current["CODE"] = code;
                                }
                                ImpObj.GetTable("CCCTAGS").Current["ISACTIVE"] = isactive ? 1 : 0;
                                ImpObj.GetTable("CCCTAGS").Current["NAME"] = name;
                                ImpObj.GetTable("CCCTAGS").Current["FROMDATE"] = fromdate;
                                ImpObj.GetTable("CCCTAGS").Current["TODATE"] = todate;
                                if (checkitems.Count > 0)
                                {
                                    using (var tagsnl = ImpObj.GetTable("CCCTAGSNL"))
                                    {
                                        foreach (var item in checkitems)
                                        {
                                            var mtrl = item_list.Where(x => x.Code.Trim() == item.ItemCode.Trim()).FirstOrDefault()?.Id ?? 0;
                                            if (mtrl > 0)
                                            {
                                                var recNo1 = tagsnl.Find("MTRL", mtrl);
                                                if (recNo1 == -1)
                                                {
                                                    tagsnl.Current.Append();
                                                    tagsnl.Current["MTRL"] = mtrl;
                                                    tagsnl.Current.Post();
                                                }
                                            }
                                        }
                                    }
                                }
                                ImpObj.PostData();
                            }
                        }
                        catch (Exception ex)
                        {
                            logs_remarks = logs_remarks + $"Πρόβλημα στο Tag «{code}»." + ex.Message + Environment.NewLine;
                        }
                    }
                    else
                    {
                        logs_remarks = logs_remarks + $"Το Tag «{code}» δεν έχει έγκυρα είδη." + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
        public string SetNidNew(List<NidRecord> exceldata, List<SqlData> item_list)
        {
            var logs_remarks = "";
            if (exceldata == null || exceldata.Count == 0)
                return logs_remarks;
            ProgressNotify(1, 1);
            MarkStage(1);
            ProgressNotify(2, exceldata.Count);
            var counter = 0;
            foreach (var nid in exceldata)
            {
                counter++;
                ProgressNotify(3, counter);
                try
                {
                    var itemcode = nid.ItemCode;
                    var mtrl = item_list.FirstOrDefault(x => x.Code.Trim().Equals(itemcode.Trim(), StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
                    if (mtrl <= 0)
                    {
                        logs_remarks += $"Το Είδος «{nid.ItemCode}» δεν βρέθηκε στο Softone." + Environment.NewLine;
                        continue;
                    }
                    foreach (var store in nid.NidStores)
                    {
                        var storeId = store.StoreId;
                        var nidValue = store.Nid;
                        var webpage = "";
                        if (storeId == 11)
                        {
                            webpage = "https://www.mothercare.gr/product/" + nidValue.ToString();
                        }
                        webpage = webpage.Replace("'", "''");
                        var updquery = $@"
                                        SET XACT_ABORT ON;
                                        BEGIN TRANSACTION;
                                        BEGIN TRY
                                            IF EXISTS (SELECT 1 FROM CCCNID WHERE MTRL = {mtrl} AND STORE = {storeId})
                                            BEGIN
                                                UPDATE CCCNID SET NID = {nidValue} {(storeId == 11? $@", WEBPAGE = '{webpage}'": "")} WHERE MTRL = {mtrl} AND STORE = {storeId};
                                            END
                                            ELSE
                                            BEGIN
                                                DECLARE @LineNum INT;
                                                SELECT @LineNum = ISNULL(MAX(LINENUM), 0) + 1 FROM CCCNID WITH (UPDLOCK, HOLDLOCK) WHERE MTRL = {mtrl};
                                                INSERT INTO CCCNID (COMPANY,MTRL,LINENUM,STORE,NID{(storeId == 11? ", WEBPAGE": "")})
                                                VALUES ({_xSupport.ConnectionInfo.CompanyId},{mtrl},@LineNum,{storeId},{nidValue}{(storeId == 11? $", '{webpage}'": "")});
                                            END;
                                            COMMIT TRANSACTION;
                                        END TRY
                                        BEGIN CATCH
                                            IF @@TRANCOUNT > 0 
                                            ROLLBACK TRANSACTION;
                                            THROW;
                                        END CATCH;";
                        _xSupport.ExecuteSQL(updquery);
                    }
                }
                catch (Exception ex)
                {
                    logs_remarks += $"Πρόβλημα στο NID του Είδους «{nid.ItemCode}». " + ex.Message + Environment.NewLine;
                }
            }
            MarkStage(2);
            MarkStage(3);
            MarkStage(0);
            ProgressNotify(0, 0);
            var resultsprocess = "result1,result2,result3";
            _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE",_xModule.Handle,1,0,resultsprocess);
            return logs_remarks;
        }
        public string SetNid(List<NidRecord> exceldata, List<SqlData> item_list)
        {
            var logs_remarks = "";
            if (exceldata.Count > 0)
            {
                ProgressNotify(1, 1); //Ξεκινά την μπάρα
                MarkStage(1); //Γράφει στάδιο
                ProgressNotify(2, exceldata.Count); //Χωρίζει την μπάρα σε κομμάτια
                var counter = 0;
                foreach (var nid in exceldata)
                {
                    counter++;
                    ProgressNotify(3, counter);//Γράφει την πρόοδο στην μπάρα
                    try
                    {
                        var itemcode = nid.ItemCode;
                        var mtrl = item_list.Where(x => x.Code.Trim() == itemcode.Trim()).FirstOrDefault()?.Id ?? 0;
                        if (mtrl > 0)
                        {
                            using (var ItemObj = _xSupport.CreateModule("ITEM;Items Mothercare"))
                            {
                                ItemObj.LocateData(mtrl);
                                using (var mtrnid = ItemObj.GetTable("CCCNID"))
                                {
                                    foreach (var store in nid.NidStores)
                                    {
                                        var storeId = store.StoreId;
                                        var recNo1 = mtrnid.Find("STORE", storeId);
                                        if (recNo1 == -1)
                                        {
                                            mtrnid.Current.Append();
                                            mtrnid.Current["STORE"] = storeId;
                                            mtrnid.Current["NID"] = store.Nid;
                                            //mtrnid.Current["INSDATE"] = store.InsDate;
                                            if (storeId == 11)
                                            {
                                                mtrnid.Current["WEBPAGE"] = "https://www.mothercare.gr/product/" + store.Nid.ToString();
                                            }
                                            mtrnid.Current.Post();
                                        }
                                        else
                                        {
                                            mtrnid.Current["NID"] = store.Nid;
                                            mtrnid.Current["INSDATE"] = store.InsDate;
                                            if (storeId == 11)
                                            {
                                                mtrnid.Current["WEBPAGE"] = "https://www.mothercare.gr/product/" + store.Nid.ToString();
                                            }
                                            mtrnid.Current.Post();
                                        }
                                    }
                                }
                                ItemObj.PostData();
                            }
                        }
                        else
                        {
                            logs_remarks = logs_remarks + $"Το Είδος «{nid.ItemCode}» δεν βρέθηκε στο Softone." + Environment.NewLine;
                        }
                    }
                    catch (Exception ex)
                    {
                        logs_remarks = logs_remarks + $"Πρόβλημα στο NID του Είδους «{nid.ItemCode}»." + ex.Message + Environment.NewLine;
                    }
                }
                MarkStage(2);
                MarkStage(3);
                MarkStage(0);
                ProgressNotify(0, 0);   //Σβήνει την μπάρα
                var resultsprocess = "result1,result2,result3";
                _xModule.Exec("CODE:ModuleIntf.SENDRESPONSE", _xModule.Handle, 1, 0, resultsprocess); //Εμφανίζει αποτέλεσμα
            }
            return logs_remarks;
        }
    }
}
