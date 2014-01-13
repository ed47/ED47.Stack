using System.Collections.Generic;
using System.Web.Http;
using ED47.BusinessAccessLayer.BusinessEntities;
using ED47.BusinessAccessLayer.Multilingual;
using ED47.Stack.TranslatorApi.Models;
using ED47.Stack.Web;

namespace ED47.Stack.TranslatorApi.Controllers
{
    public class TranslatorApiController : ApiController
    {
        private readonly IMultilingualRepository _multilingualRepository = MultilingualRepositoryFactory.Create();

        [HttpPost]
        public CallResult GetTranslations(MultilingualEntry dto)
        {
            var result = _multilingualRepository.GetPropertyTranslations(dto.Key, dto.PropertyName);
            return new CallResult
            {
                ResultData = new { Items = result }
            };
        }

        [HttpPost]
        public void SetTranslations(IEnumerable<Multilingual> translations)
        {
            _multilingualRepository.SaveTranslations(translations);
            _multilingualRepository.Commit();
        }

        [HttpPost]
        public void SetTranslation(Multilingual translation)
        {
            var listTemp = new List<Multilingual> { translation };
            _multilingualRepository.SaveTranslations(listTemp);
            _multilingualRepository.Commit();
        }
    }
}
