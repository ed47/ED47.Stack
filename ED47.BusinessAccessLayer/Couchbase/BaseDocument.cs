using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.Couchbase
{
    public abstract class BaseDocument : IDocument
    {
        public virtual void Init()
        {
            
        }

        public virtual void AfterSave()
        {
           
        }

        public bool Save()
        {
            return CouchbaseRepository.Store(this);
        }

        public bool Delete()
        {
            return CouchbaseRepository.Delete(this);
        }

        private bool _loaded = false;
        
        public virtual bool Load(bool force = false)
        {
            if(!_loaded || force )
                _loaded = CouchbaseRepository.Load(this);

            return _loaded;
        }

        public bool Exists()
        {
            return Load();
        }

        private string _type = null;
        public string Type
        {
            get { return _type ?? (_type = GetType().Name); }
            set { _type = value; }
        }

        private int? _id;

        public int Id
        {
            get
            {
                if (!_id.HasValue)
                {
                    _id = CouchbaseRepository.GetNewId(Type);
                }
                return _id.Value;

            }
            set { _id = value; }
        }

       

        protected virtual string CalcKey()
        {
            return (Type + "?id=" + Id).ToLower();
        }
        private string _key = null;
        public string Key
        {
            get { return _key ?? (_key = CalcKey()); }
            set { _key = value; }
        }

        public virtual string GetKey()
        {
            return Key;
        }
    }
}
