using System.Collections.Generic;
using System.IO;
using System.Web.Optimization;

namespace ED47.Stack.Web
{
    public class AsIsBundleOrderer : IBundleOrderer
    {
        public virtual IEnumerable<FileInfo> OrderFiles(BundleContext context, IEnumerable<FileInfo> files)
        {
            return files;
        }
    }
}