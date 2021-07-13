using WebCore.Annotations;
using WebCore.Properties;

namespace WebCore
{
    public static class AbstractionsStrings
    {
        /// <summary>
        ///     The string argument '{argumentName}' cannot be empty.
        /// </summary>
        public static string ArgumentIsEmpty([CanBeNull] object argumentName)
        {
            return string.Format(AbstractionsStrings.GetString("ArgumentIsEmpty", new string[]
            {
                "argumentName"
            }), argumentName);
        }

        /// <summary>
        ///     The collection argument '{argumentName}' must contain at least one element.
        /// </summary>
        public static string CollectionArgumentIsEmpty([CanBeNull] object argumentName)
        {
            return string.Format(AbstractionsStrings.GetString("CollectionArgumentIsEmpty", new string[]
            {
                "argumentName"
            }), argumentName);
        }

        private static string GetString(string name, params string[] formatterNames)
        {
            string text = Resources.ResourceManager.GetString(name, Resources.Culture);
            for (int i = 0; i < formatterNames.Length; i++)
            {
                text = text.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
            }
            return text;
        }
    }
}
