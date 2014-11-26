using System.Collections.Generic;

namespace ED47.Settings
{
    public interface ISettingRepository
    {
        ISetting Get(string path, string name, string defaultValue = null);
        IEnumerable<ISetting> GetByPath(string path);
        IEnumerable<ISetting> GetAll();
        void Add(ISetting setting);
        void SetValue(ISetting setting, string value);
        void Remove(ISetting setting);
        void Commit();
    }
}