using System;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public interface IFileBoxItem
    {
        string Id { get; set; }
        string Name { get; set; }
        string FileExtension { get; set; }
        string Comment { get; set; }
        int FileId { get; set; }
        DateTime CreationDate { get; set; }
        IFile File { get; }
        IFile LoadFile();
    }
}