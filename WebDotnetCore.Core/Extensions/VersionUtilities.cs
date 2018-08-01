using System;

namespace WebCore
{
    /// <summary>Provides methods to work with <see cref="Version"/> objects. </summary>
    public static class VersionUtilities
    {
        /// <summary>Converts a string to a <see cref="Version"/> object. </summary>
        /// <param name="version">The version as string. </param>
        /// <returns>The version. </returns>
        public static Version FromString(string version)
        {
            try
            {
                return !string.IsNullOrEmpty(version) ? new Version(version.Split('-')[0]) : new Version(0, 0, 0, 0);
            }
            catch (Exception)
            {
                return new Version(0, 0, 0, 0);
            }
        }
    }
}
