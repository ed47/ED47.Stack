using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Ionic.Zip;
using Ninject;
using Ninject.Parameters;

namespace ED47.Stack.Web.Multilingual
{
    public static class Multilingual
    {
        public static readonly string ResourceFilesRelativePath = ConfigurationManager.AppSettings["ed47:ResourceFilesRelativePath"] + "Translations//";

        private static KernelBase _kernel;
        public static KernelBase Kernel
        {
            get { return _kernel ?? (_kernel = new StandardKernel()); }
        }

        public static void UpdateEntry(string language, string key, string value, object attributes = null)
        {
            Repository.UpdateEntry(language, key, value, attributes);
        }

        public static void DeleteEntry(string language, string key)
        {
            Repository.DeleteEntry(language, key);
        }

        private static ITranslationRepository _translations;
        public static ITranslationRepository Repository
        {
            get
            {
                return _translations ??
                       (_translations = Kernel.Get<ITranslationRepository>(new ConstructorArgument("path", HttpContext.Current.Server.MapPath(ResourceFilesRelativePath))));
            }
        }

        public static IEnumerable<string> GetAllKeys(string language = null)
        {
            return Repository.GetAllKeys(language);
        }

        /// <summary>
        /// Gets a multilignual string in the current UI culture.
        /// </summary>
        /// <param name="path">The path of the translation.</param>
        /// <param name="args">Parameters to be passed to String.Format with the translated text.</param>
        /// <returns></returns>
        public static string N(string path, params object[] args)
        {
            return Repository.GetCurrentTranslation(path, args);
        }



        public static string N2(string path, string language, params object[] args)
        {
            return Repository.GetTranslation(path, language, args);
        }

        /// <summary>
        /// Gets a multilignual string in the current UI culture with a pluralization.
        /// </summary>
        /// <param name="path">The path of the translation.</param>
        /// <param name="pluralizeCount">The pluralize count.</param>
        /// <param name="args">Parameters to be passed to String.Format with the translated text.</param>
        /// <returns></returns>
        public static string Np(string path, int pluralizeCount, params object[] args)
        {
            return Repository.GetTranslationPluralized(path, pluralizeCount, args);
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
        /// Zips all the translations files and writes it to a stream.
        /// </summary>
        /// <param name="outputStream"></param>
        public static void ZipAllFiles(Stream outputStream)
        {
            using (var zip = new ZipFile())
            {
                zip.AddDirectory(HttpContext.Current.Server.MapPath(ResourceFilesRelativePath), "Translations");
                zip.Save(outputStream);
            }
        }

        /// <summary>
        /// Gets the dictionary of translation items for a specific language.
        /// </summary>
        /// <param name="language">The language to get the dictionary for.</param>
        /// <returns></returns>
        public static TranslationDictionary GetLanguage(string language)
        {
            return Repository.GetDictionary(language);
        }


        public static void AddMissingKey(string key)
        {
            var dict = Repository.DefaultDictionnary;
            var e = dict.GetEntry(key);
            if (e == null)
                dict.AddEntry(key, string.Format("[{0}]", key));
        }
    }
}
