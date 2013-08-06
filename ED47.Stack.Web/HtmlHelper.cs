using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ED47.Stack.Web.HelperTemplates;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ED47.Stack.Web
{
    public static class HtmlHelper
    {
        /// <summary>
        /// Renders a model in a shared store in a view.
        /// </summary>
        /// <param name="helper">The HtmlHelper.</param>
        /// <param name="model">The model to render as a shared store.</param>
        /// <param name="id">The optional ID of the store. If none is passed, model name with "-list" appended will be used.</param>
        /// <param name="name">The optional model name. If none is passed, it will be deduced from the model type.</param>
        /// <param name="addUpdateFunctionName">The name of the AJAX add/update method.</param>
        /// <param name="initNewFunctionName">The name of the JS function used to initialize a new item.</param>
        /// <param name="deleteFunctionName">The name of the JS function called when an item is deleted.</param>
        /// <param name="deleteConfirmation">Flag indicating if a confirmation should be asked when an item is deleted.</param>
        /// <param name="preselectedRecordId">The optional id of the pre-selected record.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public static MvcHtmlString RenderSharedStore(this System.Web.Mvc.HtmlHelper helper, object model,
                                                      string id = null, string name = null,
                                                      string addUpdateFunctionName = null,
                                                      string initNewFunctionName = null,
                                                      string deleteFunctionName = null, bool deleteConfirmation = true,
                                                      int? preselectedRecordId = null,
                                                      string deleteConfirmationMessage = null)
        {
// ReSharper disable JoinDeclarationAndInitializer
            Formatting formatting = Formatting.None;
// ReSharper restore JoinDeclarationAndInitializer

            if (model == null)
                return new MvcHtmlString(String.Empty);

#if DEBUG
            formatting = Formatting.Indented;
#endif
            if (String.IsNullOrWhiteSpace(name))
            {
                name = "ED47.Stack.Models.";
                var modelType = model.GetType();

                if (modelType.IsGenericType)
                    name += modelType.GetGenericArguments()[0].Name;
                else
                    name += modelType.Name.Replace("[]", String.Empty);
            }

            if (String.IsNullOrWhiteSpace(id))
                id = name.Split('.').Last().ToLowerInvariant() + "-list";

            var builder = new StringBuilder("<script language='javascript'>");
            builder.AppendLine(String.Format("ED47.views.Models['{0}'] = {1};", id,
                                             JsonConvert.SerializeObject(model, formatting,
                                                                         new JavaScriptDateTimeConverter())));
            builder.AppendLine(
                String.Format(
                    "Ext.onReady(function(){{ ED47.Stores.setup('{0}', '{1}', {2},{3},{4},{5}, {6}, {7}); }});", id,
                    name, addUpdateFunctionName ?? "null", initNewFunctionName ?? "null", deleteFunctionName ?? "null",
                    deleteConfirmation ? "true" : "false", preselectedRecordId ?? 0, deleteConfirmationMessage ?? "null"));
            builder.AppendLine("</script>");

            return new MvcHtmlString(builder.ToString());
        }

        /// <summary>
        /// Render the script used to initialize a client control in a razor view.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public static MvcHtmlString RenderControlScript(this System.Web.Mvc.HtmlHelper helper, ClientControlModel model)
        {
            var tpl = Template.Template.Get("ED47.Stack.Web.HelperTemplates.ClientControlScript.cshtml");
            if (tpl == null) return new MvcHtmlString(String.Empty);
            return new MvcHtmlString(tpl.Apply(model));
        }

        /// <summary>
        /// Render the client control in a razor view.
        /// </summary>
        /// <param name="helper">The Html helper.</param>
        /// <param name="model">The model.</param>
        /// <param name="templatePath">The optional template path.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public static MvcHtmlString RenderControl(this System.Web.Mvc.HtmlHelper helper, ClientControlModel model,
                                                  string templatePath = null)
        {
            var tpl = Template.Template.Get(templatePath ?? "ED47.Stack.Web.HelperTemplates.ClientControlView.cshtml");
            if (tpl == null) return new MvcHtmlString(String.Empty);
            return new MvcHtmlString(tpl.Apply(model));
        }

        /// <summary>
        /// Renders the page view contolr.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="model">The model.</param>
        /// <param name="templatePath">The optional template path.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Extension method")]
        public static MvcHtmlString RenderPageView(this System.Web.Mvc.HtmlHelper helper, ClientControlModel model,
                                                   string templatePath = null)
        {
            var tpl = Template.Template.Get(templatePath ?? "ED47.Stack.Web.HelperTemplates.ClientPageView.cshtml");
            if (tpl == null) return new MvcHtmlString(String.Empty);
            return new MvcHtmlString(tpl.Apply(model));
        }

        /// <summary>
        /// Create a select list from an enumeration.
        /// </summary>
        /// <param name="enumerationType">The Type of the enumeration.</param>
        /// <param name="partialKeyName">The partial i18n translation key. Each enumeration value name will be appended to it to get the select item text.</param>
        /// <param name="selectedValue">The optional selected value.</param>
        /// <returns></returns>
        public static SelectList CreateSelectListFromEnum(Type enumerationType, string partialKeyName,
                                                          object selectedValue = null)
        {
            return new SelectList(Enum.GetNames(enumerationType)
                                      .Select(el => new SelectListItem
                                          {
                                              Text = Multilingual.Multilingual.N(partialKeyName + el),
                                              Value =
                                                  ((int) Enum.Parse(enumerationType, el)).ToString(
                                                      CultureInfo.InvariantCulture)
                                          }).ToList()
                                  , "Value", "Text", selectedValue);
        }

        /// <summary>
        /// Writes a text while preserving the line returns as <br/>
        /// </summary>
        /// <param name="htmlHelper">The Razor HTML helper.</param>
        /// <param name="text">The text to preserve line returns.</param>
        public static IHtmlString PreserveNewLines(this System.Web.Mvc.HtmlHelper htmlHelper, string text)
        {
            return text == null ? null : htmlHelper.Raw(htmlHelper.Encode(text).Replace("\n", "<br/>"));
        }

        /// <summary>
        /// Adds the even/odd and/or last classes for a table style.
        /// </summary>
        /// <param name="htmlHelper">The Razor HTML helper.</param>
        /// <param name="currentCount">The current iteration count.</param>
        /// <param name="totalCount">The total number of elements.</param>
        /// <param name="evenClass">The even class</param>
        /// <param name="oddClass">The odd class</param>
        /// <param name="lastClass">The last class</param>
        public static IHtmlString AddTableLineClasses(this System.Web.Mvc.HtmlHelper htmlHelper, int currentCount, int? totalCount = null, string evenClass = "even", string oddClass = "odd", string lastClass = "last")
        {
            var classes = currentCount % 2 == 0 ? evenClass : oddClass;

            if (totalCount.HasValue && currentCount + 1 == totalCount)
            {
                if (!String.IsNullOrWhiteSpace(lastClass))
                    classes += " " + lastClass;
                else
                    classes = String.Empty;
            }

            return MvcHtmlString.Create(classes);
        }
    }
}
