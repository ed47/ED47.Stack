using System;
using System.Globalization;
using System.IO;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer.File
{
    public class LocalFileStorage : IFileStorage
    {
        public static string LocalFileRepositoryPath { get; set; }

        private static DirectoryInfo GetFileDirectoryInfo(IFile file)
        {
            var autoDir = Math.Floor(Convert.ToDouble(file.Id / 1000)) + 1;
            var di = new DirectoryInfo(Path.Combine(LocalFileRepositoryPath, autoDir.ToString(CultureInfo.InvariantCulture)));
            if (!di.Exists)
            {
                di.Create();
            }
            return di;
        }

        private static FileInfo GetFileInfo(IFile file)
        {
            var di = GetFileDirectoryInfo(file);
            return new FileInfo(Path.Combine(di.FullName, file.Id.ToString(CultureInfo.InvariantCulture)));
        }

        public void Write(IFile file, string content)
        {
            var fi = GetFileInfo(file);

            System.IO.File.WriteAllText(fi.FullName, content);
        }

        public string ReadText(IFile file)
        {
            var fi = GetFileInfo(file);

            return System.IO.File.ReadAllText(fi.FullName);
        }

        /// <summary>
        /// Copy the content of the file into the destination stream.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="destination">The destination stream.</param>
        /// <returns>[True] if the content has been copied else [False]</returns>
        public bool CopyTo(IFile file, Stream destination)
        {
            var fi = GetFileInfo(file);

            using (var s = fi.OpenRead())
            {
                s.CopyTo(destination);
            }

            return true;
        }

        /// <summary>
        /// Open a stream on the file
        /// </summary>
        public Stream OpenRead(IFile file)
        {
            var fi = GetFileInfo(file);

            return fi.OpenRead();
        }

        /// <summary>
        /// Opens a write only stream on the repository file.
        /// CALL Flush() ON THE STREAM BEFORE EXITING THE USING BLOCK!
        /// </summary>
        /// <returns>The stream</returns>
        public Stream OpenWrite(IFile file)
        {
            var fi = GetFileInfo(file);

            return fi.OpenWrite();
        }

        public Stream Open(IFile file)
        {
            var fi = GetFileInfo(file);

            return fi.Open(FileMode.Open, FileAccess.ReadWrite);
        }

        public void Write(IFile file, Stream stream)
        {
            var fi = GetFileInfo(file);

            using (var s = fi.OpenWrite())
            {
                stream.CopyTo(s);
            }
        }
    }
}
