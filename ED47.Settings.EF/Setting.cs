using ED47.BusinessAccessLayer;

namespace ED47.Settings.EF
{
    public class Setting : BusinessEntity, ISetting
    {
        public virtual int Id { get; set; }
        public virtual string Path { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string Type { get; set; }
        public virtual string Value { get; set; }
    }
}