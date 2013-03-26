using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Ionic.Zip;

namespace ED47.Stack.Web.Multilingual
{
    public class Multilingual
    {
        private const string CacheKey = "Translations";
        private static readonly object WriteFileLock = new object();

        public const string TranslationFilesRelativePath = "/App_Data/Translations/";

        protected static IDictionary<string, IDictionary<string, TranslationItem>> Translations
        {
            get
            {
                var translations =
                    MemoryCache.Default.Get(CacheKey) as IDictionary<string, IDictionary<string, TranslationItem>>;

                if (translations == null)
                {
                    translations = LoadXmlTranslations();
                    
                    #if !DEBUG
                    MemoryCache.Default.Add(new CacheItem(CacheKey, translations),
                                            new CacheItemPolicy {Priority = CacheItemPriority.NotRemovable});
                    #endif
                    #if DEBUG
                    MemoryCache.Default.Add(new CacheItem(CacheKey, translations),
                                            new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(2) });
                    #endif
                }

                return translations;
            }
        }

        public static IDictionary<string, IDictionary<string, TranslationItem>> LoadXmlTranslations()
        {
            var languages = new Dictionary<string, IDictionary<string, TranslationItem>>();

            foreach (var file in Directory.GetFiles(HttpContext.Current.Server.MapPath("/App_Data/Translations/"), "*.xml", SearchOption.AllDirectories))
            {
                var document = XDocument.Load(file);
                if (document.Root == null)
                    continue;
                
                var language = document.Root.Attribute("lang").Value;
                IDictionary<string, TranslationItem> languageTranslation;
                if (!languages.TryGetValue(language, out languageTranslation))
                {
                    languageTranslation = new Dictionary<string, TranslationItem>();
                    languages.Add(language, languageTranslation);
                }

                LoadDocument(document, languageTranslation, new TranslationFileInfo
                    {
                        FileName = Path.GetFileName(file), 
                        Language = language
                    });
                
            }

            //Prepare fallback: inject missing keys to other languages from default language.
            var fallback = languages[Properties.Settings.Default.DefaultLanguage];

            foreach (var translationItem in fallback)
            {
                foreach (var language in languages)
                {
                    if (!language.Value.ContainsKey(translationItem.Key))
                    {
                        var filename = Path.GetFileNameWithoutExtension(translationItem.Value.FileName) + "." + language.Key + ".xml";
                        language.Value.Add(new KeyValuePair<string, TranslationItem>(

                                               translationItem.Key,
                                               new TranslationItem
                                                   {
                                                       FileName = filename,
                                                       LanguageIsoCode = language.Key,
                                                       Text = translationItem.Value.Text
                                                   }
                                               ));

                        #if DEBUG
                        Debug.WriteLine(String.Format("Missing translation key in language {0}: {1}, fallback value: {2}", language.Key, translationItem.Key, translationItem.Value.Text));
                        AddMissingKey(translationItem.Key, Path.GetFileNameWithoutExtension(filename), "###" + translationItem.Value.Text);
                        #endif
                    }
                }
            }

            return languages;
        }

        /// <summary>
        /// Gets a multilignual string in the current UI culture.
        /// </summary>
        /// <param name="path">The path of the translation.</param>
        /// <param name="args">Parameters to be passed to String.Format with the translated text.</param>
        /// <returns></returns>
        public static string N(string path, params object[] args)
        {
            var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            IDictionary<string, TranslationItem> languageTranslations;

            if (Translations.TryGetValue(language, out languageTranslations))
            {
                TranslationItem translation;
                if (languageTranslations.TryGetValue(path, out translation))
                    return args.Length > 0 ? String.Format(translation.Text, args) : translation.Text;
            }

            #if !DEBUG
            //Fallback to English
            if (Translations.TryGetValue(Properties.Settings.Default.DefaultLanguage, out languageTranslations))
            {
                TranslationItem translation;
                if (languageTranslations.TryGetValue(path, out translation))
                    return translation.Text;
            }
            #endif
            #if DEBUG
                AddMissingKey(path);
            #endif

            return String.Format("[{0}]", path);
        }


