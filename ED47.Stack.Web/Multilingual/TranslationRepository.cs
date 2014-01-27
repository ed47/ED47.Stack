using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ED47.Stack.Web.Multilingual
{
    public class TranslationRepository : Dictionary<string, TranslationDictionary>
    {
        private static readonly object WriteFileLock = new object();
        public string TranslationsPath { get; set; }
        public TranslationDictionary DefaultDictionnary { get; set; }
        public bool AutoAddEntry { get; set; }

        public TranslationRepository(string path)
        {
            TranslationsPath = path;
            LoadXmlTranslations();
        }

        internal  string GetCurrentLanguage()
        {
            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            ;
        }

        public IEnumerable<string> GetAllKeys(string language = null)
        {
            var allKeys = new HashSet<string>();

            foreach (var dictionary in Values)
            {
                allKeys.UnionWith(dictionary.Keys);
            }
            return allKeys;
        }

        public TranslationDictionary GetDictionary(string langage)
        {
            TranslationDictionary dictionary;
            TryGetValue(langage.ToLowerInvariant(), out dictionary);
            return dictionary;
        }

        public TranslationDictionary GetOrCreateDictionary(string langage)
        {
            var lan = langage.ToLowerInvariant();
            var dictionary = GetDictionary(lan);
            if (dictionary == null)
            {
                dictionary = new TranslationDictionary(lan)
                {
                    Repository = this
                };
                Add(lan, dictionary);
            }
            return dictionary;
        }


        public IEnumerable<string> GetAvailableLanguages()
        {
            return Keys;
        }

        private void LoadXmlTranslations(string path = null)
        {
            var dinfo = new DirectoryInfo(path ?? TranslationsPath);
            if (!dinfo.Exists) return;

            foreach (var file in dinfo.GetFiles("*.xml", SearchOption.AllDirectories))
            {
                var document = XDocument.Load(file.FullName);

                if (document.Root == null || document.Root.Attribute("lang") == null)
                    continue;

                var language = document.Root.Attribute("lang").Value;
                var dictionary = GetOrCreateDictionary(language);
                var tfile = new TranslationFile()
                    {
                        FileInfo = file,
                        Language = language
                    };

                if (dictionary.DefaultXmlFile == null)
                    dictionary.DefaultXmlFile = tfile;
            
                if(!dictionary.XmlFiles.ContainsKey(file.FullName))
                    dictionary.XmlFiles.Add(file.FullName, tfile);

               
                dictionary.LoadXElement( tfile, document.Root);
            }

            TranslationDictionary defaultDictionary;
            TryGetValue(Properties.Settings.Default.DefaultLanguage, out defaultDictionary);
            DefaultDictionnary = defaultDictionary ?? Values.FirstOrDefault() ?? new TranslationDictionary(Properties.Settings.Default.DefaultLanguage);
            DefaultDictionnary.AutoAddEntry = AutoAddEntry;
        }

        /// <summary>
        /// Updates a multilingual entry.
        /// </summary>
        public void UpdateEntry(string language, string key, string value, object attributes = null)
        {
            var dictionary = GetDictionary(language);
            if (dictionary == null) return;

            dictionary.UpdateEntry(key, value, null, attributes);

        }

        public string GetCurrentTranslation(string key, params object[] args)
        {
            
            return GetTranslation(key, GetCurrentLanguage(), args);
        }

        /// <summary>
        /// Gets a multilignual string in a given culture-language.
        /// </summary>
        public string GetTranslation(string key, string language, params object[] args)
        {
            language = language.ToLowerInvariant();
            TranslationDictionary dictionary;

            if (TryGetValue(language, out dictionary))
                return dictionary.GetValue(key, args);

            if (DefaultDictionnary != null)
                return DefaultDictionnary.GetValue(key, args);

            var defaultValue = String.Format("[{0}]", key);

            return defaultValue;

        }

        /// <summary>
        /// Gets a multilignual string in the current UI culture with a pluralization.
        /// </summary>
        /// <returns></returns>
        public string GetTranslationPluralized(string key, int pluralizeCount, params object[] args)
        {
            var language = GetCurrentLanguage();
            key = key + (pluralizeCount <= 1 ? ".single" : ".many");
            return GetTranslation(key, language, args);
        }


    }
}