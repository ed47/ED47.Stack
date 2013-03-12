using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class Audit : BusinessEntity
    {
        [Key]
        public virtual int Id { get; set; }

        public virtual DateTime CreationDate { get; set; }

        [MaxLength(50)]
        public virtual string UserName { get; set; }
        
        [MaxLength(1000)]
        public virtual string JsonData { get; set; }

        [MaxLength(100)]
        public virtual string SenderName { get; set; }

        [MaxLength(100)]
        public virtual string ActionName { get; set; }

        /// <summary>
        /// Adds an audit entry.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="sender"></param>
        /// <param name="jsonData"></param>
        /// <param name="auditFilters"></param>
        public static void Add(string actionName, string sender, object jsonData, object auditFilters)
        {
            var audit = new Audit()
                            {
                                ActionName = actionName,
                                SenderName = sender,
                                CreationDate = DateTime.Now,
                                UserName = BaseUserContext.Instance.UserName,
                                JsonData = JsonConvert.SerializeObject(jsonData, Formatting.None)
                            };

            audit.Insert();

            if (auditFilters == null) return;

            var auditFilterType = auditFilters.GetType();

            if(auditFilterType.IsGenericType)
                auditFilterType = auditFilterType.GetGenericArguments()[0];

            var filterItems = auditFilters.GetType().GetProperties();

            foreach (var filterItem in filterItems)
            {
                var isGeneric = filterItem.PropertyType.IsGenericType;

                if (!isGeneric && filterItem.PropertyType != typeof(Int32)) throw new InvalidOperationException("Audit filter only supports Int32 properties.");
                if (isGeneric && filterItem.PropertyType.GetGenericArguments()[0] != typeof(Int32)) throw new InvalidOperationException("Audit filter only supports Int32 properties.");

                var value = filterItem.GetValue(auditFilters, null) as int?;

                if(!value.HasValue) continue;

                var auditFilter = new AuditFilter
                                      {
                                          AuditId = audit.Id,
                                          ReferenceName = filterItem.Name,
                                          ReferenceValue = value.Value
                                      };

                auditFilter.Insert();
            }
        }

        private void Insert()
        {
            BaseUserContext.Instance.Repository.Add<BusinessAccessLayer.Entities.Audit, Audit>(this);
        }
    }
}
