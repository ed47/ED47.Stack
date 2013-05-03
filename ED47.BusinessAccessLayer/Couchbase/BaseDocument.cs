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
            return Repository.Store(this);
        }

        private bool _loaded = false;
        
        public virtual bool Load(bool force = false)
        {
            if(!_loaded || force )
                _loaded = Repository.Load(this);

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
                    _id = Repository.GetNewId(Type);
                }
                return _id.Value;

            }
            set { _id = value; }
        }

        private string _key = null;

        protected string CalcKey(int id)
        {
            return Type + "?id=" + Id;
        }

        public string Key
        {
            get { return _key ?? (_key = Type + "?id=" + Id); }
            set { _key = value; }
        }

        public virtual string GetKey()
        {
            return Key;
        }
    }
}
