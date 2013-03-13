using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ED47.BusinessAccessLayer.Couchbase;

namespace ED47.Communicator.Shared.BusinessEntities
{
    public class Category : BaseDocument
    {
        private static string CalculateKey(string code)
        {
            return "Category?code=" + code;
        }

        public string Name { get; set; }
        public string Code { get; set; }

        public int[] Data { get; set; }

        public override string GetKey()
        {
            return CalculateKey(Code);
        }

        public static Category Get(string code)
        {
            return Repository.Get<Category>(new {Code = code});
        }
    }
}
