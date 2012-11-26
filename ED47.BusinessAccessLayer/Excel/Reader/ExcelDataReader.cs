using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ED47.BusinessAccessLayer.Excel.Reader;
using ED47.Stack.Web;
using SmartXLS;

namespace DED47.BusinessAccessLayer.Excel.Reader
{
    public class ExcelDataReader
    {

        private JsonObjectList _readErrors = new JsonObjectList();
        public JsonObjectList ReadErrors { get { return _readErrors?? new JsonObjectList(); }}

        public ExcelDataReader()
        {
            
        }

        public void LoadData<TExcelType>(string filename, List<TExcelType> excelUseTypeDataList) where TExcelType : ExcelData
        {
            if (excelUseTypeDataList == null)
                excelUseTypeDataList = new List<TExcelType>();

            if (string.IsNullOrEmpty(filename))
                return;

            var fInfo = new FileInfo(filename);
            if (!fInfo.Exists)
                return;


            ImportXls(filename, excelUseTypeDataList);
        }

        public bool ImportXls<TExcelType>(string filename, List<TExcelType> excelUseTypeDataList) where TExcelType : ExcelData
        {
            var name = Path.GetFileNameWithoutExtension(filename);
            var ext = Path.GetExtension(filename);

            var workBook = new WorkBook();
            try
            {
                if (ext == ".xls")
                {
                    workBook.read(filename);
                }
                else if (ext == ".xlsx")
                {
                    workBook.readXLSX(filename);
                }
                else
                {                    
                    //"Wrong file type", "User tried to upload a " + ext + " file.";
                    return false;
                }
            }
            catch (Exception err)
            {
                //"Reading Excel file", "error reading excel file: " + err.Message;
                return false;
            }

            var myObject = CreateGeneric(typeof(TExcelType)) as ExcelData;
            ExcelGenericAttribute xlAttr = ExcelGenericAttribute.GetAttribute(myObject.GetType());

            if (xlAttr != null && xlAttr.SheetNumberToRead > -1)
            {
                var sheets = xlAttr.SheetNumberToRead > 0 ? xlAttr.SheetNumberToRead : workBook.NumSheets;
                for (var s=0; s < sheets; s++)
                    excelUseTypeDataList.AddRange(ReadExcelSheetData<TExcelType>(workBook, s, ref _readErrors));
                
            } else     // read 1st sheet only
            {
                excelUseTypeDataList.AddRange(ReadExcelSheetData<TExcelType>(workBook, 0, ref _readErrors));
                
            }
            return true;
        }


        private IEnumerable<TExcelType> ReadExcelSheetData<TExcelType>(WorkBook workBook, int sheetNum, ref JsonObjectList errorList) where TExcelType:ExcelData
        {
            
            if (workBook == null) return new List<TExcelType>();
            if (sheetNum < 0) return new List<TExcelType>();

            var tempDataList = new List<TExcelType>();
            workBook.Sheet = sheetNum;

            var lastRow = workBook.LastRow;
            var lastCol = workBook.LastCol;

            
            var myObject = CreateGeneric(typeof(TExcelType)) as ExcelData;

            // get properties of the ExcelMemberData only once
            PropertyInfo[] pps = myObject.GetType().GetProperties();

            ExcelGenericAttribute xlAttr = ExcelGenericAttribute.GetAttribute(myObject.GetType());
               
            for (var i = 1; i <= lastRow; i++)
            {
                var mbrLine = CreateGeneric(typeof(TExcelType)) as ExcelData;
                mbrLine.ExcelLine = i + 1;


                if (xlAttr != null && xlAttr.AllowBlank && IsLineEmpty(workBook, i, pps.Length) )
                {
                    mbrLine.BlankLineFound = true;
                    continue;
                }

                foreach (PropertyInfo p in pps)
                {
                    ExcelColumnAttribute xlColAttr = ExcelColumnAttribute.GetAttribute(p);
                    if (xlColAttr == null || xlColAttr.OutputOnly) continue;

                    var xlValue = workBook.getText(i, xlColAttr.Column);
                    
                    ExcelColumnAttribute.Validate(p, xlValue, mbrLine);

                    mbrLine.CustomPropertyValidation(p, xlValue ,mbrLine);

                    if (mbrLine.CurrentPropertyIsBlank && xlColAttr.IgnoreLineIfBlank)
                    {
                        mbrLine.IsValid = false;
                        mbrLine.WasIgnored = true;
                        continue;
                    }
                        
                    try
                    {
                        if (mbrLine.IsValid && !mbrLine.CurrentPropertyIsBlank)
                        {
                              p.SetValue(mbrLine, mbrLine.ValidValue, null);
                        }
                    }
                    catch (Exception er)
                    {
                        if (mbrLine.IsValid) // to keep the first error
                        {
                            mbrLine.IsValid = false;
                            mbrLine.ErrorMessage = er.Message;
                        }
                    }
                    
                    if (!mbrLine.IsValid)
                        break;
                }
                  
                if (!mbrLine.IsValid && !mbrLine.WasIgnored)
                {
                    var jso = new JsonObject();
                    jso.AddProperty("line", i + 1);
                    jso.AddProperty("sheet", sheetNum);
                    jso.AddProperty("error", mbrLine.ErrorMessage);
                    errorList.Add(jso);
                }

                tempDataList.Add((TExcelType) mbrLine);
            }

            return tempDataList;
        }

        private bool IsLineEmpty(WorkBook workBook, int i, int colsCount) {
            if (workBook == null || i<= 0) 
                return true;
            var isEmpty = workBook.getLastColForRow(i)==0; // this do not check empty cells because cause "" is a value for this method

            if (!isEmpty)
            {
                for(var c=0 ;c<colsCount ; c++)
                {
                    if(!string.IsNullOrEmpty(workBook.getText(i,c)))
                        return false;
                }
            }
            return true;
        }
        
        public static object CreateGeneric(Type generic, params object[] args)
        {
            return Activator.CreateInstance(generic, args);
        }
    }
}