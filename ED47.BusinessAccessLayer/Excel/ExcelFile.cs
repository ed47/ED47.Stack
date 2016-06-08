using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ED47.BusinessAccessLayer.BusinessEntities;
using ED47.Stack.Web;
using OfficeOpenXml;

namespace ED47.BusinessAccessLayer.Excel
{
    public static class IFileExtension
    {
        public static ExcelPackage ToExcelPackage(this IFile file)
        {
            using (var f = file.OpenRead())
            {
                return new ExcelPackage(f);
            }
        }
    }

    public class ExcelFile
    {
        private int _sheetNameCounter = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelFile"/> class.
        /// </summary>
        public ExcelFile(IFile file = null)
        {
          
            Sheets = new List<ExcelSheet>();
        }

        /// <summary>
        /// Gets or sets the sheets.
        /// </summary>
        /// <value>
        /// The sheets.
        /// </value>
        public List<ExcelSheet> Sheets { get; private set; }
    
        public string FileName { get; set; }
        public string BusinessKey { get; set; }

        /// <summary>
        /// Adds a new sheet.
        /// </summary>
        /// <param name="data">The data to show in the sheet.</param>
        /// <param name="name">The sheet's name.</param>
        /// <returns>The new sheet</returns>
        public ExcelSheet AddSheet(JsonObjectList data, string name = null, bool autoColumns = false, string[] fields = null, string[] labels = null)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                name = "Content " + _sheetNameCounter.ToString(CultureInfo.InvariantCulture);
                _sheetNameCounter++;
            }

            var newSheet = new ExcelSheet(name)
                               {
                                   Data = data,
                                   Fields = fields
                                   
                               };
            
            this.Sheets.Add(newSheet);

            if (autoColumns && newSheet.Data.Count>0)
            {
                var first = newSheet.Data[0];
                newSheet.AddColumns(
                    first.Properties.Select(el=> new ExcelColumn()
                    {
                        DisplayName = el,
                        PropertyName = el,
                    }));
            }
            return newSheet;
        }

        /// <summary>
        /// Adds a new sheet.
        /// </summary>
        /// <param name="sheet">The sheet.</param>
        public void AddSheet(ExcelSheet sheet)
        {
            Sheets.Add(sheet);
        }

        /// <summary>
        /// Saves the Excel file to a file.
        /// </summary>
        /// <param name="fileInfo">The file info to save into.</param>
        public void Write(FileInfo fileInfo)
        {
            using (var excelPackage = new ExcelPackage(fileInfo))
            {
                foreach (var excelSheet in Sheets)
                {
                    excelSheet.Write(excelPackage);
                }

                excelPackage.Save();
            }
        }

        /// <summary>
        /// Saves the Excel file to a stream.
        /// </summary>
        /// <param name="stream">The output stream to write the Excel file to.</param>
        /// <param name="existingFile"></param>
        public void Write(Stream stream, IFile existingFile = null)
        {
            using (var excelPackage = (existingFile != null ? existingFile.ToExcelPackage() : new ExcelPackage()))
            {
                foreach (var excelSheet in Sheets)
                {
                    excelSheet.Write(excelPackage, existingFile != null);
                }
                
                excelPackage.SaveAs(stream);
               // excelPackage.Package.Close();
                stream.Flush();
            }
        }

      

        public IFile Write(string name, string businessKey)
        {
            var file = new FileInfo(Path.GetTempFileName());
            file.Delete();
            Write(file);
            var f = FileRepositoryFactory.Default.CreateNewFile(name, businessKey, 0);
            f.Write(file);
            System.IO.File.Delete(file.FullName);
            return f;
        }

        public FileResult Download(string name, string businessKey)
        {
            var f = Write(name, businessKey);
            using (var s = f.OpenRead())
            {
                return new FileStreamResult(s,f.GetContentType());
            }
        }

        public IFile ToFile(IFile existingFile = null)
        {
            var file = FileRepositoryFactory.Default.CreateNewFile(FileName, BusinessKey);
            
            using (var fileStream = file.OpenWrite())
            {
                Write(fileStream, existingFile);
                fileStream.Flush();
                fileStream.Close();
            }

            return file;
        }
    }
}
