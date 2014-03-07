using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer.Azure
{
    public class AzureFileStorage : IFileStorage
    {
        public void Write(IFile file, string content)
        {
            var blob = Storage.GetBlob("files", file.Id.ToString(CultureInfo.InvariantCulture));
            blob.UploadText(content);
        }

        public string ReadText(IFile file)
        {
            var blob = Storage.GetBlob("files", file.Id.ToString(CultureInfo.InvariantCulture));
            return blob.DownloadText();
        }

        public bool CopyTo(IFile file, Stream destination)
        {
            var blob = Storage.GetBlob("files", file.Id.ToString(CultureInfo.InvariantCulture));
            using (var s = blob.OpenRead())
            {
                s.CopyTo(destination);
            }
            return true;
        }

        public Stream OpenRead(IFile file )
        {
            var blob = Storage.GetBlob("files", file.Id.ToString(CultureInfo.InvariantCulture));

            if (!blob.Exists())
                return null;         

            return blob.OpenRead();
        }

        public Stream OpenWrite(IFile file)
        {
            var blob = Storage.GetBlob("files", file.Id.ToString(CultureInfo.InvariantCulture));
         
            return blob.OpenWrite();
        }

        public void Write(IFile file,Stream stream)
        {
            var blob = Storage.GetBlob("files", file.Id.ToString(CultureInfo.InvariantCulture));
            using (var s = blob.OpenWrite())
            {
                stream.CopyTo(s);
            }
        }
    }
}
