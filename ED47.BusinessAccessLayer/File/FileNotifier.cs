using System.Text.RegularExpressions;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer.File
{
    public enum FileActionType
    {
        New,
        Delete
        // Move ?
    }

    public class FileNotifierArgs
    {
        public FileActionType ActionType { get; set; }
        public FileBox FileBox { get; set; }
        public int? FileBoxId { get; set; }
    }

    public class FileNotifier : Notifier<File, FileNotifierArgs>
    {       
    }
}