using System.Collections.Generic;
using System.Web.Mvc;
using Omu.ValueInjecter;
using System.Linq;
using System;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Generic DTO for transfering data between client and business entities.
    /// </summary>
    public class ClientData : Dictionary<string,object>
    {
        public ClientData ()
        {            
        }

        public ClientData(FormCollection formCollection, string removePrefix = null)
        {
            foreach (var key in formCollection.Keys)
            {
                var cleanKey = String.IsNullOrWhiteSpace(removePrefix) ? key.ToString() : key.ToString().Replace(removePrefix + ".", "");
                Add(cleanKey, formCollection[key.ToString()]);
            }
        }
        public TBusinessEntity To<TBusinessEntity>(string[] whiteList = null) where TBusinessEntity : new()
        {
            var result = new TBusinessEntity();
            return WriteTo(result,whiteList);
        }

        public TBusinessEntity WriteTo<TBusinessEntity>(TBusinessEntity target, IEnumerable<string> whiteList = null) where TBusinessEntity : new()
        {
            var filter = this as Dictionary<string,object>;
            if(whiteList != null)
            {
                var dict1 = filter;
                filter = whiteList.Where(dict1.ContainsKey).ToDictionary(el => el, el => filter[el]);
            }

            target.InjectFrom<DictionaryInjection>(filter);

            var businessEntity = target as BusinessEntity;
            if (businessEntity != null)
                businessEntity.ClientData = this;

            return target;
        }
    }
}