using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using ED47.BusinessAccessLayer;
using Ninject;

namespace ED47.Settings.EF
{
    public class SettingRepository : ISettingRepository
    {
        public virtual ED47.Settings.ISetting Get(string path, string name, string defaultValue = null)
        {

            var cache = MemoryCache.Default;

            var key = String.Format("path={0}&name={1}", path, name);
            var cachesetting = cache.Get(key) as Settings.ISetting;
            if (cachesetting != null) return cachesetting;


            var setting = BusinessComponent.Kernel.Get<BaseUserContext>().Repository.Find<Entity.Setting, Setting>(el => el.Path == path && el.Name == name);

            if (setting == null)
            {
                setting = new Setting
                {
                    Name = name,
                    Path = path,
                    Value = defaultValue
                };

                Add(setting);
            }

            cache.Add(key, setting, new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15)
            });

            return setting;
        }

        public virtual IEnumerable<ED47.Settings.ISetting> GetByPath(string path)
        {
            return BusinessComponent.Kernel.Get<BaseUserContext>().Repository.Where<Entity.Setting, Setting>(el => el.Path.StartsWith(path));
        }

        public virtual void Add(ED47.Settings.ISetting setting)
        {
            BusinessComponent.Kernel.Get<BaseUserContext>().Repository.Add<Entity.Setting, ED47.Settings.ISetting>(setting);
        }

        public virtual void SetValue(ED47.Settings.ISetting setting, string value)
        {
            setting.Value = value;
            
            BusinessComponent.Kernel.Get<BaseUserContext>().Repository.Update<Entity.Setting, ED47.Settings.ISetting>(setting);
        }

        public virtual void Remove(ED47.Settings.ISetting setting)
        {
            BusinessComponent.Kernel.Get<BaseUserContext>().Repository.Delete<Entity.Setting, ED47.Settings.ISetting>(setting);
        }

        public void Commit()
        {
            BusinessComponent.Kernel.Get<BaseUserContext>().Repository.Commit();
        }
    }
}