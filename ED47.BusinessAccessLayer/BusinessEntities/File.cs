using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using ED47.Stack.Reflector.Attributes;
using ED47.Stack.Web;
using Ninject;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    [Model]
    public class File : BusinessEntity
    {
        public virtual int Id { get; set; }

        public virtual int GroupId { get; set; }

        [MaxLength(200)]
        public virtual string BusinessKey { get; set; }

        [MaxLength(200)]
        public virtual string Name { get; set; }

        [MaxLength(2)]
        public virtual string Lang { get; set; }

        public virtual int Version { get; set; }

        public virtual bool LoginRequired { get; set; }

        public virtual Guid Guid { get; set; }
        public virtual string MimeType { get; set; }
        internal static readonly IFileRepository FileRepository = BusinessComponent.Kernel.Get<IFileRepository>();

        public virtual bool Encrypted { get; set; }

        public static TFile GetById<TFile>(int id) where TFile : File, new()
        {
            return BaseUserContext.Instance.Repository.Find<Entities.File, TFile>(el => el.Id == id);
        }

        public static IEnumerable<File> GetAll()
        {
            var context = BaseUserContext.Instance;
            if (context == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            return context.Repository.GetAll<Entities.File, File>();
        }

        public static File Get(int id)
        {
            var context = BaseUserContext.Instance;

            if (context == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            return context.Repository.Find<Entities.File, File>(el => el.Id == id);
        }

        public static TFile GetFileByKey<TFile>(string businessKey, int version = -1) where TFile : File
        {
            var context = BaseUserContext.Instance;

            if (context == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            return context.Repository.ExecuteTableFunction<TFile>("File_GetFileByKey", businessKey, version).FirstOrDefault();
        }

        public static IEnumerable<TFile> GetHistoryFilesByKey<TFile>(string businessKey) where TFile : File
        {
            return BaseUserContext.Instance.Repository.ExecuteTableFunction<TFile>("File_GetHistoryFilesByKey", businessKey);
        }

        public static IEnumerable<TFile> GetFilesByKeyFamilly<TFile>(string businessKeyFamilly) where TFile : File
        {
            return BaseUserContext.Instance.Repository.ExecuteTableFunction<TFile>("File_GetFilesByKeyFamilly", businessKeyFamilly);
        }

        public static IEnumerable<TFile> GetFilesByGroupId<TFile>(int groupId, int page = 0, int pageSize = 50) where TFile : File
        {
            return BaseUserContext.Instance.Repository.ExecuteTableFunction<TFile>("File_GetFilesByGroupId", groupId,
                                                                                     page, pageSize);
        }

        private string _Url;
        public string Url
        {
            get { return _Url ?? (_Url = GetUrl(this)); }
        }

        /// <summary>
        /// Gets the download URL for a file.
        /// </summary>
        /// <param name="file">The file reference.</param>
        /// <param name="specificVersion">If true, a specific version is fetched [default]. If false, latest version will be downloaded.</param>
        /// <returns>The URL to download the file.</returns>
        public static string GetUrl(File file, bool specificVersion = true)
        {
            if (specificVersion)
                return String.Format("/fileRepository.axd?id={0}&token={1}", file.Id, file.Guid);
            return String.Format("/fileRepository.axd?key={0}&token={1}", file.BusinessKey, file.Guid);
        }


        /// <summary>
        /// Creates a new file into the repository.
        /// </summary>
        /// <typeparam name="TFile">The type of the file.</typeparam>
        /// <param name="name">The name (with extension)</param>
        /// <param name="businessKey">The business key.</param>
        /// <param name="groupId">The group id.</param>
        /// <param name="requiresLogin">if set to <c>true</c> [requires login].</param>
        /// <param name="encrypted">Pass <c>True</c> to encrypt the file on disk.</param>
        /// <returns></returns>
        public static TFile CreateNewFile<TFile>(string name, string businessKey, int? groupId = 0, bool requiresLogin = true, string langId = null, bool encrypted = false) where TFile : File, new()
        {
            var previous = GetFileByKey<File>(businessKey);
            var version = previous != null ? previous.Version + 1 : 1;

            var file = new TFile
            {
                Name = name,
                BusinessKey = businessKey,
                MimeType = MimeTypeHelper.GetMimeType(name),
                Version = version,
                LoginRequired = requiresLogin,
                Lang = langId,
                GroupId = groupId.GetValueOrDefault(0),
                Encrypted = encrypted
            };
            BaseUserContext.Instance.Repository.Add<Entities.File, TFile>(file);

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

        /// <summary>
        /// Copy the content of the file into the destination stream.
        /// </summary>
        /// <param name="destination">The destination stream.</param>
        /// <returns>[True] if the content has been copied else [False]</returns>
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


        /// <summary>
        /// Copy the content of the file into the destination stream.
        /// </summary>
        /// <param name="destination">The destination stream.</param>
        /// <returns>[True] if the content has been copied else [False]</returns>
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

        /// <summary>
        /// Open a stream on the file
        /// </summary>
        /// <param name="addView">if set to <c>true</c> [add view].</param>
        /// <returns></returns>
        public Stream OpenRead(bool addView = false)
        {
            HttpContext context = HttpContext.Current;

            if (context != null && addView)
                AddView(context.User, context.Request.UserHostAddress);

            if (Encrypted)
                return Cryptography.Decrypt(FileRepository.OpenRead(this));

            return FileRepository.OpenRead(this);
        }

        /// <summary>
        /// Opens a write only stream on the repository file.
        /// </summary>
        /// <returns>The stream</returns>
        public Stream OpenWrite()
        {
            if (Encrypted)
                return Cryptography.Encrypt(FileRepository.OpenWrite(this));
            else
                return FileRepository.OpenWrite(this);
        }

        /// <summary>
        /// Writes the specified disk file into the repository file.
        /// </summary>
        /// <param name="file">The file to be copied.</param>
        public void Write(FileInfo file)
        {
            if (!System.IO.File.Exists(file.FullName)) return;
            using (var s = OpenWrite())
            {
                using (var rs = file.OpenRead())
                {
                    if (rs.CanRead)
                    {
                        if (Encrypted)
                            Cryptography.Encrypt(rs, s);
                        else
                            rs.CopyTo(s);

                    }
                }
            }
        }


        /// <summary>
        /// Gets the mime type of the file.
        /// </summary>
        /// <returns></returns>
        public string GetContentType()
        {
            if (String.IsNullOrEmpty(Name)) return "";
            return MimeTypeHelper.GetMimeType(Name);
        }


        /// <summary>
        /// Adds a read audit for this file.
        /// </summary>
        /// <param name="viewer">The viewer.</param>
        /// <param name="viewerAddress">The IP address of the viewer.</param>
        public void AddView(IPrincipal viewer, string viewerAddress = null)
        {/*
            var fileView = new DEF_FIV_FileView
            {
                FIV_ViewDate = DateTime.Now,
                FI_ID = FI_ID,
                FIV_ViewAddress = viewerAddress
            };

            if (viewer.Identity.IsAuthenticated)
            {
                var user = DEF_U_User.GetUserByUsername(viewer.Identity.Name);

                if (user != null)
                    fileView.U_ID = user.U_ID;
            }

            fileView.Save();*/
        }


    }
}
