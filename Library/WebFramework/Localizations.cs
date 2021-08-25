using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace WebFramework
{
    /// <summary>Localizations for ResourceManager</summary>
    public sealed class Localizations
    {
        /// <summary>
        /// Default Localizations
        /// </summary>
        public static AvaliableLocalizations Default = AvaliableLocalizations.Chinese;
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
            Type type = typeof(AvaliableLocalizations);
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
            Type type = typeof(AvaliableLocalizations);
            var field = type.GetField(Default.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return new CultureInfo(attribute.Description);
        }

        /// <summary>
        /// Set Default Culture and ResourceManager:{AssemblyName}.Resources-en-US.resources
        /// </summary>
        /// <param name="newLocalization"></param>
        public static void SetDefaultCulture(AvaliableLocalizations newLocalization)
        {
            Default = newLocalization;
            Culture = GetDefaultCulture();
            Thread.CurrentThread.CurrentCulture = Culture;
            Thread.CurrentThread.CurrentUICulture = Culture;
            Type type = typeof(AvaliableLocalizations);
            foreach (string name in Enum.GetNames(type))
            {
                if (newLocalization != (AvaliableLocalizations)Enum.Parse(type, name)) continue;
                var field = type.GetField(name);
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                var assembly = Assembly.GetEntryAssembly();
                var baseName = $"{assembly.GetName().Name}.Resources-{attribute.Description}";
                ResourceManager = new ResourceManager(baseName, assembly);
                break;
            }
        }
    }

    /// <summary>
    /// Avaliable Localizations
    /// </summary>
    public enum AvaliableLocalizations
    {
        /// <summary>
        /// English
        /// </summary>
        [Description("en-US")]
        English,
        /// <summary>
        /// Chinese
        /// </summary>
        [Description("zh-CN")]
        Chinese
    }
}
