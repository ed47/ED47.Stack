using System.Collections.Generic;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer.File
{
    public static class FileExtensions
    {
        public static void CopyTo(this IEnumerable<IFile> files, IFileBox filebox)
        {
            if (files == null)
                return;

            foreach (var file in files)
            {
                var newFile = file.Duplicate();
                filebox.AddFile(newFile);
            }
        }
    }
}
