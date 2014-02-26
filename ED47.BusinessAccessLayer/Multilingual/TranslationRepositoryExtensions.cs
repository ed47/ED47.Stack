using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ED47.BusinessAccessLayer.BusinessEntities;
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
                for (var i = 3; i <= sheet.Dimension.End.Row; i++)
                {
                    var key = sheet.Cells[i, 1].GetValue<string>();
                    for (var j = 2; j <= sheet.Dimension.End.Column; j++)
                    {
                        var title = sheet.Cells[2, j].GetValue<string>();
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

        public static ExcelFile ExportXls(this ITranslationRepository repository, string pattern = null, IEnumerable<string> translatableLanguages = null)
        {
            var availableLanguages = Lang.GetLanguages().ToDictionary(el => el.IsoCode, el => el);
            var lans = (translatableLanguages ?? repository.GetAvailableLanguages()).ToList();
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
                    var dictionnary = repository.GetDictionary(lan);

                    if (dictionnary == null)
                        continue;

                    var entry = dictionnary.GetEntry(key);
                    el.AddProperty(lan + "_orig", entry != null ? entry.Value : "");
                    el.AddProperty(lan + "_new", "");
                }
                data.Add(el);
            }
            
            var columns = new List<ExcelColumn>
            {
                new ExcelColumn
                {
                    DisplayName = "Label",
                    PropertyName = "property"
                }
            };

            sheet.AddHeader(new ExcelColumn());

            foreach (var lan in lans)
            {
                columns.Add(new ExcelColumn
                {
                    DisplayName = lan + " orig",
                    PropertyName = lan + "_orig"
                });
                 columns.Add(new ExcelColumn
                 {
                    DisplayName = lan + " new",
                    PropertyName = lan + "_new"
                });

                sheet.AddHeader(new ExcelColumn { HeaderColSpan = 1, DisplayName = availableLanguages[lan.ToLowerInvariant()].Name});
            }
            
            sheet.AddColumns(columns);
            sheet.Data = data;

            var excelFile = new ExcelFile
            {
                FileName = "LabelExport" + pattern + ".xlsx",
                BusinessKey = "LabelExport" + pattern
            };
            excelFile.AddSheet(sheet);
            
            return excelFile;
        }
    }
}
