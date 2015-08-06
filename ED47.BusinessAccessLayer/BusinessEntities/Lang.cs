using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ninject;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    /// <summary>
    /// Class containing all the language codes used in an application.
    /// </summary>
    public class Lang : BusinessEntity
    {
        /// <summary>
        /// The 2-letter ISO code of the language.
        /// </summary>
        [MaxLength(2)]
        public virtual string IsoCode { get; set; }

        /// <summary>
        /// The languages name.
        /// </summary>
        [MultilingualProperty]
        [MaxLength(200)]
        public virtual string Name { get; set; }


        public static IEnumerable<Lang> GetLanguages()
        {
            var context = BaseUserContext.Instance ?? BusinessComponent.Kernel.Get<BaseUserContext>();

            return context.Repository.GetAll<Entities.Language, Lang>().ToList();
        }

        private static readonly IEnumerable<Lang> Languages  = Lang.GetLanguages();
        public static bool IsAvailableLang(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return false;
            name = name.ToLowerInvariant().Split(new[] {"-"}, StringSplitOptions.RemoveEmptyEntries)[0];
            return Languages.Any(l => l.IsoCode.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}
