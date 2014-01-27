using System.IO;
using System.Xml.Linq;

namespace ED47.Stack.Web.Multilingual
{
    public class TranslationItem
    {
        public TranslationDictionary Dictionary { get; set; }
        public string Key { get; set; }
        public TranslationFile File { get; set; }
        public string Value { get; set; }
        
        public void Update(string value, object attributes)
        {
            Value = value;
            Save();
        }

        private void Save(object attributes = null)
        {
            var splitPath = Key.Split('.');
            lock (Dictionary.WriteLock)
            {
                var document = XDocument.Load(File.FileInfo.FullName);
                var currentElement = document.Root;
                if (document.Root == null)
                    return;

                // ReSharper disable LoopCanBeConvertedToQuery
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
                // ReSharper restore LoopCanBeConvertedToQuery
                if (currentElement != null)
                {
                    var data = new XCData(Value);
                    currentElement.RemoveNodes();
                    currentElement.Add(data);
                }
                document.Save(File.FileInfo.FullName);
            }
        }
    }
}
