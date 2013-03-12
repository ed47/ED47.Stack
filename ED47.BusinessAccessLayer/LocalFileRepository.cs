using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using File = ED47.BusinessAccessLayer.BusinessEntities.File;


namespace ED47.BusinessAccessLayer
{
    public class LocalFileRepository : IFileRepository
    {
        public static string LocalFileRepositoryPath { get; set; }

        private static DirectoryInfo GetFileDirectoryInfo(File file)
        {
            var autoDir = Math.Floor(Convert.ToDouble(file.Id / 1000))+1;
            var di = new DirectoryInfo(Path.Combine(LocalFileRepositoryPath, autoDir.ToString(CultureInfo.InvariantCulture)));
            if(!di.Exists)
            {
                di.Create();
            }
            return di;
        }

        private static FileInfo GetFileInfo(File file)
        {
            var di = GetFileDirectoryInfo(file);
            return new FileInfo(Path.Combine(di.FullName, file.Id.ToString(CultureInfo.InvariantCulture)));
        }

    
        public bool Write(File file, Byte[] content = null)
        {
            var fi = GetFileInfo(file);
            if (content != null)
            {
                using (var fs = fi.Open(FileMode.CreateNew))
                {
                    fs.Write(content, 0, content.Length);
                }

            }
            return true;
        }

        public bool Append(File file, byte[] content)
        {
            var fi = GetFileInfo(file);
            if (content != null)
            {
                using (var fs = fi.Open(FileMode.Append))
                {
                    fs.Write(content, 0, content.Length);
                }

            }
            return true;
        }

        public bool Delete(File file)
        {
            var fi = GetFileInfo(file);
            if (fi.Exists) fi.Delete();
            return !fi.Exists;
        }

        public bool Exist(File file)
        {
            var fi = GetFileInfo(file);
            return fi.Exists;
        }

        public Stream OpenWrite(File file)
        {
            var fi = GetFileInfo(file);
            return fi.OpenWrite() ;

        }

        public Stream OpenRead(File file)
        {
            var fi = GetFileInfo(file);
            return fi.Exists ? fi.OpenRead() : null;
        }


    }
}
