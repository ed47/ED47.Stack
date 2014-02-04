namespace ED47.Stack.Web.Multilingual
{
    public interface ITranslationEntry
    {
        TranslationDictionary Dictionary { get; set; }
        string Key { get; set; }
        TranslationFile File { get; set; }
        string Value { get; set; }
        void Update(string value, object attributes = null);
    }
}