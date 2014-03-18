using System.ComponentModel.DataAnnotations;

namespace ED47.BusinessAccessLayer.File
{
	public class FileExtensionWhiteList : BusinessEntity
	{
		public virtual int Id { get; set; }

		[MaxLength(10)]
		public virtual string Extension { get; set; }
	}
}