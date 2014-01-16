using System.Collections.Generic;

namespace ED47.BusinessAccessLayer
{
    public class QueryOptions<TEntity> where TEntity : DbEntity
    {
        public IEnumerable<string> Includes { get; set; }

        public int? Take { get; set; }

        public int? Skip { get; set; }
    }
}
