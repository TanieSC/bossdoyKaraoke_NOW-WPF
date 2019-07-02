﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bossdoyKaraoke_NOW.Properties;

namespace bossdoyKaraoke_NOW
{
    class AppConfig
    {
        static Configuration _config;
        static ClientSettingsSection _entry;
        static string _appSetting;

        private static void AddProperty(string key, string value)
        {
            //  if (Settings.Default.Properties[key] == null)
            //  {
            SettingsProperty newProp = new SettingsProperty(key)
            {
                PropertyType = typeof(string),
                SerializeAs = SettingsSerializeAs.String,
                DefaultValue = string.Empty,
                Provider = Settings.Default.Providers["LocalFileSettingsProvider"],
            };

            newProp.Attributes.Add(typeof(ApplicationScopedSettingAttribute), new ApplicationScopedSettingAttribute());  //.Add(getType(Configuration.UserScopedSettingAttribute), New Configuration.UserScopedSettingAttribute())

            SettingsPropertyValue newPropValue = new SettingsPropertyValue(newProp);
            newPropValue.PropertyValue = value;

            Settings.Default.Properties.Add(newProp);
            Settings.Default.PropertyValues.Add(newPropValue);
            Settings.Default.Save();
            //  }
        }

        public static void Initialize()
        {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            _entry = _config.GetSectionGroup("applicationSettings").Sections["bossdoyKaraoke_NOW.Properties.Settings"] as ClientSettingsSection;
        }

        public static void Set(string key, string value)
        {
            if (Settings.Default.Properties[key] != null)
            {
                Settings.Default[key] = value;
                Settings.Default.Save();
            }
            else
            {
                AddProperty(key, value);
            }
        }

        public static T Get<T>(string key)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            try
            {
                // Settings.Default[key].
                _appSetting = _entry.Settings.Get(key).Value.ValueXml.InnerText;
            }
            catch (NullReferenceException)
            {

                return default(T);
            }

            return (T)(converter.ConvertFromInvariantString(_appSetting));
        }

        public static void SetFxDefaultSettings(string key, string replaceOldStringPortionOfKey = null, string newStringPortionOfKey = null)
        {
            Settings.Default.Properties.Cast<SettingsProperty>().OrderBy(s => s.Name).Select(d =>
                          {
                              bool isname = d.Name.Contains(key);
                              string name = string.Empty;
                              if (isname)
                              {
                                  if (replaceOldStringPortionOfKey == null && newStringPortionOfKey == null)
                                      name = d.Name.Remove(0, 3);
                                  else
                                      name = d.Name.Remove(0, 3).Replace(replaceOldStringPortionOfKey, newStringPortionOfKey);

                                  Set(name, d.DefaultValue.ToString());

                              }

                              return string.Empty;

                          }).ToArray();
        }
    }
}