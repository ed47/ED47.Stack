using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Principal;
using System.Web;
using ED47.BusinessAccessLayer.BusinessEntities;
using ED47.Stack.Web;
using Ninject;

namespace ED47.BusinessAccessLayer.EF
{
	public class File : BusinessEntity, IFile
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

        public virtual DateTime CreationDate { get; set; }
		
		private string _Url;
		public string Url
		{
            get { return _Url ?? (_Url = FileRepository.GetUrl(this)); }
		}

		public void Write(string content)
		{
			using (var s = OpenWrite())
			{
				if (s == null) return;
				var sw = new StreamWriter(s);
				sw.Write(content);
				sw.Close();
				s.Flush();
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
				return Cryptography.Decrypt(FileRepository.OpenRead(this), KeyHash);

			return FileRepository.OpenRead(this);
		}

		public string KeyHash { get; set; }

		/// <summary>
		/// Opens a write only stream on the repository file.
		/// CALL Flush() ON THE STREAM BEFORE EXITING THE USING BLOCK!
		/// </summary>
		/// <returns>The stream</returns>
		public Stream OpenWrite()
		{
			if (Encrypted)
			{
				string keyHash;
				var stream = Cryptography.Encrypt(FileRepository.OpenWrite(this), out keyHash);
				KeyHash = keyHash;
				Save();
				BaseUserContext.Instance.Commit();

				return stream;
			}

			return FileRepository.OpenWrite(this);
		}

		private void Save()
		{
			BaseUserContext.Instance.Repository.Update<Entities.File, File>(this);
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
						rs.CopyTo(s);
					}
				}
				s.Flush();
			}
		}

        public void Write(Stream stream)
        {
            using (var s = OpenWrite())
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(s);
                s.Flush();
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
