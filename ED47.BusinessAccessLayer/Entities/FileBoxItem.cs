using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace ED47.BusinessAccessLayer.Entities
{
    public class FileBoxItem : BaseDbEntity
    {
        [MaxLength(200)]
        public virtual string Name { get; set; }

        [MaxLength(10)]
        public virtual string FileExtension { get; set; }

        [MaxLength(2000)]
        public virtual string Comment { get; set; }

        public virtual int FileBoxId { get; set; }

        [ForeignKey("FileBoxId")]
        public virtual FileBox FileBox { get; set; }

        public virtual int? FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual Entities.File File { get; set; }

        public virtual bool IsPublic { get; set; }

        [DefaultValue(false)]
        public bool IsFolder { get; set; }

        public int? FolderId { get; set; }

        [MaxLength(250)]
        public string ReportingScope { get; set; }
    }
}