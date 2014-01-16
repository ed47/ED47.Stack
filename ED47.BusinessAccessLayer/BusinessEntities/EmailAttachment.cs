namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class EmailAttachment : BusinessEntity
    {
#pragma warning disable 169
        private static readonly string[] Includes = new[] { "File"};
#pragma warning restore 169

        public virtual int Id { get; set; }

        public virtual int FileId { get; set; }

        private IFile _file;
        public virtual IFile File
        {
            get { return _file ?? (_file = FileRepositoryFactory.Default.Get(FileId)); }
        }
        
        public virtual int EmailId { get; set; }

        private Email _email;
        public virtual Email Email
        {
            get { return _email ?? (_email = Email.Get(FileId)); }
        }
        
        public static EmailAttachment Create(int fileId, int emailId)
        {
            var attachement = new EmailAttachment
                {
                 FileId = fileId,
                 EmailId = emailId
                };

            BaseUserContext.Instance.Repository.Add<Entities.EmailAttachment, EmailAttachment>(attachement);

            return attachement;
        }

        public static EmailAttachment Get(int id)
        {
            return BaseUserContext.Instance.Repository.Find<Entities.EmailAttachment, EmailAttachment>(el => el.Id == id);
        }

        public void Delete()
        {
            BaseUserContext.Instance.Repository.Delete<Entities.EmailAttachment, EmailAttachment>(this);
        }
    }
}
