using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using ED47.BusinessAccessLayer;
using Ninject;

namespace ED47.Settings.EF
{
    public class SettingRepository : ISettingRepository
    {
        public virtual ED47.Settings.ISetting Get(string path, string name, string defaultValue = null, bool ignoreCache = false)
        {
            var cache = MemoryCache.Default;
            var key = String.Format("Settings?path={0}&name={1}", path, name);

            if (!ignoreCache)
            {
                var cachesetting = cache.Get(key) as Settings.ISetting;
                if (cachesetting != null) return cachesetting;
            }

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
            var cache = MemoryCache.Default;

            var key = String.Format("Settings?path={0}&name={1}", setting.Path, setting.Name);
            if (cache.Contains(key))
                cache.Remove(key);

            BusinessComponent.Kernel.Get<BaseUserContext>().Repository.Update<Entity.Setting, ED47.Settings.ISetting>(setting);
        }

        public virtual void Remove(ED47.Settings.ISetting setting)
        {
            BusinessComponent.Kernel.Get<BaseUserContext>().Repository.Delete<Entity.Setting, ED47.Settings.ISetting>(setting);
        }


        public virtual IEnumerable<Settings.ISetting> GetAll()
        {
            var cache = MemoryCache.Default;

            var key = String.Format("Settings?all");
            var settings = cache.Get(key) as IEnumerable<Settings.ISetting>;
            if (settings != null) return settings;
            settings = BusinessComponent.Kernel.Get<BaseUserContext>().Repository.Where<Entity.Setting, Setting>(el => true).ToArray();
            cache.Add(key, settings, new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30)
            });
            return settings;
        }

        public void Commit()
        {
            BusinessComponent.Kernel.Get<BaseUserContext>().Repository.Commit();
        }
    }
}