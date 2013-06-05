using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ED47.Stack.Web
{

    [Serializable]
    [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    public class JsonObject : ISerializable
    {
      

        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();
        private JsonObjectSerialization _serializationType = JsonObjectSerialization.DataOnly;

        private readonly object _source;
        private string _typeName = "";

        public JsonObject()
        {
        }

        public JsonObject(string json)
        {
            if (String.IsNullOrWhiteSpace(json)) return;

            JObject obj = JsonConvert.DeserializeObject(json) as JObject;

            foreach (var property in obj.Properties())
            {
                AddProperty(property.Name, property.Value);
            }
        }

        /// <summary>
        /// Creates a JsonObject from a XDocument.
        /// </summary>
        /// <param name="source">The XDocument to build the JsonObject from.</param>
        public JsonObject(XDocument source)
        {
            if (source == null) return;
            if (source.Root == null) return;
            
            XmlAddRecursive(source.Root.Elements());
        }

        /// <summary>
        /// Recursively traverse the XDocument and add properties when a bottom element is found.
        /// </summary>
        /// <param name="elements">The elements to explore.</param>
        /// <param name="path">The path to this dept.</param>
        private void XmlAddRecursive(IEnumerable<XElement> elements, string path = "")
        {
            foreach (var element in elements)
            {
                var nextPath = String.IsNullOrWhiteSpace(path) ? element.Name.ToString() : path + "." + element.Name;
                if (element.HasElements)
                {
                    XmlAddRecursive(element.Elements(), nextPath);
                }
                else
                {
                    AddProperty(nextPath, element.Value);
                }
            }
        }

        public JsonObject(object source)
        {
            if (source == null) return;
            
            _source = source;
            dynamic t = new { };
            
            var pps = source.GetType().GetProperties();
            foreach (var p in pps)
            {
                AddProperty(p.Name, p.GetValue(source, null));
            }
        }

      

        public JsonObjectSerialization SerializationType
        {
            get { return _serializationType; }
            set { _serializationType = value; }
        }

        public JsonObject Parent { get; set; }

        public object Source
        {
            get { return _source; }
        }


        public string[] Properties
        {
            get { return _properties.Keys.ToArray(); }
        }

        public object this[string propertyName]
        {
            get
            {
                if (_properties.ContainsKey(propertyName))
                    return _properties[propertyName];
                if (propertyName == "parent")
                    return Parent;

                return null;
            }
            set
            {
                if (_properties.ContainsKey(propertyName))
                    _properties[propertyName] = value;
                else
                    _properties.Add(propertyName, value);
            }
        }

        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }

       

        public void RemoveProperty(string name)
        {
            if (_properties.ContainsKey(name))
                _properties.Remove(name);
        }

        public bool HasProperty(string propertyName)
        {
            return _properties.ContainsKey(propertyName) || (Parent != null && propertyName == "parent");
        }

        public void AddProperty(string name, object value)
        {
            var jsonObject = value as JsonObject;
            if (jsonObject != null)
            {
                (jsonObject).Parent = this;
            }
            var jsonObjectList = value as JsonObjectList;
            if (jsonObjectList != null)
            {
                (jsonObjectList).Parent = this;
            }
            if (_properties.ContainsKey(name))
                this[name] = value;
            else
                _properties.Add(name, value);
        }
        

        public static void FindAllChildren(JsonObject obj, List<JsonObject> result)
        {
            result.Add(obj);
            foreach (var p in obj._properties.Values)
            {
                if (p is JsonObjectList)
                {
                    foreach (JsonObject o in (JsonObjectList)p)
                    {
                        FindAllChildren(o, result);
                    }
                }
                else if (p is JsonObject)
                {
                    FindAllChildren((JsonObject)p, result);
                }
            }
        }

        public List<JsonObject> FindChildrenByRegex(string pattern)
        {
            var reg = new Regex(pattern);

            var result = new List<JsonObject>();

            if (String.IsNullOrEmpty(pattern))
            {
                FindAllChildren(this, result);
            }
            else
            {
                FindChildren(this, reg, result);
            }

            return result;
        }

        public void Apply(Action<JsonObject> fn)
        {
            fn(this);
            var children = new List<JsonObject>();
            FindAllChildren(this, children);
            foreach (var o in children)
            {
                if (o != this)
                    o.Apply(fn);
            }
        }

        public IEnumerable<TResult> Apply<TResult>(Func<JsonObject,TResult> fn)
        {
            var res = new List<TResult>();
            res.Add(fn(this));
            var children = new List<JsonObject>();
            FindAllChildren(this, children);
            foreach (var o in children)
            {
                if (o != this)
                    res.AddRange(o.Apply(fn));
            }
            return res;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void FindChildren(JsonObject o, Regex pattern, string path, ref List<JsonObject> result)
        {
            if (pattern.Match(path).Success)
                result.Add(o);
            foreach (var k in o._properties.Keys)
            {
                var p = o._properties[k];
                var list = p as JsonObjectList;
                if (list != null)
                {
                    foreach (JsonObject obj in list)
                    {
                        FindChildren(obj, pattern, path + (!String.IsNullOrEmpty(path) ? "." : "") + k, ref result);
                    }
                }
                else
                {
                    var jsonObject = p as JsonObject;
                    if (jsonObject != null)
                    {
                        FindChildren(jsonObject, pattern, path + (!String.IsNullOrEmpty(path) ? "." : "") + k, ref result);
                    }
                }
            }
        }

        public static void FindChildren(JsonObject o, Regex pattern, List<JsonObject> result)
        {
            FindChildren(o, pattern, "", ref result);
        }

        /// <summary>
        /// Finds the children.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="path">The path to find the children</param>
        /// <param name="result">The result list to be filled</param>
       [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static void FindChildren(JsonObject o, string path, List<JsonObject> result)
        {
            var i = path.IndexOf(".", System.StringComparison.Ordinal);
            if (i > 0)
            {
                var field = path.Substring(0, i);
                var child = o[field];
                if (child is JsonObjectList)
                {
                    var col = child as JsonObjectList;
                    foreach (JsonObject c in col)
                    {
                        FindChildren(c, path.Substring(i + 1), result);
                    }
                }
                else if (child is JsonObject)
                {
                    FindChildren(child as JsonObject, path.Substring(i + 1), result);
                }
            }
            else
            {
                var child = o[path];
                if (child is JsonObjectList)
                {
                    var col = child as JsonObjectList;
                    result.AddRange(col.Cast<JsonObject>());
                }
                else
                {
                    result.Add(child as JsonObject);
                }
            }
        }

       /// <summary>
       /// Adds and merges an object to the current JsonObject.
       /// </summary>
       /// <param name="obj">The object to be merge</param>
        public void AddObject(JsonObject obj)
        {
            foreach (var k in obj._properties.Keys.Where(k => !_properties.ContainsKey(k)))
            {
                this[k] = obj[k];
            }
        }

        private static object GetPropertyValue(object obj, string property)
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

            return null;
        }

        /// <summary>
        /// Get the value of a property of a child property (with a path like prop1.prop2)
        /// </summary>
        /// <param name="propertyName">The name or path of the property</param>
        /// <param name="dataObject">The data object to explore (by default the current JsonObject)</param>
        /// <returns></returns>
        public object GetValue(string propertyName, object dataObject = null)
        {
            var obj = dataObject ?? this;
            if (propertyName == "this" || propertyName == ".")
                return obj;

            
            var path = propertyName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (path.Length == 0)
                return obj;

            var index = 0;
            var child = GetPropertyValue(obj, path[0]);
            index++;
            var scope = child;
            while (index < path.Length)
            {
                if (child != null)
                {
                    child = GetPropertyValue(child, path[index]);
                    scope = child ?? scope;
                }
                else
                {
                    break;
                }
                index++;
            }
            //var lastIdent = path[path.Length - 1];
            //Match mfunc = new Regex(_RegSimpleFunc).Match(lastIdent);
            
            return child;
        }

        public override string ToString()
        {
            return Serialize();
        }

        private string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);

        }

        public static object Deserialize(string str)
        {
            return JsonConvert.DeserializeObject(str);
        }

       
        #region ISerializable Membres

        protected  JsonObject(SerializationInfo info, StreamingContext context)
        {
          

            //Deserialize(info.GetString("data"));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var property in _properties)
            {
                info.AddValue(property.Key, property.Value);
            }

          
        }

        #endregion
    }
}