        public static string N(string path, string language, params object[] args)
        {
            IDictionary<string, TranslationItem> languageTranslations;

            if (Translations.TryGetValue(language, out languageTranslations))
            {
                TranslationItem translation;
                if (languageTranslations.TryGetValue(path, out translation))
                    return args.Length > 0 ? String.Format(translation.Text, args) : translation.Text;
            }

            #if !DEBUG
                //Fallback to English
                        if (Translations.TryGetValue(Properties.Settings.Default.DefaultLanguage, out languageTranslations))
                        {
                            TranslationItem translation;
                            if (languageTranslations.TryGetValue(path, out translation))
                                return translation.Text;
                        }
            #endif
            #if DEBUG
                 AddMissingKey(path);
            #endif

            return String.Format("[{0}]", path);
        }
     public static string N2(string path, string language, params object[] args)
        {
            IDictionary<string, TranslationItem> languageTranslations;

            if (Translations.TryGetValue(language, out languageTranslations))
            {
                TranslationItem translation;
                if (languageTranslations.TryGetValue(path, out translation))
                    return args.Length > 0 ? String.Format(translation.Text, args) : translation.Text;
            }

#if !DEBUG
    //Fallback to English
                        if (Translations.TryGetValue(Properties.Settings.Default.DefaultLanguage, out languageTranslations))
                        {
                            TranslationItem translation;
                            if (languageTranslations.TryGetValue(path, out translation))
                                return translation.Text;
                        }
#endif
#if DEBUG
            AddMissingKey(path);
#endif

            return String.Format("[{0}]", path);
        }

        /// <summary>
        /// Gets a multilignual string in the current UI culture with a plurielization.
        /// </summary>
        /// <param name="path">The path of the translation.</param>
        /// <param name="pluralizeCount">The pluralize count.</param>
        /// <param name="args">Parameters to be passed to String.Format with the translated text.</param>
        /// <returns></returns>
        public static string Np(string path, int pluralizeCount, params object[] args)
        {
            var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            IDictionary<string, TranslationItem> languageTranslations;

            if (Translations.TryGetValue(language, out languageTranslations))
            {
                TranslationItem translation;
                path = path + (pluralizeCount <= 1 ? ".single" : ".many");
                if (languageTranslations.TryGetValue(path, out translation))
                    return args.Length > 0 ? String.Format(translation.Text, args) : translation.Text;
            }

#if !DEBUG
            //Fallback to English
            if (Translations.TryGetValue(Properties.Settings.Default.DefaultLanguage, out languageTranslations))
            {
                TranslationItem translation;
                if (languageTranslations.TryGetValue(path, out translation))
                    return translation.Text;
            }
#endif
#if DEBUG
            AddMissingKey(path);
#endif

            return String.Format("[{0}]", path);
        }

        /// <summary>
        /// Gets a multilignual MvcHtmlString in the current UI culture.
        /// This is best to output HTML from the translations.
        /// </summary>
        /// <param name="path">The path of the translation.</param>
        /// <param name="args">Parameters to be passed to String.Format with the translated text.</param>
        /// <returns></returns>
        public static MvcHtmlString H(string path, params object[] args)
        {
            return MvcHtmlString.Create(N(path, args));
        }

        /// <summary>
        /// Adds a missing key to the translation XML file.
        /// </summary>
        /// <param name="path">The path to the new key.</param>
        /// <param name="filename">The optional filename.</param>
        /// <param name="value">The value to put in the missing key.</param>
        public static void AddMissingKey(string path, string filename = null, string value = null)
        {
            var splitPath = path.Split('.');
            
            if(String.IsNullOrWhiteSpace(filename))
                filename = splitPath.First();

            var matchingFiles = Directory.GetFiles(HttpContext.Current.Server.MapPath("/App_Data/Translations/"), filename + ".xml");
            var file = matchingFiles.FirstOrDefault();

            if (file == null)
                return;
            
            lock (WriteFileLock)
            {
                var document = XDocument.Load(file);
                AddMissingKeyPath(document.Root, splitPath, 0, value);
                document.Save(file);
            }
        }

        /// <summary>
        /// Adds a missing key to the translation XML file by recursively navigating the path and adding missing elements.
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <param name="path">The path to the new key.</param>
        /// <param name="currentPathItemIndex">The current path item index.</param>
        /// <param name="value">The value to add in the missing key</param>
        private static void AddMissingKeyPath(XElement parent, IList<string> path, int currentPathItemIndex, string value = null)
        {
            var currentPathItem = path[currentPathItemIndex];
            var newElement = parent.Element(currentPathItem) ?? new XElement(currentPathItem);
            if(newElement.Parent == null)
                parent.Add(newElement);

            var nextIndex = currentPathItemIndex + 1;
            if (nextIndex > path.Count() - 1)
            {
                if (!String.IsNullOrWhiteSpace(newElement.Value))
                    return;

                if (String.IsNullOrWhiteSpace(value))
                    newElement.Value = "[" + String.Join(".", path) + "]";
                else
                    newElement.Value = value;
                return;
            }

            AddMissingKeyPath(newElement, path, nextIndex, value);
        }

