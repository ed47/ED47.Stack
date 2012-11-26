using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using RazorEngine;

namespace ED47.Stack.Web
{
    public delegate object TemplateFuncDelegate(object[] parameters);
    [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    public class Template
    {
        public enum TemplateType
        {
            XTemplate = 1,
            Razor = 2
        }

        private static readonly Dictionary<string, Type> Types = new Dictionary<string, Type>();
        private const string RegularExpressionFunction = @"\[([\w\.]+)\(([^\]]*)\)\]";

        private const string RegularExpressionTemplate = "<(tpl[0-9]*)\\s+(for|if)=\"([^\"]*)?\"([^>]*)>((.|\\s)*?)</\\1>";
        //"<(tpl[0-9]*)\\s*(for=\"([^\"]+))?\\b[^>]*>(.*?)</\\1>";

        private const string RegField = @"\{\[?([\w\-\(\)\,\.\$'_:?%#+*-/<>=]+)\]?\}";
        private const string RegTemplateFunctions = @"<script>\s*tplFunctions\s*=\s*{\s*((.|\s)*)}\s*</script>";
        private const string RegScriptTemplateFunctions = @"(<script>\s*tplFunctions\s*=\s*{\s*((.|\s)*)}\s*</script>)";

        private static readonly Dictionary<string, TemplateFuncDelegate> StdFunctions =
            new Dictionary<string, TemplateFuncDelegate>();

        public static readonly Dictionary<string, string> Templates = new Dictionary<string, string>();

        private static readonly Regex RegCleanTpl = new Regex(@"<(/?)tpl[\d]+(>)?");
        private readonly Evaluator _evaluator = new Evaluator();
        private readonly Dictionary<string, TemplateFuncDelegate> _functions = new Dictionary<string, TemplateFuncDelegate>();
        private List<TemplateOccurence> _occurrences;
        private readonly List<StackItem> _stack = new List<StackItem>();
        private string _templateText = "";

        public TemplateType TplType { get; set; }
        
        static Template()
        {
            AddStdFunction("isNullOrEmpty", IsNullOrEmpty);
            AddStdFunction("isNotNullOrEmpty", IsNotNullOrEmpty);
            AddStdFunction("isNotNull", IsNotNull);
            AddStdFunction("isNull", IsNull);
            AddStdFunction("isEqual", IsEqual);
            AddStdFunction("isNotEqual", IsNotEqual);
            AddStdFunction("format", Format);
            AddStdFunction("copy", Copy);
            AddStdFunction("injectXTemplate", _InjectXTemplate);
            AddStdFunction("toLower", ToLower);
            AddStdFunction("ellipsis", Ellipsis);
        }

        public Template()
        {
            _evaluator.Tpl = this;
            AddFunction("eval", Eval);
            AddFunction("sum", _Sum);
            AddFunction("avg", _Avg);
            AddFunction("cur", Currency);
            AddFunction("inject", Inject);
        }

        public Template(string tplText)
            : this()
        {
            TemplateText = tplText;
        }

        /// <summary>
        /// The template's full filename.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Dynamically inject this content a the begining of each tpl item
        /// </summary>
        public string PreTplContent { get; set; }

        public string RootPath { get; set; }

        /// <summary>
        /// Dynamically inject this content a the end of each tpl item
        /// </summary>
        public string PostTplContent { get; set; }

        /// <summary>
        /// Dynamically inject this content a the begining of each of tpl
        /// </summary>
        public string PreContainerContent { get; set; }


        /// <summary>
        /// Dynamically inject this content a the end of each of tpl
        /// </summary>
        public string PostContainerContent { get; set; }

        /// <summary>
        /// For debug proposes, to identify the object type passse in params
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetThis(object obj)
        {
            if (obj == null)
                return "NULL";

            return obj.ToString();
        }

        public object Current
        {
            get
            {
                if (_stack.Count > 0)
                    return CurrentStackItem.Current;
                return null;
            }
        }

        internal StackItem CurrentStackItem
        {
            get
            {
                if (_stack.Count > 0)
                    return _stack[_stack.Count - 1];
                return null;
            }
        }

        public string TemplateText
        {
            get { return _templateText; }
            set { _templateText = value; }
        }
        
        public List<TemplateOccurence> Occurences
        {
            get { return _occurrences ?? (_occurrences = Compile(null, TemplateText).ToList()); }
            set { _occurrences = value; }
        }

        /// <summary>
        /// Gets a template from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the template.</param>
        /// <param name="name">The template's name.</param>
        /// <returns></returns>
        public static Template Get(Stream stream, string name)
        {
            var templateType = TemplateType.XTemplate;
            name = name.ToLowerInvariant();

            if (Path.GetExtension(name) == ".cshtml")
                templateType = TemplateType.Razor;

            return new Template(new StreamReader(stream).ReadToEnd()) { Name = name, TplType = templateType };
        }

        /// <summary>
        /// Gets the specified template by name or path.
        /// </summary>
        /// <param name="name">The name or path of the template.</param>
        /// <param name="assembly">The optional assembly to find the template in.</param>
        /// <returns></returns>
        public static Template Get(string name, Assembly assembly = null )
        {
            Template tpl = null;
            var originalName = name;
            name = name.ToLowerInvariant();
            
            if (Templates.ContainsKey(name) && File.Exists(Templates[name]))
            {
                tpl = new Template(File.ReadAllText(Templates[name]))
                          {Name = Templates[name], TplType = TemplateType.XTemplate};

                var ext = Path.GetExtension(Templates[name]);
                if (ext == ".cshtml")
                {
                    tpl.TplType = TemplateType.Razor;
                }
                return tpl;
            }
            if (File.Exists(name))
            {
                TemplateType templateType = TemplateType.XTemplate;
                if (Path.GetExtension(name) == ".cshtml")
                {
                    templateType = TemplateType.Razor;
                }

                return new Template(File.ReadAllText(name)) { Name = name, TplType = templateType };
            }

            using (var templateStream = (assembly ?? typeof(Template).Assembly).GetManifestResourceStream(originalName))
            {
                if (templateStream != null)
                {
                    var reader = new StreamReader(templateStream);
                    tpl = new Template(reader.ReadToEnd()) { Name = name, TplType = TemplateType.XTemplate };
                    var ext = Path.GetExtension(name);
                    if (ext == ".cshtml")
                    {
                        tpl.TplType = TemplateType.Razor;
                    }
                    return tpl;
                }
            }

            return null;
        }

        public static Dictionary<string, string> GetRegisteredTemplates()
        {
            return Templates;
        }

        public static void Register(string tplName, string filename)
        {
            tplName = tplName.ToLowerInvariant();
                
            if (Templates.ContainsKey(tplName))
            {
                Templates[tplName] = filename;
                return;
            }
            Templates.Add(tplName, filename);
        }

        public static void RegisterType(Type type)
        {
            var name = type.Name.ToLowerInvariant();
            if (Types.ContainsKey(name))
            {
                Types[name] = type;
                return;
            }
            Types.Add(name, type);
        }

        public static void RegisterDirectory(string path, string searchPattern)
        {
            if (!Directory.Exists(path))
                return;


            foreach (var filename in Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories))
            {
                var tplName = Path.GetFileNameWithoutExtension(filename);

                var shortpath = filename.Substring(path.Length);
                shortpath = shortpath.Substring(0, shortpath.Length - Path.GetFileName(filename).Length);

                shortpath = shortpath.Replace("\\", "_");

                tplName = shortpath + tplName;


                Register(tplName, filename);
            }
        }


        public void AddFunction(string name, TemplateFuncDelegate func)
        {
            _functions.Add(name, func);
        }

        public static void AddStdFunction(string name, TemplateFuncDelegate func)
        {
            StdFunctions.Add(name, func);
        }


        private static string Currency(object[] param)
        {
            double value;
            return Double.TryParse(param[0].ToString(), out value) ? String.Format("{0:n}", value) : param[0].ToString();
        }


        private void InitChildTemplate(Template t)
        {
            foreach (var obj in _stack)
            {
                t._stack.Add(obj);
            }

            foreach (var key in _functions.Keys.Where(key => !t._functions.ContainsKey(key)))
            {
                t.AddFunction(key, _functions[key]);
            }
        }


        public static double Sum(Template tpl, object data, string path, string field)
        {
            var field1 = path;
            var field2 = field;

            var coll = new List<object>();
            FindChildren(tpl, data, field1, coll);
            double sum = 0;

            for (int index = 0; index < coll.Count; index++)
            {
                var obj = coll[index];
                double value = 0;
                var tmp = GetValue(tpl, obj, field2);
                var svalue = tmp != null ? tmp.ToString() : "0";
                if (Double.TryParse(svalue, out value))
                {
                    sum += value;
                }
            }
            return sum;
        }


        public static double Avg(Template tpl, object data, string path, string field)
        {
            var field1 = path;
            var field2 = field;

            var coll = new List<object>();
            FindChildren(tpl, data, field1, coll);
            double sum = 0;

            foreach (var obj in coll)
            {
                double value = 0;
                var tmp = GetValue(tpl, obj, field2);
                var svalue = tmp != null ? tmp.ToString() : "0";
                if (Double.TryParse(svalue, out value))
                {
                    sum += value;
                }
            }
            return sum / coll.Count;
        }

        public static void FindChildren(Template tpl, object o, string path, List<object> result)
        {
            var i = path.IndexOf(".");
            if (i > 0)
            {
                var field = path.Substring(0, i);
                var child = GetPropertyValue(tpl, o, field);
                if (child is IEnumerable)
                {
                    var col = child as IEnumerable;
                    foreach (var c in col)
                    {
                        FindChildren(tpl, c, path.Substring(i + 1), result);
                    }
                }
                else
                {
                    FindChildren(tpl, child, path.Substring(i + 1), result);
                }
            }
            else
            {
                var child = GetPropertyValue(tpl, o, path);
                if (child is IEnumerable)
                {
                    var col = child as IEnumerable;
                    result.AddRange(col.Cast<object>());
                }
                else
                {
                    result.Add(child);
                }
            }
        }


        private string Eval(object[] args)
        {
            _evaluator.Evaluate(args[0].ToString());
            return _evaluator.Result().value.ToString();
        }

        private string _Sum(object[] args)
        {
            double res = 0;
            if (args.Length >= 2)
            {
                res = Sum(this, Current, args[0].ToString(), args[1].ToString());
            }
            return args.Length >= 3 ? String.Format("{" + args[2] + "}", res) : res.ToString(CultureInfo.InvariantCulture);
        }

        private static string Ellipsis(object[] args)
        {
            if (args.Length != 2)
            {
                return "Fct ellispis need 2 arguments";
            }

            var txt = args[0] != null ? args[0].ToString() : String.Empty;
            var length = Convert.ToInt32(args[1],CultureInfo.InvariantCulture);
            if (txt.Length <= length)
                return txt;
            return txt.Substring(0, length) + "...";
        }

        private string _Avg(object[] args)
        {
            double res = 0;
            if (args.Length >= 2)
            {
                res = Avg(this, Current, args[0].ToString(), args[1].ToString());
            }
            return args.Length >= 3 ? String.Format("{" + args[2] + "}", res) : res.ToString(CultureInfo.InvariantCulture);
        }

        private static string Copy(object[] args)
        {
            if (args.Length == 1)
            {
                return Get(args[0].ToString())._templateText;
            }
            return "";
        }

        private static string CleanTpl(Match m)
        {
            return "<" + m.Groups[1] + "tpl" + m.Groups[2];
        }

        public static string InjectXTemplate(string tplName)
        {
            return _InjectXTemplate(new object[] { tplName });
        }

        private static string _InjectXTemplate(object[] args)
        {
            if (args.Length == 1)
            {
                var tpl = Get(args[0].ToString());
                if (tpl != null)
                {
                    var tplTxt = tpl.TemplateText;
                    tplTxt = RegCleanTpl.Replace(tplTxt, CleanTpl);
                    var tplFunctions = tpl.GetTplFunctions();
                    if (!String.IsNullOrEmpty(tplFunctions))
                    {
                        tplTxt = tplTxt.Replace(tpl.GetScriptTplFunctions(), string.Empty);

                        return "ED47.Stack.Template._Templates['" + args[0] + "'] = new Ext.XTemplate('" +
                               tplTxt.Replace("'", @"\'").Replace("\n", string.Empty).Replace("\r", string.Empty).
                                   Replace("\t", string.Empty) + "', {\r\n" + tplFunctions + "});";
                    }
                    return "ED47.Stack.Template._Templates['" + args[0] + "'] = new Ext.XTemplate('" +
                           tplTxt.Replace("'", @"\'").Replace("\n", string.Empty).Replace("\r", string.Empty).
                               Replace("\t", string.Empty) + "');";
                }
                return "// missing template " + args[0];
            }
            return "// missing template name argument";
        }

        private static string ToLower(object[] args)
        {
            return args.Length != 1 ? "// missing text argument" : args[0].ToString().ToLower(CultureInfo.InvariantCulture);
        }

        private static string Format(object[] args)
        {
            return args.Length == 2 ? String.Format("{" + args[1] + "}", args[0]) : "";
        }

        private static string IsNotNull(object[] args)
        {
            if (args.Length == 0)
                return "false";

            if (args[0] == null)
                return "false";
            return Convert.ToString(args[0],CultureInfo.InvariantCulture) == "" ? "false" : "true";
        }

        private static string IsNotNullOrEmpty(object[] args)
        {
            if (args.Length == 0)
                return "false";

            if (args[0] == null)
                return "false";

            return string.IsNullOrEmpty(args[0].ToString()) ? "false" : "true";
        }

        private static string IsNullOrEmpty(object[] args)
        {
            if (args.Length == 0)
                return "false";

            if (args[0] == null)
                return "false";

            return string.IsNullOrEmpty(args[0].ToString()) ? "true" : "false";
        }

        private static string IsNull(object[] args)
        {
            if (args.Length == 0)
                return "true";

            if (args[0] == null)
                return "true";
            return Convert.ToString(args[0],CultureInfo.InvariantCulture) == "" ? "true" : "false";
        }

        private static string IsEqual(object[] args)
        {
            if (args.Length != 2)
                return "false";

            if (args[0] is string || args[1] is string)
            {
                return args[0].ToString() == args[1].ToString() ? "true" : "false";
            }

            return args[0] == args[1] ? "true" : "false";
        }

        private static string IsNotEqual(object[] args)
        {
            if (args.Length != 2)
                return "false";

            if (args[0] is string || args[1] is string)
            {
                return args[0].ToString() == args[1].ToString() ? "false" : "true";
            }


            return args[0] == args[1] ? "false" : "true";
        }


        private string Inject(object[] args)
        {
            var tplName = args[0].ToString();
            if (args.Length == 1)
            {
                if (Templates.ContainsKey(tplName) && File.Exists(Templates[tplName]))
                {
                    var t = new Template(File.ReadAllText(Templates[tplName]));

                    InitChildTemplate(t);
                    return t.Apply(Current);
                }
            }
            if (args.Length == 2)
            {
                var target = args[1];

                if (Templates.ContainsKey(tplName) && File.Exists(Templates[tplName]))
                {
                    var t = new Template(File.ReadAllText(Templates[tplName]));
                    InitChildTemplate(t);
                    return t.Apply(target);
                }
            }
            return "";
        }

        public string GetTplFunctions()
        {
            var mtplfcts = Regex.Match(TemplateText, RegTemplateFunctions);

            if (mtplfcts.Success && mtplfcts.Groups.Count > 1)
                return mtplfcts.Groups[1].Value;
            return "";
        }

        public string GetScriptTplFunctions()
        {
            var mscript = Regex.Match(TemplateText, RegScriptTemplateFunctions);

            if (mscript.Success && mscript.Groups.Count > 1)
                return mscript.Groups[0].Value;
            return "";
        }

        private static object GetPropertyValue(Template t, object obj, string property)
        {
            if (obj is JsonObject && ((JsonObject)obj).HasProperty(property))
            {
                var res = ((JsonObject)obj)[property];
                if (res != null)
                    return res;
            }

            var pinfo = obj != null ? obj.GetType().GetProperty(property) : null;
            if (pinfo != null)
            {
                return pinfo.GetValue(obj, null);
            }
            if (property == "this" || property == "values")
            {
                return obj;
            }

            if (property == "_path")
            {
                if (t._stack.Count == 0)
                    return "";

                return (t.RootPath != "" ? t.RootPath : "") +
                       String.Join("/", t._stack.Take(t._stack.Count - 1).Select(s => s.Name).ToArray());
            }

            if (property == "xindex")
            {
                if (t._stack.Count == 0) return -1;
                return t._stack[t._stack.Count - 1].Index;
            }
            if (property == "_parent")
            {
                //TODO Faire la recherche du parent plus propement
                var i = t._stack.Select(item => item.Current).ToList().IndexOf(obj);
                if (i > 1)
                    return t._stack[i - 1].Current;
            }

            return Types.ContainsKey(property) ? Types[property] : null;
        }

        /*
                private object GetPropertyValue(object obj, string property)
                {
                    return GetPropertyValue(this, obj, property);
                }
        */

        internal static object GetValue(Template t, object obj, string ident)
        {
            if (ident == "this" || ident == "." || ident == "values")
                return obj;

            if (ident == "_path")
            {
                if (t._stack.Count == 0)
                    return "";

                return (t.RootPath != "" ? t.RootPath : "")
                       + String.Join("/", t._stack.Take(t._stack.Count - 1).Select(s => s.Name).ToArray());
            }

            var path = ident.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (path.Length == 0)
                return obj;

            var index = 0;
            var child = GetPropertyValue(t, obj, path[0]);
            index++;
            var scope = child;
            while (index < path.Length)
            {
                if (child != null)
                {
                    child = GetPropertyValue(t, child, path[index]);
                    scope = child ?? scope;
                }
                else
                {
                    break;
                }
                index++;
            }
            var lastIdent = path[path.Length - 1];
            if (child == null)
            {
                switch (lastIdent)
                {
                    case "length":
                        {
                            var newPath = ident.Substring(0, ident.Length - 7);
                            var res = new List<object>();
                            FindChildren(t, obj, newPath, res);
                            return res.Count;
                        }
                    case "isNull":
                        {
                            return obj == null ? "true" : "false";
                        }
                    //case "isNullOrEmpty":
                    //    {
                    //        string prop = ident.Replace(".isNullOrEmpty", "").Replace("values.", "");
                    //        var val = ((JsonObject) obj)[prop].ToString();

                    //        return string.IsNullOrEmpty(val)  ? "true" : "false";
                    //    }
                }
            }
            //Match mfunc = new Regex(_RegSimpleFunc).Match(lastIdent);
            if (scope != null && child == null)
            {
                if (scope is Type)
                {
                    var mf = ((Type)scope).GetMethod(lastIdent);
                    if (mf != null)
                    {
                        var mc = new MethodCall
                                     {
                                         Fct =
                                             Delegate.CreateDelegate(typeof (TemplateFuncDelegate), mf) as
                                             TemplateFuncDelegate,
                                         Scope = null
                                     };
                        return mc;
                    }
                }
                else
                {
                    var scopeType = scope.GetType();
                    var mf = scopeType.GetMethod(lastIdent);
                    if (mf != null)
                    {
                        var mc = new MethodCall
                                     {
                                         Fct =
                                             Delegate.CreateDelegate(typeof (TemplateFuncDelegate), scope, lastIdent) as
                                             TemplateFuncDelegate
                                     };
                        return mc;
                    }
                }
            }


            return child;
        }

        private object GetValue(object obj, string ident)
        {
            return GetValue(this, obj, ident);
        }


        private object ExecuteObjectFunction(Match m)
        {
            var fname = m.Groups[1].Value;
            var paramsString = m.Groups[2].Value;

            var _params = paramsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var paramsObject = new object[_params.Length];

            for (var i = 0; i < _params.Length; i++)
            {
                if (_params[i] == "this")
                {
                    paramsObject[i] = Current;
                    continue;
                }
                if (_params[i].Substring(0, 1) == "'")
                {
                    paramsObject[i] = _params[i].Substring(1, _params[i].Length - 2);
                    continue;
                }
                int iv;
                if (Int32.TryParse(_params[i], out iv))
                {
                    paramsObject[i] = iv;
                    continue;
                }
                double dv;
                if (Double.TryParse(_params[i], out dv))
                {
                    paramsObject[i] = dv;
                    continue;
                }
                paramsObject[i] = GetValue(Current, _params[i]);
            }

            TemplateFuncDelegate f = null;
            _functions.TryGetValue(fname, out f);

            if (f == null)
                StdFunctions.TryGetValue(fname, out f);

            if (f != null)
                return f(paramsObject);

            var mc = GetValue(_stack.Count > 0 ? _stack[_stack.Count - 1].Current : null, fname) as MethodCall;
            return mc != null ? mc.Fct(paramsObject) : null;
        }

        private string ExecuteFunction(Match m)
        {
            var res = ExecuteObjectFunction(m);
            return res != null ? res.ToString() : "";
        }

        private string Evaluator(Match m)
        {
            if (m.Groups.Count == 0)
                return "";

            var ident = m.Value.Substring(1, m.Value.Length - 2);

            var fm = Regex.Match(ident, RegularExpressionFunction);
            if (fm.Success)
            {
                return ExecuteFunction(fm);
            }

            var value = GetValue(Current, ident);
            if (value is DateTime)
            {
                value = ((DateTime)value).ToString("dd.MM.yyyy");
            }

            return value != null ? value.ToString() : "";
        }


        private string ApplyObject(string fragment, object o, int stackIndex)
        {
            _stack.Add(new StackItem { Current = o, Index = stackIndex });

            var tpls = Regex.Matches(fragment, RegularExpressionTemplate);

            var res = new StringBuilder();
            var index = 0;
            if (tpls.Count > 0)
            {
                foreach (Match m in tpls)
                {
                    res.Append(fragment.Substring(index, m.Index - index));

                    index = m.Length + m.Index;

                    var tplName = m.Groups[3].Value;
                    var tplType = m.Groups[2].Value;
                    var content = m.Groups[5].Value;

                    if (tplType == "if")
                    {
                        var fm = Regex.Match(tplName, RegularExpressionFunction);
                        if (fm.Success)
                        {
                            if (ExecuteFunction(fm).ToLower() == "true")
                            {
                                res.Append(ApplyObject(content, o, stackIndex));
                            }
                        }
                        else
                        {
                            var cond = Eval(new object[] { tplName }); //GetValue(o, tplName);
                            if (cond != null && cond.ToLower() == "true")
                            {
                                res.Append(ApplyObject(content, o, stackIndex));
                            }
                        }
                        continue;
                    }

                    content = PreTplContent + content + PostTplContent;
                    if (tplName == "." || tplName == "")
                    {
                        if (CurrentStackItem != null)
                            CurrentStackItem.Name = ".";
                        res.Append(ApplyObject(content, o, 0));
                    }
                    else
                    {
                        object target;
                        var fm = Regex.Match(tplName, RegularExpressionFunction);
                        if (fm.Success)
                        {
                            target = ExecuteObjectFunction(fm);
                        }
                        else
                        {
                            CurrentStackItem.Name = tplName;
                            target = GetValue(o, tplName);
                        }
                        if (target != null)
                        {
                            if (target is IEnumerable)
                            {
                                var list = target as IEnumerable;
                                var i = 0;
                                var count = list.Cast<object>().Count();

                                foreach (var o2 in list)
                                {
                                    res.Append(
                                        ApplyObject(
                                            (i == 0 ? PreContainerContent : "") + content +
                                            (i == count - 1 ? PostContainerContent : ""), o2, i));
                                    i++;
                                }
                            }
                            else
                            {
                                res.Append(ApplyObject(PreContainerContent + content + PostContainerContent, target, 0));
                            }
                        }
                        else
                        {
                            res.Append(ApplyObject(PreContainerContent + PostContainerContent, new JsonObject(), 0));
                        }
                    }
                }
                res.Append(fragment.Substring(index));
            }
            else
            {
                res.Append(fragment);
            }

            var res2 = Regex.Replace(res.ToString(), RegField, Evaluator);
            _stack.RemoveAt(_stack.Count - 1);

            return res2;
        }


        private string ApplyData(string fragment, IEnumerable data, int stackIndex)
        {
            var res = new StringBuilder();
            var startIndex = fragment.IndexOf("<tpl");
            var start = startIndex > 0 ? fragment.Substring(0, startIndex) : "";
            var endIndex = fragment.LastIndexOf("</tpl");
            while (endIndex > 0 && fragment[endIndex] != '>')
                endIndex++;
            var end = endIndex > 0 ? fragment.Substring(endIndex + 1) : "";

            var tpls = Regex.Matches(fragment, RegularExpressionTemplate);


            /* foreach (object o in data){ dataCount++; break;};

            if (dataCount == 0 && (PreContainerContent!="" || PostContainerContent != ""))
            {
                var tmp = new JsonObject();
                res.Append(ApplyObject(PreContainerContent+PostContainerContent, tmp, 0));
            }*/

            foreach (var o in data)
            {
                if (tpls.Count > 0)
                {
                    foreach (Match m in tpls)
                    {
                        var tplName = m.Groups[3].Value;
                        var tplType = m.Groups[2].Value;
                        var content = m.Groups[5].Value;
                        if (tplType == "if")
                        {
                            var fm = Regex.Match(tplName, RegularExpressionFunction);
                            if (fm.Success)
                            {
                                if (ExecuteFunction(fm).ToLower() == "true")
                                {
                                    res.Append(ApplyObject(content, o, stackIndex));
                                }
                            }
                            else
                            {
                                var cond = Eval(new object[] { tplName }); //GetValue(o, tplName);
                                if (cond != null && cond.ToLower() == "true")
                                {
                                    res.Append(ApplyObject(content, o, stackIndex));
                                }
                            }
                            continue;
                        }
                        content = PreTplContent + content + PostTplContent;
                        if (tplName == "." || tplName == "")
                        {
                            if (CurrentStackItem != null)
                                CurrentStackItem.Name = ".";
                            res.Append(ApplyObject(content, o, 0));
                        }
                        else
                        {
                            object target;
                            var fm = Regex.Match(tplName, RegularExpressionFunction);
                            if (fm.Success)
                            {
                                target = ExecuteObjectFunction(fm);
                            }
                            else
                            {
                                target = GetValue(o, tplName);
                                if (CurrentStackItem != null)
                                    CurrentStackItem.Name = tplName;
                            }
                            if (target != null)
                            {
                                if (target is IEnumerable)
                                {
                                    var list = target as IEnumerable;
                                    var i = 0;
                                    var count = list.Cast<object>().Count();
                                    foreach (var o2 in list)
                                    {
                                        res.Append(
                                            ApplyObject(
                                                (i == 0 ? PreContainerContent : "") + content +
                                                (i == count - 1 ? PostContainerContent : ""), o2, i));
                                        //res.Append(this.ApplyObject(content, o2, i));
                                        i++;
                                    }
                                }
                                else
                                {
                                    res.Append(ApplyObject(PreContainerContent + content + PostContainerContent, target,
                                                           0));
                                    // res.Append(this.ApplyObject(content, target, 0));
                                }
                            }
                            else
                            {
                                res.Append(ApplyObject(PreContainerContent + PostContainerContent, new JsonObject(), 0));
                            }
                        }
                    }
                }
                else
                {
                    res.Append(ApplyObject(fragment, o, 0));
                }
            }
            return start + res + end;
        }
        
        public string Apply(object o)
        {
            if (TplType == TemplateType.Razor)
            {
                return Razor.Parse(TemplateText, o, this.Name);
            }

            IEnumerable list;
            if (!(o is IEnumerable))
            {
                var list2 = new List<object> {o};
                list = list2;
            }
            else
                list = o as IEnumerable;

            if (TemplateText == "")
                return PreTplContent + PostTplContent;

            var res = new StringBuilder();

            res.Append(ApplyData(_templateText, list, 0));
            
            return res.ToString();
        }

        public void Apply(object o, Stream s)
        {
            var w = new StreamWriter(s);
            w.Write(Apply(o));
        }

        public IEnumerable<TemplateOccurence> Compile()
        {
            return Occurences;
        }

        /// <summary>
        /// Make a recursive search to list all the occurences of the template.
        /// </summary>
        /// <returns>A array of template occurences</returns>
        public TemplateOccurence[] GetAllOccurences()
        {
            var occurences = new List<TemplateOccurence>();
            foreach (var o in Occurences)
            {
                o.GetAllOccurences(occurences);
            }
            return occurences.ToArray();
        }

        public TemplateOccurence Select(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return Occurences.FirstOrDefault();

            var occurences = Occurences;
            var path = pattern.Split(new[] { '/' });

            TemplateOccurence occ = null;

            foreach (var t in path)
            {
                var t1 = t;
                occ = occurences.FirstOrDefault(o => o.Name == t1);
                if (occ == null)
                    return null;
                occurences = occ.Occurences.ToList();
            }


            return occ;
        }

        private IEnumerable<TemplateOccurence> Compile(TemplateOccurence parent, string content)
        {
            var occurences = new List<TemplateOccurence>();
            var tpls = Regex.Matches(content, RegularExpressionTemplate);

            foreach (Match m in tpls)
            {
                var tplType = m.Groups[2].Value;
                if (tplType != "for") continue;
                var o = CompileOccurence(parent, m);

                var i = 0;
                var fragment = "";
                foreach (var to in o.Occurences)
                {
                    fragment += o.TplText.Substring(i, (to.Index) - i);
                    i = to.Index + to.Length;
                }
                if (i < o.TplText.Length)
                {
                    fragment += o.TplText.Substring(i, o.TplText.Length - i);
                }
                o.Fragment = fragment;

                var matches = Regex.Matches(fragment, RegField);
                var exprs = (from Match mexpr in matches select new TemplateExpression { Name = mexpr.Groups[0].Value }).ToList();

                o.Expressions = exprs;
                occurences.Add(o);
            }

            return occurences;
        }

        private TemplateOccurence CompileOccurence(TemplateOccurence parent, Match m)
        {
            var tplContent = m.Groups[5].Value;
            var tplAttributes = m.Groups[4].Value;
            var tplName = m.Groups[3].Value;
            var tpl = new TemplateOccurence
            {
                Name = tplName,
                TplText = tplContent,
                Index = m.Index,
                Path = (parent != null ? parent.Path + "/" : "") + tplName,
                Length = m.Length
            };
            tpl.Occurences = Compile(tpl, tplContent);

            var regAttr = new Regex("([\\w_]*)=\"([^\"]*)\"");
            foreach (var a in from Match match in regAttr.Matches(tplAttributes)
                              select new TemplateAttribute
                              {
                                  Name = match.Groups[1].Value.ToLower(),
                                  Value = match.Groups[2].Value.ToLower()
                              })
            {
                tpl.Attributes.Add(a.Name, a);
            }
            return tpl;
        }

        #region Nested type: MethodCall

        internal class MethodCall
        {
            internal TemplateFuncDelegate Fct;
            internal object Scope;
        }

        #endregion

        #region Nested type: StackItem

        internal class StackItem
        {
            internal string Name { get; set; }
            internal object Current { get; set; }
            internal int Index { get; set; }
        }

        #endregion

        #region Nested type: TemplateAttribute

        public class TemplateAttribute
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        #endregion

        #region Nested type: TemplateExpression

        public class TemplateExpression
        {
            public string Name { get; set; }
        }

        #endregion

        #region Nested type: TemplateOccurence

        public class TemplateOccurence
        {
            public Dictionary<string, TemplateAttribute> Attributes = new Dictionary<string, TemplateAttribute>();
            public String Name { get; set; }
            public TemplateOccurence parent { get; set; }
            public String Fragment { get; set; }
            public String TplText { get; set; }
            public int Index { get; set; }
            public int Length { get; set; }
            public IEnumerable<TemplateOccurence> Occurences { get; set; }
            public IEnumerable<TemplateExpression> Expressions { get; set; }
            public String Path { get; set; }

            public Template ToTemplate()
            {
                var tpl = new Template("<tpl for=\".\">" + TplText + "</tpl>") { RootPath = Path };
                return tpl;
            }

            /// <summary>
            /// Make a recursive search to list all the children occurences and populate the list in parameter
            /// </summary>
            /// <returns>A array of template occurences</returns>
            public void GetAllOccurences(List<TemplateOccurence> list)
            {
                list.Add(this);

                foreach (var o in Occurences)
                    o.GetAllOccurences(list);
            }
        }

        #endregion
    }
}