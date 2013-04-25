using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public interface IFile
    {
        int Id { get; set; }
        int GroupId { get; set; }
        string BusinessKey { get; set; }
        string Name { get; set; }
        string Lang { get; set; }
        int Version { get; set; }
        Boolean LoginRequired { get; set; }
        Guid Guid { get; set; }
        string MimeType { get; set; }
        string Url { get;}
        void Write(string content);
        string ReadText(bool addView = true);
        bool CopyTo(Stream destination);
        bool CopyTo(FileInfo destination);
        Stream OpenRead(bool addView = false);
        Stream OpenWrite();
        void Write(FileInfo file);
        string GetContentType();
    }
}