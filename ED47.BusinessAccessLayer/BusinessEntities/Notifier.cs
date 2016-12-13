using System;
using System.Text.RegularExpressions;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public interface IWithBusinessKey
    {
        string GetBusinessKey();
    }

    public class Notifier<TEntity, TArgs> where TEntity : IWithBusinessKey
    {
        public string Regex { get; set; }
        public Action<TEntity, Match, TArgs> Action { get; set; }
        public string Guid { get; set; }

        public Notifier()
        {
            Guid = System.Guid.NewGuid().ToString();
        }

        public bool TryNotify(TEntity entity, TArgs args)
        {
            var m = new Regex(Regex).Match(entity.GetBusinessKey());
            if (Action == null || !m.Success) return false;

            Action(entity, m, args);
            return true;
        }        
    }
}