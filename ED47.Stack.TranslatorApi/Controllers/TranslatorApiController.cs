using System.Collections.Generic;
using System.Web.Http;
using ED47.BusinessAccessLayer;
using ED47.BusinessAccessLayer.BusinessEntities;
using ED47.BusinessAccessLayer.Multilingual;
using ED47.Stack.TranslatorApi.Models;
using ED47.Stack.Web;
using Ninject;

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
            var context = BaseUserContext.Instance;

            if (context == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            _multilingualRepository.SaveTranslations(translations);
            context.Commit();
        }

        [HttpPost]
        public void SetTranslation(Multilingual translation)
        {
            var context = BaseUserContext.Instance;

            if (context == null)
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            var listTemp = new List<Multilingual>() { translation };
            _multilingualRepository.SaveTranslations(listTemp);
            context.Commit();
        }
    }
}
