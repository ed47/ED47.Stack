using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Omu.ValueInjecter;
using System.Linq;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Generic DTO for transfering data between client and business entities.
    /// </summary>
    public class ClientData : Dictionary<string, object>
    {
        public ClientData()
        {

        }

        public ClientData(FormCollection formCollection, bool allowArrays = false)
        {
            foreach (string key in formCollection.Keys)
            {
                Add(allowArrays ? key.Replace("[]", String.Empty) : key, formCollection[key.ToString()]);
            }
        }
        public TBusinessEntity To<TBusinessEntity>(string[] whiteList = null) where TBusinessEntity : new()
        {
            var result = new TBusinessEntity();
            return WriteTo(result, whiteList);
        }

        public TBusinessEntity WriteTo<TBusinessEntity>(TBusinessEntity target, string[] whiteList = null)
        {
            var filter = this as Dictionary<string, object>;
            if (whiteList != null)
            {
                var dict1 = filter;
                filter = whiteList.Where(dict1.ContainsKey).ToDictionary(el => el, el => filter[el]);
            }

            target.InjectFrom<DictionaryInjection>(filter);

            var businessEntity = target as IBusinessEntity;
            if (businessEntity != null)
                businessEntity.ClientData = this;

            return target;
        }
    }
}