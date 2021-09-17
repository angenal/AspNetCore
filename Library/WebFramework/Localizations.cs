using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using WebCore;
using WebInterface;

namespace WebFramework
{
    /// <summary>Localizations for ResourceManager</summary>
    public sealed class Localizations
    {
        /// <summary>
        /// Default Localizations
        /// </summary>
        public static Language Default = Language.Chinese;
        /// <summary>
        /// Default Culture String
        /// </summary>
        public static string DefaultCulture = "zh-CN";
        /// <summary>
        /// Set Default ResourceManager use SetDefaultCulture()
        /// </summary>
        public static ResourceManager ResourceManager;
        /// <summary>
        /// Set Default Culture use SetDefaultCulture()
        /// </summary>
        public static CultureInfo Culture;

        /// <summary>
        /// Supported Cultures
        /// </summary>
        /// <returns></returns>
        public static IList<CultureInfo> SupportedCultures()
        {
            var cultures = new List<CultureInfo>();
            Type type = typeof(Language);
            foreach (string name in Enum.GetNames(type))
            {
                var field = type.GetField(name);
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                cultures.Add(new CultureInfo(attribute.Description));
            }
            return cultures;
        }

        /// <summary>
        /// Get Default Culture
        /// </summary>
        /// <returns></returns>
        public static CultureInfo GetDefaultCulture()
        {
            Type type = typeof(Language);
            var field = type.GetField(Default.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return new CultureInfo(attribute.Description);
        }

        /// <summary>
        /// Set Default Culture and ResourceManager:{AssemblyName}.Resources-en-US.resources
        /// </summary>
        /// <param name="newLocalization"></param>
        public static bool SetDefaultCulture(Language newLocalization)
        {
            return SetDefaultCulture(newLocalization.ToString());
        }

        /// <summary>
        /// Set Default Culture and ResourceManager:{AssemblyName}.Resources-en-US.resources
        /// </summary>
        /// <param name="newLocalization"></param>
        public static bool SetDefaultCulture(string newLocalization)
        {
            string newValue = newLocalization.Length <= 5 ? newLocalization : newLocalization.ToTitleCase(), culture = null;
            Language value = Default;
            Type type = typeof(Language);
            var assembly = Assembly.GetEntryAssembly();
            bool ok = false, parsed = Enum.TryParse(type, newValue, out var result);
            if (parsed)
            {
                value = (Language)result;
                var name = value.ToString();
                var field = type.GetField(name);
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute == null) return ok;
                culture = attribute.Description;
                ok = true;
            }
            if (!ok)
            {
                foreach (string name in Enum.GetNames(type))
                {
                    var field = type.GetField(name);
                    var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attribute == null || !attribute.Description.Equals(newValue, StringComparison.OrdinalIgnoreCase)) continue;
                    culture = attribute.Description;
                    value = (Language)Enum.Parse(type, name);
                    ok = true;
                    break;
                }
            }
            if (ok)
            {
                Default = value;
                DefaultCulture = culture;
                Culture = GetDefaultCulture();
                var baseName = $"{assembly.GetName().Name}.{culture}";
                //var baseName = $"{assembly.GetName().Name}.Resources-{culture}";
                ResourceManager = new ResourceManager(baseName, assembly);
                Thread.CurrentThread.CurrentCulture = Culture;
                Thread.CurrentThread.CurrentUICulture = Culture;
            }
            return ok;
        }
    }
}
