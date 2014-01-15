using System.Collections.Generic;
using System.Linq;

namespace ED47.BusinessAccessLayer.Multilingual
{
    public static class MultilingualRepositoryExtensions
    {
        public static void SaveTranslations(this IMultilingualRepository repository, IEnumerable<Multilingual.IMultilingual> translations)
        {
            translations.ToList().ForEach(el => el.Save());
        }
    }
}