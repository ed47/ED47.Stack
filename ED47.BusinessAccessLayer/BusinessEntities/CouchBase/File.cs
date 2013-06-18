using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using ED47.BusinessAccessLayer.Couchbase;
using ED47.BusinessAccessLayer.Tools;
using ED47.Stack.Web;
using Ninject;

namespace ED47.BusinessAccessLayer.BusinessEntities.CouchBase
{
    public class File : BaseDocument,IFile
    {
        public int GroupId { get; set; }

        public string BusinessKey { get; set; }

        public string Name { get; set; }

        public string Lang { get; set; }

        public int Version { get; set; }

        public bool LoginRequired { get; set; }

        public Guid Guid { get; set; }

        public string MimeType { get; set; }
        internal static readonly IFileRepository FileRepository = BusinessComponent.Kernel.Get<IFileRepository>();

        private string _Url;
        public string Url
        {
            get { return _Url ?? (_Url = GetUrl(this)); }
        }

        public static string GetUrl(File file, bool specificVersion = true)
        {
            var request = HttpContext.Current.Request;
            var appUrl = String.Format("{0}://{1}",request.Url.Scheme,request.Url.Authority);
            if (specificVersion)
                return String.Format("{0}/fileRepository.axd?id={1}&token={2}",appUrl, file.Id, file.Guid);
            return String.Format("{0}/fileRepository.axd?key={1}&token={2}", appUrl, file.BusinessKey, file.Guid);
        }

        public void Write(string content)
        {
            using (var s = OpenWrite())
            {
                if (s == null) return;
                var sw = new StreamWriter(s);
                sw.Write(content);
                sw.Close();
            }
        }

        public string ReadText(bool addView = true)
        {
            using (var s = OpenRead(addView))
            {
                if (s == null) return "";
                var sr = new StreamReader(s);
                return sr.ReadToEnd();
            }
        }

        public bool CopyTo(Stream destination)
        {
            using (var s = OpenRead())
            {
                if (s != null)
                {
                    s.CopyTo(destination);
                    return true;
                }
                return false;
            }
        }

        public bool CopyTo(FileInfo destination)
        {
            using (var s = OpenRead())
            {
                if (s != null)
                {
                    s.CopyTo(destination.OpenWrite());
                    return true;
                }
                return false;
            }
        }

        public Stream OpenRead(bool addView = false)
        {
            return FileRepository.OpenRead(this);
        }

        public Stream OpenWrite()
        {
            return FileRepository.OpenWrite(this);
        }

        public void Write(FileInfo file)
        {
            if (!System.IO.File.Exists(file.FullName)) return;
            using (var s = OpenWrite())
            {
                using (var rs = file.OpenRead())
                {
                    if (rs.CanRead)
                        rs.CopyTo(s);
                }
            }
        }

        public string GetContentType()
        {
            if (String.IsNullOrEmpty(Name)) return "";
            return MimeTypeHelper.GetMimeType(Name);
        }

        public static File Get(int id)
        {
            return CouchbaseRepository.Get<File>(new { Id = id });
        }

        public static File Get(string businessKey)
        {
            return null;
        }

        public static TFile CreateNewFile<TFile>(string name, string businessKey, int? groupId = 0, bool requiresLogin = true, string langId = null) where TFile : File, new()
        {
            var previous = GetFileByKey(businessKey);
            var version = previous != null ? previous.Version + 1 : 1;

            var file = new TFile
            {
                Name = name,
                BusinessKey = businessKey,
                MimeType = MimeTypeHelper.GetMimeType(name),
                Version = version,
                LoginRequired = requiresLogin,
                Guid = Guid.NewGuid(),
                Lang = langId,
                GroupId = groupId.GetValueOrDefault(0)
            };
            file.Save();

            return file;
        }

        public static TFile Upload<TFile>(string businessKey, int groupId = 0) where TFile : File, new()
        {
            var cxt = HttpContext.Current;
            if (cxt == null) return null;

            if (cxt.Request.Files.Count == 0) return null;
            var file = cxt.Request.Files[0];
            var res = CreateNewFile<TFile>(Path.GetFileName(file.FileName), businessKey, groupId, false);
            using (var s = res.OpenWrite())
            {
                file.InputStream.CopyTo(s);
            }
            return res;
        }

        public static File Upload(string businessKey, int groupId = 0)
        {
            return Upload<File>(businessKey, groupId);
        }

        public static File UploadAndResize(string businessKey, int w, int h , int groupId = 0)
        {
            return UploadAndResize<File>(businessKey, w, h, groupId);
        }


        public static TFile UploadAndResize<TFile>(string businessKey, int w, int h, int groupId = 0) where TFile : File, new()
        {
            var cxt = HttpContext.Current;
            if (cxt == null) return null;

            if (cxt.Request.Files.Count == 0) return null;


            var temp = Path.GetTempFileName();
            var file = cxt.Request.Files[0];

            if (!file.ContentType.StartsWith("image/"))
                return null;
            
            var validExtensions = new[] {".gif", ".jpeg",".jpg",".png"};
            var fileName = Path.GetFileName(file.FileName);
            var ext = Path.GetExtension(fileName);

            if (validExtensions.Contains(ext.ToLower()))
            {
                var tmpFileName = Path.GetTempPath() + Guid.NewGuid() + ext;
                file.SaveAs(tmpFileName);
                ImageUtilities.Resize(tmpFileName, temp, w, h);
                var res = CreateNewFile<TFile>(fileName, businessKey, groupId, false);
                var resized = System.IO.File.OpenRead(temp);
                using (var s = res.OpenWrite())
                {
                    resized.CopyTo(s);
                }
                resized.Dispose();
                return res;
            }
            return null;

        }

        public static File GetFileByKey(string businessKey)
        {
            return CouchbaseRepository.GetBy<File>("file", "fileByBusinessKey", businessKey); 
        }

        public static IEnumerable<File> All(int start = 0 , int count = 10)
        {
            return CouchbaseRepository.All<File>("File");
        }

        public new bool Delete()
        {
            FileRepository.Delete(this);
            return base.Delete();
        }
    }
}
