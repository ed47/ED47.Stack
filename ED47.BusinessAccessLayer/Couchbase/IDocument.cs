using System.Runtime.Serialization;
using Newtonsoft;
namespace ED47.BusinessAccessLayer.Couchbase
{
    public interface IDocument
    {
        int Id { get;  }
        string Type { get; set; }
        string Key { get; set; } 
        string GetKey();
        void Init();
        void AfterSave();
        bool Save();
    }

   
}