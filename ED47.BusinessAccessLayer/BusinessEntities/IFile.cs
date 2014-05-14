using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Principal;
using System.Web;
using ED47.Stack.Reflector.Attributes;
using Newtonsoft.Json;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    [Model]
    public interface IFile
    {
        int GroupId { get; set; }

        [MaxLength(200)]
        string BusinessKey { get; set; }

        [MaxLength(200)]
        string Name { get; set; }

        [MaxLength(2)]
        string Lang { get; set; }

        int Version { get; set; }
        bool LoginRequired { get; set; }
        Guid Guid { get; set; }
        string MimeType { get; set; }
        bool Encrypted { get; set; }
        string Url { get; }
        string KeyHash { get; set; }
        int Id { get; set; }
       
        DateTime CreationDate { get; }
        string CreatorUsername { get; set; }
        string Metadata { get; }
        void Write(string content);
        string ReadText(bool addView = true);
        /// <summary>
        /// Copy the content of the file into the destination stream.
        /// </summary>
        /// <param name="destination">The destination stream.</param>
        /// <returns>[True] if the content has been copied else [False]</returns>
        bool CopyTo(Stream destination);

       

        /// <summary>
        /// Open a stream on the file
        /// </summary>
        /// <param name="addView">if set to <c>true</c> [add view].</param>
        /// <returns></returns>
        Stream OpenRead(bool addView = false);

        /// <summary>
        /// Opens a write only stream on the repository file.
        /// CALL Flush() ON THE STREAM BEFORE EXITING THE USING BLOCK!
        /// </summary>
        /// <returns>The stream</returns>
        Stream OpenWrite();

        Stream Open();

        /// <summary>
        /// Writes the specified disk file into the repository file.
        /// </summary>
        /// <param name="file">The file to be copied.</param>
        void Write(FileInfo file);

        void Write(Stream stream);

        /// <summary>
        /// Gets the mime type of the file.
        /// </summary>
        /// <returns></returns>
        string GetContentType();

        /// <summary>
        /// Adds a read audit for this file.
        /// </summary>
        /// <param name="viewer">The viewer.</param>
        /// <param name="viewerAddress">The IP address of the viewer.</param>
        void AddView(IPrincipal viewer, string viewerAddress = null);

        IFile Duplicate();
        string GetMetadataViewName();
        dynamic GetMetadata();
    }
}