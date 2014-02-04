using System.Xml.Linq;

namespace ED47.Stack.Web.Multilingual
{
    public class TranslationEntry : ITranslationEntry
    {
        public TranslationDictionary Dictionary { get; set; }
        public string Key { get; set; }
        public TranslationFile File { get; set; }
        public string Value { get; set; }
        
        public void Update(string value, object attributes = null)
        {
            Value = value;
            Save(attributes);
        }

        private void Save(object attributes = null)
        {
            var splitPath = Key.Split('.');
            lock (Dictionary.WriteLock)
            {
                var document = XDocument.Load(File.FileInfo.FullName);
                var currentElement = document.Root;
                if (currentElement == null)
                    return;

                
                foreach (var node in splitPath)
                {
                    var nextElement = currentElement.Element(node);

                    if (nextElement == null)
                    {
                        var newElement = new XElement(node);
                        if (attributes != null)
                            newElement.ReplaceAttributes(attributes);
                        currentElement.Add(newElement);
                        nextElement = newElement;
                    }

                    currentElement = nextElement;
                }
             
                var data = new XCData(Value);
                currentElement.RemoveNodes();
                currentElement.Add(data);
               
                document.Save(File.FileInfo.FullName);
                Dictionary.Repository.ClearCache(Dictionary.Language);
            }
        }
    }
}
