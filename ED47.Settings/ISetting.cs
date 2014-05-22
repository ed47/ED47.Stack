namespace ED47.Settings
{
    public interface ISetting : ED47.BusinessAccessLayer.IBusinessEntity
    {
        string Path { get; }
        string Name { get; }
        string Description { get; }
        string Value { get; set; }
    }
}
