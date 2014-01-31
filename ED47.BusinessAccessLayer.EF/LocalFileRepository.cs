using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ED47.BusinessAccessLayer.BusinessEntities;
using ED47.Stack.Web;
using Ninject;

namespace ED47.BusinessAccessLayer.EF
{
    public class LocalFileRepository : BusinessAccessLayer.LocalFileRepository
    {
        public override IFile Get(int id)
        {
            var context = BaseUserContext.Instance;

            if (context == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            return context.Repository.Find<Entities.File, File>(el => el.Id == id);
        }
        
        // <summary>
        /// Creates a new file into the repository.
        /// </summary>
        /// <typeparam name="TFile">The type of the file.</typeparam>
        /// <param name="name">The name (with extension)</param>
        /// <param name="businessKey">The business key.</param>
        /// <param name="groupId">The group id.</param>
        /// <param name="requiresLogin">if set to <c>true</c> [requires login].</param>
        /// <param name="encrypted">Pass <c>True</c> to encrypt the file on disk.</param>
        /// <returns></returns>
        public override IFile CreateNewFile(string name, string businessKey, int? groupId = 0, bool requiresLogin = true, string langId = null, bool encrypted = false)
        {
            var previous = GetFileByKey(businessKey);
            var version = previous != null ? previous.Version + 1 : 1;

            var file = new File
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
            BaseUserContext.Instance.Repository.Add<Entities.File, File>(file);

            return file;
        }

        public override IFile GetFileByKey(string businessKey, int? version = null) 
        {
            var context = BaseUserContext.Instance;

            if (context == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            return context.Repository.Where<Entities.File, File>(el => !el.IsDeleted && el.BusinessKey == businessKey && (version == null || el.Version == version.Value)).FirstOrDefault();
        }

        public IEnumerable<IFile> GetAll()
        {
            var context = BaseUserContext.Instance;
            if (context == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            return context.Repository.GetAll<Entities.File, File>();
        }

        public override IEnumerable<IFile> GetHistoryFilesByKey(string businessKey)
        {
            var context = BaseUserContext.Instance;

            if (context == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            return context.Repository
                    .Where<Entities.File, File>(el => !el.IsDeleted && el.BusinessKey == businessKey)
                    .OrderByDescending(el => el.CreationDate);
        }

        public IFile Upload(string businessKey, int groupId = 0)
        {
            var cxt = HttpContext.Current;
            if (cxt == null) return null;

            if (cxt.Request.Files.Count == 0) return null;
            var file = cxt.Request.Files[0];
            var res = CreateNewFile(Path.GetFileName(file.FileName), businessKey, groupId, false);
            using (var s = res.OpenWrite())
            {
                file.InputStream.CopyTo(s);
                s.Flush();
            }
            return res;
        }

        /// <summary>
        /// Gets the download URL for a file.
        /// </summary>
        /// <param name="file">The file reference.</param>
        /// <param name="specificVersion">If true, a specific version is fetched [default]. If false, latest version will be downloaded.</param>
        /// <returns>The URL to download the file.</returns>
        public string GetUrl(File file, bool specificVersion = true)
        {
            if (specificVersion)
                return String.Format("/fileRepository.axd?id={0}&token={1}", file.Id, file.Guid);
            return String.Format("/fileRepository.axd?key={0}&token={1}", file.BusinessKey, file.Guid);
        }

        public IEnumerable<IFile> GetFilesByKeyFamilly(string businessKeyFamilly)
        {
            return ((Repository)BaseUserContext.Instance.Repository).ExecuteTableFunction<File>("File_GetFilesByKeyFamilly", businessKeyFamilly);
        }

        public IEnumerable<IFile> GetFilesByGroupId(int groupId, int page = 0, int pageSize = 50)
        {
            return ((Repository)BaseUserContext.Instance.Repository).ExecuteTableFunction<File>("File_GetFilesByGroupId", groupId, page, pageSize);
        }
    }
}
