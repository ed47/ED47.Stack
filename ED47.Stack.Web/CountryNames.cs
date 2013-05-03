using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Linq;
using System.Runtime.Caching;

namespace ED47.Stack.Web
{
    public class CountryIsoNames
    {
        

        //public List<CountryData> GetAllCountries(string culture,string iso)
        //{
        //    var doc = GetResourceXMLFile(culture);
        //}

        public IEnumerable<Country> GetAllCountries(string culture)
        {
            if (string.IsNullOrEmpty(culture))
                return  new List<Country>();

            var cacheKey = "CountrieNames_" + culture;
            var memCache = MemoryCache.Default.Get(cacheKey);

            if (memCache != null)
                return memCache as IEnumerable<Country>;

            var listCountries = new List<Country>();
            var doc = GetResourceXMLFile(culture);
            
            foreach (var node in doc.Root.Descendants("country"))
            {
                listCountries.Add(new Country{Iso = node.Element("iso").Value, Name = node.Element("name").Value});
            }
            MemoryCache.Default.Add(new CacheItem(cacheKey, listCountries), new CacheItemPolicy());
            return listCountries;
        }

        public XDocument GetResourceXMLFile(string culture)
        {
            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("ED47.Stack.Web.CountriesList." + culture+".xml")) {
                return  XDocument.Load(stream);
            }
        }

        public string GetResourceTextFile(string culture) {

            var result = string.Empty;

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("ED47.Stack.Web.CountriesList." + culture+".xml")) {

                using (var sr = new StreamReader(stream)) {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
    }

    public class Country
    {
        public string Iso { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}