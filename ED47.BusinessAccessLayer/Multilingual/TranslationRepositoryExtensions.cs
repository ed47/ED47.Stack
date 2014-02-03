using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ED47.BusinessAccessLayer.Excel;
using ED47.Stack.Web;
using ED47.Stack.Web.Multilingual;
using OfficeOpenXml;
using ExcelColumn = ED47.BusinessAccessLayer.Excel.ExcelColumn;

namespace ED47.BusinessAccessLayer.Multilingual
{
    public static class TranslationRepositoryExtensions
    {
        public static void ImportXls(this ITranslationRepository repository, Stream stream)
        {
            using (var file = new ExcelPackage())
            {
                file.Load(stream);
                
                var sheet = file.Workbook.Worksheets.First();
                for (var i = 2; i <= sheet.Dimension.End.Row; i++)
                {
                    var key = sheet.Cells[i, 1].GetValue<string>();
                    for (var j = 2; j <= sheet.Dimension.End.Column; j++)
                    {
                        var title = sheet.Cells[1, j].GetValue<string>();
                        if (String.IsNullOrEmpty(title)) continue;

                        if (title.EndsWith(" new"))
                        {
                            var lan = title.Substring(0, title.Length - 4);
                            var value = sheet.Cells[i, j].GetValue<string>();
                            if (String.IsNullOrWhiteSpace(value)) continue;
                            Stack.Web.Multilingual.Multilingual.UpdateEntry(lan, key, value);
                        }
                    }
                }
            }

            var archiveFile = FileRepositoryFactory.Default.CreateNewFile("LabelImport.xlsx", "LabelImport");
            archiveFile.Write(stream);
        }

        public static ExcelFile ExportXls(this ITranslationRepository repository, string pattern = null)
        {
            var lans = repository.GetAvailableLanguages().ToList();
            lans.Remove(repository.DefaultDictionnary.Language);
            lans.Insert(0,repository.DefaultDictionnary.Language);
            
            var keys = repository.GetAllKeys().Where(el=> String.IsNullOrEmpty(pattern) || el.StartsWith(pattern));
            var sheet = new ExcelSheet("Labels");

            var data = new JsonObjectList();

            foreach (var key in keys)
            {
                var el = new JsonObject();
                el.AddProperty("property",key);
                foreach (var lan in lans)
                {
                    var entry = repository.GetDictionary(lan).GetEntry(key);
                    el.AddProperty(lan + "_orig", entry != null ? entry.Value : "");
                    el.AddProperty(lan + "_new", "");
                }
                data.Add(el);
            }
            
            var headers = new List<ExcelColumn>
            {
                new ExcelColumn
                {
                    DisplayName = "Label",
                    PropertyName = "property"
                }
            };

            foreach (var lan in lans)
            {
                headers.Add(new ExcelColumn
                {
                    DisplayName = lan + " orig",
                    PropertyName = lan + "_orig"
                });
                 headers.Add(new ExcelColumn
                 {
                    DisplayName = lan + " new",
                    PropertyName = lan + "_new"
                });
            }
            
            sheet.HeaderColumns.AddRange(headers);
            sheet.Data = data;

            var excelFile = new ExcelFile
            {
                FileName = "Export" + pattern + ".xlsx",
                BusinessKey = "Export" + pattern
            };
            excelFile.AddSheet(sheet);
            
            return excelFile;
        }
    }
}
