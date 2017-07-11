using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ED47.Stack.Web.Multilingual
{
    public class TranslationDictionary : Dictionary<string, ITranslationEntry>
    {
        public bool AutoAddEntry { get; set; }
        public ITranslationRepository Repository { get; set; }
        public TranslationDictionary(string language, TranslationDictionary fallback = null)
        {
            Fallback = fallback;
            Language = language;
            TranslationFiles = new Dictionary<string, TranslationFile>();
        }

        internal static readonly object WriteLock = new object();

        public TranslationFile DefaultTranslationFile { get; set; }
        public IDictionary<string, TranslationFile> TranslationFiles { get; set; }

        public string Language { get; private set; }

        public TranslationDictionary Fallback { get; set; }

        public string GetValue(string key, params object[] args)
        {
            ITranslationEntry item;
            if (TryGetValue(key, out item))
                return args.Length > 0 ? String.Format(item.Value, args) : item.Value;

            if (AutoAddEntry)
            {
                var e = AddEntry(key, String.Format("[{0}]", key));
                if (e != null) return e.Value;
                return String.Format("[{0}]", key);
            }

            if (Fallback != null && Fallback != this) return Fallback.GetValue(key, args);

            if (Multilingual.Repository.DefaultDictionnary != this)
                return Multilingual.Repository.DefaultDictionnary.GetValue(key, args);

            return "?" + key + "?";
        }

        public ITranslationEntry GetEntry(string key)
        {
            ITranslationEntry entry;
            TryGetValue(key, out entry);
            return entry;
        }

        /// <summary>
        /// Loads a translation file.
        /// </summary>
        public void LoadFile(TranslationFile file, XElement root)
        {
            LoadXElements(root.Elements(), file);
        }

        /// <summary>
        /// Recursively traverse the XDocument and add properties when a bottom element is found.
        /// </summary>
        private void LoadXElements(IEnumerable<XElement> elements, TranslationFile file, string path = "")
        {
            foreach (var element in elements)
            {
                var nextPath = String.IsNullOrWhiteSpace(path) ? element.Name.ToString() : path + "." + element.Name;
                if (element.HasElements)
                {
                    LoadXElements(element.Elements(), file, nextPath);
                }
                else
                {
                    var e = GetEntry(nextPath) ?? new TranslationEntry
                    {
                        Dictionary = this,
                        File = file,
                        Key = nextPath
                    };
                    e.Value = element.Value;
                    this[nextPath] = e;
                }
            }
        }

        public ITranslationEntry UpdateEntry(string key, string value, TranslationFile file = null, object attributes = null)
        {
            var entry = GetEntry(key);
            if (entry == null && file == null)
            {
                return AddEntry(key, value, attributes);
            }

            if (entry == null)
            {
                entry = new TranslationEntry
                {
                    Dictionary = this,
                    File = file,
                    Key = key
                };

                Add(key, entry);
            }

            entry.Update(value, attributes);
            return entry;
        }

        public void RemoveEntry(string key)
        {
            var entry = GetEntry(key);
            if (entry == null)
                return;

            entry.Delete();
        }

        public ITranslationEntry AddEntry(string key, string value, object attributes = null)
        {
            var file = GetFile(key);

            return UpdateEntry(key, value, file, attributes);
        }

        internal TranslationFile GetFile(string key)
        {
            var subkeys = key.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (subkeys.Count == 1) return DefaultTranslationFile;
            for (int i = 0; i < subkeys.Count; i++)
            {
                var pattern = String.Join(".", subkeys.Take(i + 1));
                var file = TranslationFiles.Values.Where(el => el.FileInfo.Name.ToLowerInvariant().StartsWith(pattern.ToLowerInvariant()) && el.Language.ToLowerInvariant() == Language.ToLowerInvariant())
                        .OrderBy(el => el.FileInfo.FullName.Length)
                        .FirstOrDefault();

                if (file != null)
                    return file;
            }
            return null;
        }
    }
}