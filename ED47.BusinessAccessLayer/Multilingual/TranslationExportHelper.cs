﻿using System;
using System.Collections.Generic;
using System.Linq;
using ED47.BusinessAccessLayer.BusinessEntities;
using ED47.BusinessAccessLayer.Excel;
using ED47.Stack.Web;

namespace ED47.BusinessAccessLayer.Multilingual
{
    public static class TranslationExportHelper
    {
        public static ExcelFile ExportTranslations<TEntity, TBusinessEntity>(this IEnumerable<TBusinessEntity> entities, ExcelFile existingFile = null) where TEntity : DbEntity where TBusinessEntity : class, IBusinessEntity, new()
        {
            var keys = entities.ToDictionary(el => MultilingualRepositoryFactory.Default.GetTranslationtKey<TEntity, TBusinessEntity>(typeof(TEntity).Name, el), el => (object)el);
            var multilingualProperties = MultilingualRepositoryFactory.Default
                                            .GetMultilingualProperties<TEntity>()
                                            .Select(el => el.Name);
            var multilinguals = MultilingualRepositoryFactory.Default
                                    .GetTranslations(keys, properties: multilingualProperties, includeMissingTranslations: true)
                                    .GroupBy(el => el.Key);

            var languageColumns = new List<string>();
            var jEntities = new JsonObjectList();

            foreach (var multilingual in multilinguals)
            {
                foreach (var entryForProperty in multilingual.GroupBy(el => el.PropertyName))
                {
                    var jEntity = new JsonObject();
                    jEntity["Key"] = multilingual.Key;
                    jEntity["Property"] = entryForProperty.Key;

                    foreach (var entry in entryForProperty)
                    {
                        var original = entry.LanguageIsoCode.ToUpperInvariant() + "[ORIG]";
                        var newTranslation = entry.LanguageIsoCode.ToUpperInvariant() + "[NEW]";

                        if (!languageColumns.Contains(original))
                        {
                            languageColumns.Add(original);

                            if (entry.LanguageIsoCode != "MASTER")
                                languageColumns.Add(newTranslation);
                        }

                        jEntity[original] = entry.Text;

                        if (entry.LanguageIsoCode != "MASTER")
                            jEntity[newTranslation] = String.Empty;
                    }

                    if (!String.IsNullOrWhiteSpace(jEntity["MASTER[ORIG]"].ToString()))
                        jEntities.Add(jEntity);
                }
            }

            var excelFile = existingFile ?? new ExcelFile
            {
                FileName = "TranslationExport" + typeof(TBusinessEntity).Name + ".xlsx",
                BusinessKey = "TranslationExport"
            };

            var sheet = excelFile.AddSheet(jEntities, typeof(TBusinessEntity).Name);

            sheet.AddColumns(
                new ExcelColumn {PropertyName = "Key", DisplayName = "Key", IsReadOnly = true },
                new ExcelColumn {PropertyName = "Property", DisplayName = "Property", IsReadOnly = true  });

            sheet.AddHeader(new ExcelColumn(), new ExcelColumn(), new ExcelColumn());

            var masterColumn = languageColumns.SingleOrDefault(el => el.Contains("MASTER"));
            if (masterColumn != null)
                sheet.AddColumns(new ExcelColumn { PropertyName = masterColumn, DisplayName = masterColumn });

            var languages = Lang.GetLanguages().ToDictionary(el => el.IsoCode, el => el);

            foreach (var languageColumn in languageColumns.Where(el => !el.Contains("MASTER")))
            {
                sheet.AddColumns(new ExcelColumn { PropertyName = languageColumn, DisplayName = languageColumn });

                if (languageColumn.Contains("[ORIG]"))
                    sheet.AddHeader(new ExcelColumn
                    {
                        HeaderColSpan = 1,
                        DisplayName = languages[languageColumn.Replace("[ORIG]", String.Empty).ToLowerInvariant()].Name
                    });
            }
            
            return excelFile;
        }
    }
}