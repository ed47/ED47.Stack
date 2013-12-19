using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Caching;

namespace ED47.Stack.Web
{
    public static class CountryIsoNames
    {
        public static IEnumerable<Country> GetAllCountries(string culture)
        {
            if (string.IsNullOrEmpty(culture))
                return new List<Country>();

            var cacheKey = "CountryNames_" + culture;
            var memCache = MemoryCache.Default.Get(cacheKey);

            if (memCache != null)
                return memCache as IEnumerable<Country>;

            var listCountries = new List<Country>();
            var doc = GetResourceXmlFile(culture);

            foreach (var node in doc.Root.Descendants("country"))
            {
                listCountries.Add(new Country { Iso = node.Element("iso").Value, Name = node.Element("name").Value });
            }
            MemoryCache.Default.Add(new CacheItem(cacheKey, listCountries), new CacheItemPolicy());
            return listCountries;
        }

        public static XDocument GetResourceXmlFile(string culture)
        {
            using (Stream stream = typeof(CountryIsoNames).Assembly.GetManifestResourceStream("ED47.Stack.Web.CountriesList." + culture + ".xml"))
            {
                return XDocument.Load(stream);
            }
        }

        public static string GetResourceTextFile(string culture)
        {
            using (var stream = typeof(CountryIsoNames).Assembly.GetManifestResourceStream("ED47.Stack.Web.CountriesList." + culture + ".xml"))
            {
                using (var sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}