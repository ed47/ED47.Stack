using System.Collections.Generic;

namespace ED47.Stack.Web.Multilingual
{
    public interface ITranslationRepository : IDictionary<string, TranslationDictionary>
    {
        string TranslationsPath { get; set; }
        TranslationDictionary DefaultDictionnary { get; set; }
        IEnumerable<string> GetAllKeys(string language = null);
        TranslationDictionary AddLanguage(string language);
        TranslationDictionary GetDictionary(string langage);
        TranslationDictionary GetOrCreateDictionary(string langage);
        string GetFromCache(string key);
        void CacheData(string key, string data, string language);
        void ClearCache(string language);
        IEnumerable<string> GetAvailableLanguages();

        /// <summary>
        /// Updates a multilingual entry.
        /// </summary>
        void UpdateEntry(string language, string key, string value, object attributes = null);

        void DeleteEntry(string lan, string key);

        string GetCurrentTranslation(string key, params object[] args);

        /// <summary>
        /// Gets a multilignual string in a given culture-language.
        /// </summary>
        string GetTranslation(string key, string language, params object[] args);

        /// <summary>
        /// Gets a multilignual string in the current UI culture with a pluralization.
        /// </summary>
        /// <returns></returns>
        string GetTranslationPluralized(string key, int pluralizeCount, params object[] args);

        IEnumerable<string> FindKeys(string key, string language);
    }
}