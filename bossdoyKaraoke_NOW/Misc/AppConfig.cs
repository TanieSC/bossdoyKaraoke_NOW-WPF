using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bossdoyKaraoke_NOW.Properties;
using Microsoft.CSharp.RuntimeBinder;

namespace bossdoyKaraoke_NOW.Misc
{
    class AppConfig
    {
        static Configuration _config;
        static ClientSettingsSection _entry;
        static string _appSetting;

        private static void AddProperty(string key, dynamic value)
        { 
            //  if (Settings.Default.Properties[key] == null)
            //  {
            SettingsProperty newProp = new SettingsProperty(key)
            {
                PropertyType = typeof(string),
                SerializeAs = SettingsSerializeAs.String,
                DefaultValue = value,
                Provider = Settings.Default.Providers["LocalFileSettingsProvider"]
            };

            newProp.Attributes.Add(typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute());  //.Add(getType(Configuration.UserScopedSettingAttribute), New Configuration.UserScopedSettingAttribute())

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
            _entry = _config.GetSectionGroup("userSettings").Sections["bossdoyKaraoke_NOW.Properties.Settings"] as ClientSettingsSection;
            //Console.WriteLine("Local user config path: {0}", _config.FilePath);
        }

        public static void Set(dynamic key, dynamic value)
        {
            key = Convert.ToString(key);

            if (Settings.Default.Properties[key] != null)
            {
                Settings.Default[key] = Convert.ToString(value);
                Settings.Default.Save();
            }
            else
            {
                AddProperty(key, value);
            }
        }

        public static T Get<T>(dynamic key)
        {
            key = Convert.ToString(key);

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            try
            {
                // Settings.Default[key].
                _appSetting = _entry.Settings.Get(key).Value.ValueXml.InnerText;
            }
            catch (RuntimeBinderException)
            {
                return default(T);
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

                                  Set(name, d.DefaultValue);
                              }

                              return string.Empty;

                          }).ToArray();
            Initialize();
        }
    }
}
