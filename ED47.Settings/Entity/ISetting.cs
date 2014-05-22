namespace ED47.Settings.Entity
{
    public interface ISetting
    {
        string Path { get; }
        string Name { get; }
        string Description { get; }
        string Value { get; set; }
    }
}
