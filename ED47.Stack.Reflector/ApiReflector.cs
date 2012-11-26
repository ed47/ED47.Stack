using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ED47.Stack.Reflector.Attributes;
using ED47.Stack.Reflector.Metadata;
using ED47.Stack.Reflector.Templates;

namespace ED47.Stack.Reflector
{
    public static class ApiReflector
    {
        /// <summary>
        /// Loads an assembly for reflection.
        /// </summary>
        /// <param name="assemblyName">The fully qualified assembly name.</param>
        /// <returns></returns>
        static public string GenerateControllerScript(string assemblyName)
        {
            var apiControllerTypes = ApiReflector.GetApiControllerTypes(assemblyName);
            var apiControllerInfos = ApiReflector.GetControllerInfos(apiControllerTypes, ControllerKind.Api);
            var mvcControllerInfos = ApiReflector.GetControllerInfos(apiControllerTypes, ControllerKind.Mvc);

            var stringBuilder = ApiReflector.GenerateApiControllerScripts(apiControllerInfos);
            
            stringBuilder.AppendLine();
            stringBuilder.Append(ApiReflector.GenerateMvcControllerScripts(mvcControllerInfos));

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Generates the ASP.Net MVC controller scripts.
        /// </summary>
        /// <param name="mvcControllerInfos">The controller informations.</param>
        /// <returns></returns>
        private static StringBuilder GenerateMvcControllerScripts(IEnumerable<ControllerInfo> mvcControllerInfos)
        {
            var scripts = new StringBuilder();

            if (mvcControllerInfos.Any())
                scripts.AppendLine(String.Format("//ED47.Stack ASP.Net MVC Controllers for assembly {0}", mvcControllerInfos.First().ControllerType.Assembly.ToString()));

            foreach (var controller in mvcControllerInfos)
            {
                var controllerTemplate = new JsMvcController()
                {
                    ControllerInfo = controller
                };
                scripts.AppendLine('\n' + controllerTemplate.TransformText());
            }

            return scripts;
        }

        /// <summary>
        /// Generates the controller scripts.
        /// </summary>
        /// <param name="controllerInfos">The controller informations.</param>
        /// <returns></returns>
        private static StringBuilder GenerateApiControllerScripts(IEnumerable<ControllerInfo> controllerInfos)
        {
            var scripts = new StringBuilder();
            if (controllerInfos.Any())
                scripts.AppendLine(String.Format("//ED47.Stack ASP.Net Web API Controllers for assembly {0}", controllerInfos.First().ControllerType.Assembly.ToString()));

            foreach (var controller in controllerInfos)
            {
                var controllerTemplate = new JsApiController()
                                             {
                                                 ControllerInfo = controller
                                             };
                scripts.AppendLine(controllerTemplate.TransformText());
            }
            return scripts;
        }

        /// <summary>
        /// Gets the controller types from an assembly.
        /// </summary>
        /// <param name="assemblyName">The assembly's fully qualified name.</param>
        /// <returns></returns>
        private static IEnumerable<ControllerItem> GetApiControllerTypes(string assemblyName)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                return assembly
                    .GetTypes()
                    .Where(t => t.BaseType != null && (t.BaseType.FullName == "System.Web.Http.ApiController" || t.BaseType.FullName == "System.Web.Mvc.Controller"))
                    .Select(t => new ControllerItem
                                     {
                                         Kind = t.BaseType.FullName == "System.Web.Http.ApiController" ? ControllerKind.Api : ControllerKind.Mvc, 
                                         Type = t
                                     });
                
            }
            catch (System.IO.FileNotFoundException)
            {
                return new List<ControllerItem>(0);
            }
        }

        /// <summary>
        /// Gets the ControllerInfo collection from a list of types.
        /// </summary>
        /// <param name="targetTypes">The types that contain controllers.</param>
        /// <param name="kind">The kind of controller to get infos for (Web/API)</param>
        /// <returns></returns>
        private static IEnumerable<ControllerInfo> GetControllerInfos(IEnumerable<ControllerItem> targetTypes, ControllerKind kind)
        {
            var types = targetTypes.Where(t => t.Kind == kind).Select(t => t.Type);
            var controllerInfos = new List<ControllerInfo>(types.Count());

            foreach (var targetType in types)
            {
                var customAttributes = targetType.GetCustomAttributes(false);
                //Ignore if controller is marked with [SkipJavascriptGeneration]
                if (customAttributes.OfType<SkipJavascriptGenerationAttribute>().Any())
                    continue;
             
                var controllerInfo = new ControllerInfo
                                         {
                                             ControllerType = targetType,
                                             Kind = kind,
                                             UseNamespace = customAttributes.OfType<GenerateWithNamespaceAttribute>().Any(),
                                             EnableClientCaching = customAttributes.OfType<EnableClientCacheAttribute>().Any()
                                         };
                ApiReflector.GetActionInfos(controllerInfo, kind);

                if(controllerInfo.Actions.Any())
                    controllerInfos.Add(controllerInfo);
            }
            return controllerInfos;
        }

        /// <summary>
        /// Gets all the action infos from a controller info.
        /// </summary>
        /// <param name="controllerInfo">The controller info.</param>
        /// <param name="kind">The controller kind.</param>
        private static void GetActionInfos(ControllerInfo controllerInfo, ControllerKind kind)
        {
            var actions = controllerInfo.ControllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var action in actions)
            {
                var customAttributes = action.GetCustomAttributes(false);
                //Ignore if controller is marked with [SkipJavascriptGeneration]
                if (customAttributes.OfType<SkipJavascriptGenerationAttribute>().Any())
                    continue;
    
                //MVC Controller actions aren't generated by default.
                if (kind == ControllerKind.Mvc && !customAttributes.OfType<GenerateJavascriptAttribute>().Any())
                {
                    continue;
                }

                ActionInfo.GetHttpVerb(action);
                controllerInfo.Actions.Add(new ActionInfo
                                               {
                                                   Name = action.Name,
                                                   MethodInfo = action,
                                                   Controller = controllerInfo,
                                                   Verb = ActionInfo.GetHttpVerb(action),
                                                   EnableClientCaching = customAttributes.OfType<EnableClientCacheAttribute>().Any() || controllerInfo.EnableClientCaching
                                               });
            }
        }
    }
}
