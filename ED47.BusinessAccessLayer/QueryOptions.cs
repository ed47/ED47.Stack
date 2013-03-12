using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ED47.BusinessAccessLayer
{
    public class QueryOptions<TEntity> where TEntity : DbEntity
    {
        public IEnumerable<string> Includes { get; set; }

        public int? Take { get; set; }

        public int? Skip { get; set; }
    }
}
