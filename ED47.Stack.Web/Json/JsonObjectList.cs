using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ED47.Stack.Web
{
    public delegate void ApplyDelegate(JsonObject jsonObject);
     [Serializable]
    public class JsonObjectList : IEnumerable,ISerializable
    {
        public int Tab;
        private readonly List<JsonObject> _items = new List<JsonObject>();
        private JsonObject _parent;
        private string[] _sortFields;

        public JsonObjectList()
        {
        }

        public JsonObjectList(IEnumerable objs, string typename = null)
        {
            if (objs == null) return;
            foreach (var o in objs)
            {
                if (o is JsonObject)
                {
                    var obj = o as JsonObject;
                    Add(obj);
                    obj.TypeName = typename;
                }
                else
                    Add(new JsonObject(o) { TypeName = typename });
            }
        }
        
        public JsonObject Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                foreach (var o in _items)
                {
                    o.Parent = value;
                }
            }
        }

        public IEnumerable<JsonObject> Items
        {
            get { return _items.ToArray(); }
        }

        public JsonObject this[int index]
        {
            get
            {
                try
                {
                    return _items[index];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public JsonObject Current
        {
            get { return _items.Count > 0 ? _items[_items.Count - 1] : null; }
        }

       

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return new JsonObjectListEnum(_items);
        }

        #endregion IEnumerable Members

        public void AddObjects(IEnumerable<object> objs)
        {
            foreach (var o in objs)
            {
                if (o is JsonObject)
                {
                    Add(o as JsonObject);
                }
                else
                {
                    Add(new JsonObject(o));
                }
            }
        }

        public void Add(IEnumerable<JsonObject> objs)
        {
            foreach (var o in objs)
            {
                Add(o);
            }
        }

        private int _SortFunc(JsonObject a, JsonObject b)
        {
            var i = 0;
            var cmp = 0;
            Type ta;
            var tname = "System.String";
            var oa = a[_sortFields[i]];
            var ob = b[_sortFields[i]];

            if (oa == null) return -1;
            if (ob == null) return 1;

            while (i < _sortFields.Length && cmp == 0)
            {
                ta = oa.GetType();
                Type tb = ob.GetType();
                if (ta == tb) tname = ta.FullName;
                switch (tname)
                {
                    case "System.Int32":
                        cmp = Comparer<Int32>.Default.Compare(Convert.ToInt32(oa, CultureInfo.InvariantCulture), Convert.ToInt32(ob,CultureInfo.InvariantCulture));
                        break;

                    case "System.DateTime":
                        cmp = Comparer<DateTime>.Default.Compare(Convert.ToDateTime(oa, CultureInfo.InvariantCulture), Convert.ToDateTime(ob, CultureInfo.InvariantCulture));
                        break;
                    default:
                        cmp = Comparer<string>.Default.Compare(oa.ToString(), ob.ToString());
                        break;
                }
                i++;
            }
            return cmp;
        }

        public void Sort(string pattern)
        {
            _sortFields = pattern.Split(new[] { ',' });
            _items.Sort(_SortFunc);
        }

        public JsonObject AddNew()
        {
            var item = new JsonObject();
            _items.Add(item);
            return item;
        }

        public void Add(JsonObject item)
        {
            item.Parent = Parent;
            _items.Add(item);
        }

        public void SetTypeName(string name)
        {
            foreach (var t in _items)
            {
                t.TypeName = name;
            }
        }

     

        public JsonObjectList GroupBy(string[] groupFields, string[] groupNames)
        {
            var keyfields = groupFields[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var res = new JsonObjectList();
            var groups = new Dictionary<string, List<JsonObject>>();

            foreach (var elt in _items)
            {
                var keys = new string[keyfields.Length];

                for (var i = 0; i < keyfields.Length; i++)
                {
                    var val = elt[keyfields[i]];
                    keys[i] = val != null ? val.ToString() : "NULL";
                }

                var k = string.Join("#", keys);

                if (!groups.ContainsKey(k))
                {
                    groups.Add(k, new List<JsonObject>());
                }
                groups[k].Add(elt);
            }

            foreach (var k in groups.Keys)
            {
                var o = new JsonObject();
                var g = groups[k];
                foreach (var t in keyfields)
                {
                    o.AddProperty(t, g[0][t]);
                }

                var items = new JsonObjectList { g.ToArray() };
                if (groupFields.Length > 1)
                {
                    var nextFields = new string[groupFields.Length - 1];
                    Array.Copy(groupFields, 1, nextFields, 0, groupFields.Length - 1);

                    var nextGroups = new string[groupNames.Length - 1];
                    Array.Copy(groupNames, 1, nextGroups, 0, groupNames.Length - 1);
                    items = items.GroupBy(nextFields, nextGroups);
                }
                o.AddProperty(groupNames[0], items);

                res.Add(o);
            }
            return res;
        }

        public List<JsonObject> FindChildren(string path)
        {
            if (path == "")
            {
                return _items;
            }

            var result = new List<JsonObject>();
            foreach (var o in _items)
            {
                JsonObject.FindChildren(o, path,  result);
            }
            return result;
        }

        public List<JsonObject> FindChildrenByRegex(string pattern)
        {
            var reg = new Regex(pattern);

            var result = new List<JsonObject>();
            foreach (var o in _items)
            {
                if (pattern == "")
                {
                    JsonObject.FindAllChildren(o,  result);
                }
                else
                {
                    JsonObject.FindChildren(o, reg,  result);
                }
            }
            return result;
        }

        public void Apply(Action<JsonObject> fn)
        {
            foreach (var o in _items)
            {
                o.Apply(fn);
            }
        }

        public IEnumerable<TResult> Apply<TResult>(Func<JsonObject,TResult> fn)
        {
            var res = new List<TResult>();
            foreach (var o in _items)
            {
               res.AddRange(o.Apply(fn));
            }
            return res;
        }

        public void Apply(ApplyDelegate fn, string pattern)
        {
            foreach (var o in FindChildrenByRegex(pattern))
            {
                fn(o);
            }
        }

        /// <summary>
        /// Join the JsonObjects from data to the JsonObject of target.
        /// </summary>
        /// <param name="target">The list of JsonObject to put the new object into</param>
        /// <param name="data">The data to join to the target</param>
        /// <param name="pattern">A pattern corresponding to the path within the data to find the items to be joined with the target</param>
        /// <param name="propertyName">The property to add to regroup all the data joined,
        /// set this parameter to null to merge the data with the target.</param>
        /// <param name="localField">The name of the foreign key if not equals to propertyname</param>
        public static void Join(List<JsonObject> target, JsonObjectList data, string pattern, string propertyName,
                                string localField)
        {
            var i = pattern.LastIndexOf(".");
            var field = i > 0 ? pattern.Substring(i + 1) : pattern;
            var path = i > 0 ? pattern.Substring(0, i) : "";

            var groups = new Dictionary<string, List<JsonObject>>();
            foreach (var elt in data.FindChildren(path))
            {
                if (elt == null) continue;
                var k = elt[field].ToString();
                if (!groups.ContainsKey(k))
                {
                    groups.Add(k, new List<JsonObject>());
                }
                groups[k].Add(elt);
            }

            foreach (var jo in target)
            {
                var k = jo[localField].ToString();
                if (groups.ContainsKey(k))
                {
                    if (propertyName != null)
                    {
                        var items = new JsonObjectList { groups[k] };
                        jo.AddProperty(propertyName, items);
                    }
                    else
                    {
                        if (groups[k].Count > 0)
                            jo.AddObject(groups[k][0]);
                    }
                }
            }
        }

        /// <summary>
        /// Join the data (1 - n) to the current JsonObjectList
        /// </summary>
        /// <param name="data">The data to join with</param>
        /// <param name="pattern">A pattern corresponding to the path within the data to find the items to be joined with the target</param>
        /// <param name="propertyName">The property to create to regroup all the children from data. Null for a (1 - 1) relation</param>
        /// <param name="localField">The field in the current object to be used as a foreign key</param>
        public void Join(JsonObjectList data, string pattern, string propertyName, string localField)
        {
            Join(_items, data, pattern, propertyName, localField);
        }

        /// <summary>
        /// Join the data (1 - n) to the current JsonObjectList
        /// </summary>
        /// <param name="data">The data to join with</param>
        /// <param name="field">The relation field</param>
        /// <param name="propertyName">The property to create to regroup all the children from data. Null for a (1 - 1) relation</param>
        public void Join(JsonObjectList data, string field, string propertyName)
        {
            Join(data, field, propertyName, field);
        }

        /// <summary>
        /// Join the data (1 - 1) to the current JsonObjectList
        /// </summary>
        /// <param name="data">The data to join with</param>
        /// <param name="field">The relation field</param>
        public void Join(JsonObjectList data, string field)
        {
            Join(data, field, null, field);
        }

        public JsonObjectList FilterBy(string field, object value)
        {
            var result = new JsonObjectList();

            foreach (var jso in _items)
            {
                if (jso[field].Equals(value))
                    result.Add(jso);
            }

            return result;
        }

       
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("data",Items);
        }

        public override string ToString()
        {
            return Serialize();
        }

        private string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }

    public class JsonObjectListEnum : IEnumerator
    {
        private int _index = -1;
        private readonly List<JsonObject> _object;

        public JsonObjectListEnum(List<JsonObject> _object)
        {
            this._object = _object;
        }

        #region IEnumerator Members

        public bool MoveNext()
        {
            _index++;
            return (_index < _object.Count);
        }

        public void Reset()
        {
            _index = -1;
        }

        public object Current
        {
            get
            {
                try
                {
                    return _object[_index];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        #endregion IEnumerator Members
    }
}