        /// <summary>
        /// Loads a translation file.
        /// </summary>
        /// <param name="source">The XML translation source.</param>
        /// <param name="dictionnary">The translations dictionary to load string into.</param>
        /// <param name="file">The information of the file that contains the XML translations.</param>
        private static void LoadDocument(XDocument source, IDictionary<string, TranslationItem> dictionnary, TranslationFileInfo file)
        {
            if (source == null) return;
            if (source.Root == null) return;

            XmlLoadRecursive(source.Root.Elements(), dictionnary, file);
        }

        /// <summary>
        /// Recursively traverse the XDocument and add properties when a bottom element is found.
        /// </summary>
        /// <param name="elements">The elements to explore.</param>
        /// <param name="dictionnary">The dictionnary to add translations to.</param>
        /// <param name="file">The information about the file currently being loaded.</param>
        /// <param name="path">The path to this dept.</param>
        private static void XmlLoadRecursive(IEnumerable<XElement> elements, IDictionary<string, TranslationItem> dictionnary, TranslationFileInfo file, string path = "")
        {
            foreach (var element in elements)
            {
                var nextPath = String.IsNullOrWhiteSpace(path) ? element.Name.ToString() : path + "." + element.Name;
                if (element.HasElements)
                {
                    XmlLoadRecursive(element.Elements(), dictionnary, file, nextPath);
                }
                else
                {
                    dictionnary[nextPath] = new TranslationItem
                                                {
                                                    Text = element.Value,
                                                    LanguageIsoCode = file.Language,
                                                    FileName = file.FileName
                                                };
                }
            }
        }

        /// <summary>
        /// Updates a multilingual entry.
        /// </summary>
        /// <param name="path">The path to the entry.</param>
        /// <param name="value">The value.</param>
        /// <param name="filename">The filename containing the entry.</param>
        public static void UpdateEntry(string path, string value, string filename)
        {
            var splitPath = path.Split('.');
            var matchingFiles = GetMatchingFiles(filename);
            var file = matchingFiles.FirstOrDefault();

            if (file == null)
                return;

            lock (WriteFileLock)
            {
                var document = XDocument.Load(file);
                var currentElement = document.Root;

                // ReSharper disable LoopCanBeConvertedToQuery
                foreach (var node in splitPath)
                {
                   var nextElement = currentElement.Element(node);

                   if (nextElement == null)
                   {
                       var newElement = new XElement(node);
                       currentElement.Add(newElement);
                       nextElement = newElement;
                   }

                    currentElement = nextElement;
                }
                // ReSharper restore LoopCanBeConvertedToQuery
            
                currentElement.SetValue(value);
            
                document.Save(file);
            }

            MemoryCache.Default.Remove(CacheKey);
        }

        private static IEnumerable<string> GetMatchingFiles(string filename)
        {
            var matchingFiles = Directory.GetFiles(HttpContext.Current.Server.MapPath(TranslationFilesRelativePath), filename);
            return matchingFiles;
        }

        /// <summary>
        /// Zips all the translations files and writes it to a stream.
        /// </summary>
        /// <param name="outputStream"></param>
        public static void ZipAllFiles(Stream outputStream)
        {
            using (var zip = new ZipFile())
            {
                zip.AddDirectory(HttpContext.Current.Server.MapPath(TranslationFilesRelativePath), "Translations");
                zip.Save(outputStream);
            }
        }

        /// <summary>
        /// Gets the dictionary of translation items for a specific language.
        /// </summary>
        /// <param name="language">The language to get the dictionary for.</param>
        /// <returns></returns>
        public static IDictionary<string, TranslationItem> GetLanguage(string language)
        {
            if (Translations.ContainsKey(language))
                return Translations[language];

            if (Translations.ContainsKey(Properties.Settings.Default.DefaultLanguage))
                return Translations[Properties.Settings.Default.DefaultLanguage]
                            .ToDictionary(el => el.Key, el => new TranslationItem 
                            { 
                                FileName  = Path.GetFileNameWithoutExtension(el.Value.FileName) + "." + language + ".xml", 
                                LanguageIsoCode = language, 
                                Text = el.Value.Text
                            });

            return new Dictionary<string, TranslationItem>();
        }
    }
}
