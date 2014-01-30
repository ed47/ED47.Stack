using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace ED47.BusinessAccessLayer.Multilingual
{
    public static class MultilingualRepositoryExtension
    {
        private const int KeyColumn = 1;
        private const int PropertyNameColumn = 2;
        private const int MasterColumn = 3;

        public static void ImportExcel(this IMultilingualRepository repository, Stream stream)
        {
            var multilinguals = new List<BusinessEntities.Multilingual>();

            using (var package = new ExcelPackage(stream))
            {
                var workBook = package.Workbook;

                if (workBook == null)
                    return;

                if (workBook.Worksheets.Count == 0)
                    return;

                foreach (var currentWorksheet in workBook.Worksheets)
                {
                    var newLanguageRows = GetLanguageColumns(currentWorksheet);

                    for (var i = 2; i < currentWorksheet.Cells.End.Row; i++)
                    {
                        foreach (var newLanguageRow in newLanguageRows)
                        {
                            var value = currentWorksheet.Cells[i, newLanguageRow.Key].GetValue<string>();

                            if (String.IsNullOrWhiteSpace(value))
                                continue;

                            var entry = new BusinessEntities.Multilingual
                            {
                                Key = currentWorksheet.Cells[i, KeyColumn].GetValue<string>(),
                                PropertyName = currentWorksheet.Cells[i, PropertyNameColumn].GetValue<string>(),
                                LanguageIsoCode = newLanguageRow.Value,
                                Text = value
                            };

                            multilinguals.Add(entry);
                        }
                    }
                }
            }

            repository.Upsert(multilinguals);
        }

        private static IDictionary<int, string> GetLanguageColumns(ExcelWorksheet sheet)
        {
            var headers = new Dictionary<int, string>();

            for (var i = MasterColumn + 1; i < sheet.Cells.End.Column; i++)
            {
                var header = sheet.Cells[1, i].GetValue<string>();

                if (String.IsNullOrWhiteSpace(header) || !header.Contains("[NEW]"))
                    continue;

                var language =
                    header.Split(new[] { '[' }, StringSplitOptions.RemoveEmptyEntries).First().ToLowerInvariant();
                headers.Add(i, language);
            }

            return headers;
        }

    }
}