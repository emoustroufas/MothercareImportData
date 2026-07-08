using Softone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Services
{
    public class SoftoneTools
    {
        public void HideWarningsFromS1Module(XModule XModule, XSupport XSupport)
        {
            object otherModule = XSupport.GetStockObj("ModuleIntf", true);
            object[] myArray1;
            myArray1 = new object[3];
            myArray1[0] = XModule.Handle;
            myArray1[1] = "WARNINGS";    //Param Name
            myArray1[2] = "OFF";         //Param Value
            XSupport.CallPublished(otherModule, "SetParamValue", myArray1);
        }
        public void SetPropertyCustom(XModule XModule, string fieldpanel, string fieldpanelname, string property, string truefalse)
        {
            var myobj = new object[3];
            myobj[0] = fieldpanelname;
            myobj[1] = property;
            myobj[2] = truefalse;
            XModule.SetProperty(fieldpanel, myobj);
        }
        public object GetDataSet(XModule XModule, XSupport XSupport, string tablename)
        {
            object otherModule = XSupport.GetStockObj("ModuleIntf", true);
            object[] myArray1;
            myArray1 = new object[2];
            myArray1[0] = XModule.Handle;
            myArray1[1] = tablename;
            var res = XSupport.CallPublished(otherModule, "GetDataset", myArray1);
            return res;
        }
        public void SetDatasetLinks(XModule XModule, XSupport XSupport, object DatasetHandle, int Value)//ModuleHandle, DatasetHandle, Value);
        {
            object otherModule = XSupport.GetStockObj("ModuleIntf", true);
            object[] myArray1;
            myArray1 = new object[3];
            myArray1[0] = XModule.Handle;
            myArray1[1] = DatasetHandle;
            myArray1[2] = Value;
            XSupport.CallPublished(otherModule, "SetDatasetLinks", myArray1);
        }
    }
}
