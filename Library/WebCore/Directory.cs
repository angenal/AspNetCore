using System;
using System.IO;
using System.Threading;

namespace WebCore
{
    public static class DirectoryUtils
    {
        /// <summary>
        /// Delete a directory recursively
        /// </summary>
        /// <param name="path">The folder to delete</param>
        /// <param name="wait">If true, loop on exceptions that are retryable, and verify the directory no longer exists.</param>
        public static void DeleteDirectory(this string path, bool wait = false)
        {
            if (!Directory.Exists(path)) return;

            foreach (string dir in Directory.GetDirectories(path)) DeleteDirectory(dir, wait);

            bool retry = true;
            while (retry)
            {
                retry = false;
                try { Directory.Delete(path, true); }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    if (!wait)
                    {
                        try { Directory.Delete(path, true); }
                        catch { }
                        return;
                    }
                    retry = true;
                }
            }

            if (!wait) return;

            while (Directory.Exists(path)) Thread.Yield();
        }

        /// <summary>
        /// Create a clean directory, removing a previous one if needed.
        /// </summary>
        /// <param name="path">The folder to create</param>
        public static void CreateDirectory(this string path, bool isClean = true, bool wait = false)
        {
            if (Directory.Exists(path))
            {
                if (!isClean) return;
                DeleteDirectory(path, wait);
            }

            Directory.CreateDirectory(path);
        }
    }
}
