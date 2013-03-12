namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class AuditFilter : BusinessEntity
    {
        public virtual int AuditId { get; set; }
        
        public virtual string ReferenceName { get; set; }

        public virtual int ReferenceValue { get; set; }

        public void Insert()
        {
            BaseUserContext.Instance.Repository.Add<Entities.AuditFilter, AuditFilter>(this);
        }
    }
}
