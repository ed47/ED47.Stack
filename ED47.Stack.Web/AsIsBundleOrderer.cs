using System.Collections.Generic;
using System.IO;
using System.Web.Optimization;

namespace ED47.Stack.Web
{
    public class AsIsBundleOrderer : IBundleOrderer
    {
      
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }
}