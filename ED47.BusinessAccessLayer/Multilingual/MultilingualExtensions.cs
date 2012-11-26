﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.Multilingual
{
    public static class MultilingualExtensions
    {


        /// <summary>
        /// Translates a collection of entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="businessEntities">The collection of entities to translate.</param>
        /// <param name="isoLanguageCode">The 2-letter ISO code for the language to translate to.</param>
        /// <param name="dbContext">The Entity Framework DB Context.</param>
        public static void Translate<TEntity, TBusinesEntity>(this Repository repository, IEnumerable<TBusinesEntity> businessEntities, string isoLanguageCode)
            where TEntity : DbEntity
            where TBusinesEntity : BusinessEntity, new() {
            if (!businessEntities.Any())
                return;

            isoLanguageCode = isoLanguageCode.Trim().ToLower();
            var entityName = typeof(TBusinesEntity).Name;

            var keys = businessEntities.SelectMany(b => b.GetKeys<TEntity>().Select(kv => entityName + "[" + kv.Value + "]"));
            var translations = BusinessEntities.Multilingual.GetTranslations(isoLanguageCode, keys, repository.DbContext);

            foreach (var entity in businessEntities) {
                var item = entity; //HACK: To prevent modified closure bug in .Net 4.0
                
                var key = entityName + "[" + string.Join(",", item.GetKeys<TEntity>().Select(kv => kv.Value)) + "]";
                BusinessEntities.Multilingual.ApplyTranslation(item, translations.Where(t => t.Key == key));
            }
        }

        private static void seekex(IEnumerable<object> iEnumerable) {
            
        }


    }
}