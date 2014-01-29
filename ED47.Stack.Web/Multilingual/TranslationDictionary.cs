using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace ED47.Stack.Web.Multilingual
{
    public class TranslationDictionary : Dictionary<string, TranslationEntry>
    {
        public bool AutoAddEntry { get; set; }
        public TranslationRepository Repository { get; set; }
        public TranslationDictionary(string language, TranslationDictionary fallback = null)
        {
            Fallback = fallback;
            Language = language;
            TranslationFiles = new Dictionary<string, TranslationFile>();
        }

        internal readonly object WriteLock = new object();
        
        public TranslationFile DefaultTranslationFile { get; set; }
        public Dictionary<string, TranslationFile> TranslationFiles { get; set; }
        
        public string Language { get; private set; }

        public TranslationDictionary Fallback { get; set; }

        public string GetValue(string key, params object[] args)
        {
            TranslationEntry item;
            if (TryGetValue(key, out item))
                return args.Length > 0 ? String.Format(item.Value, args) : item.Value;

            if (AutoAddEntry)
            {
                var e = AddEntry(key, String.Format("[{0}]", key));
                if (e != null) return e.Value;
                return String.Format("?{0}?", key);
            }
            
            if(Fallback != null && Fallback != this) return Fallback.GetValue(key, args);

            return null;
        }

        public TranslationEntry GetEntry(string key)
        {
            TranslationEntry entry;
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

        public TranslationEntry UpdateEntry(string key, string value, TranslationFile file = null , object attributes = null )
        {
            var entry = GetEntry(key);
            if (entry == null && file == null)
            {
                return AddEntry(key, value, attributes);
                
            }
            if (entry == null)
            {
                entry = new TranslationEntry()
                {
                    Dictionary = this,
                    File = file,
                    Key = key
                };
                Add(key, entry);
                if (!TranslationFiles.ContainsKey(file.FileInfo.FullName)) 
                    TranslationFiles.Add(file.FileInfo.FullName, file);
            }

            entry.Update(value, attributes);
            return entry;
        }


        public TranslationEntry AddEntry(string key, string value, object attributes = null)
        {
            var file = GetFile(key);
            if (file == null)
            {
                return null;
            }
            return UpdateEntry(key, value, file, attributes);
        }

        private TranslationFile GetFile(string key)
        {
            var subkeys = key.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (subkeys.Count == 1) return DefaultTranslationFile;
            for (int i = subkeys.Count - 2; i >= 0; i--)
            {
                var pattern = String.Join(".", subkeys.Take(i));
                var file = TranslationFiles.Values.Where(el => el.FileInfo.Name.StartsWith(pattern) && el.Language == Language)
                        .OrderBy(el => el.FileInfo.FullName.Length)
                        .FirstOrDefault();
                if (file != null) return file;
            }
            return DefaultTranslationFile;
        }

       

    }
}