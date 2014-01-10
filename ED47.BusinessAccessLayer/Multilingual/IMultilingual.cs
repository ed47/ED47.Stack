namespace ED47.BusinessAccessLayer.Multilingual
{
    public interface IMultilingual
    {
        /// <summary>
        /// The composite key for the translation entry in the following format: EntityName[Id]
        /// For entities with multiple IDs, separate by commas with no spaces.
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// The translated property's name.
        /// </summary>
        string PropertyName { get; set; }

        /// <summary>
        /// The translation's ISO language code.
        /// </summary>
        string LanguageIsoCode { get; set; }

        /// <summary>
        /// The translated text.
        /// </summary>
        string Text { get; set; }
    }
}