using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebCore.IO
{
    /// <summary>
    /// An component that retrieves its md5 value from a file.
    /// </summary>
    public class FileMD5
    {
        /// <summary>
        /// Gets the md5 value.
        /// </summary>
        /// <param name="paths">The paths to read. The first path that can be successfully read will be used.</param>
        /// <returns>The md5 value.</returns>
        public static string ComputeHash(params string[] paths) => new FileMD5(paths).GetValue();

        /// <summary>
        /// The paths to read.
        /// </summary>
        private readonly string[] _paths;

        /// <summary>
        /// Should the contents of the file be hashed?
        /// </summary>
        private readonly bool _hashContents;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMD5"/> class.
        /// </summary>
        /// <param name="path">The path of the file holding the component ID.</param>
        /// <param name="hashContents">A value determining whether the file contents should be hashed.</param>
        public FileMD5(string path, bool hashContents = false) : this(new[] { path }, hashContents) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMD5"/> class.
        /// </summary>
        /// <param name="paths">The paths to read. The first path that can be successfully read will be used.</param>
        /// <param name="hashContents">A value determining whether the file contents should be hashed.</param>
        public FileMD5(IEnumerable<string> paths, bool hashContents = false)
        {
            _paths = paths.ToArray();
            _hashContents = hashContents;
        }

        /// <summary>
        /// Gets the md5 value.
        /// </summary>
        /// <returns>The md5 value.</returns>
        public string GetValue()
        {
            foreach (var path in _paths)
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    string contents;

                    using (var file = File.OpenText(path))
                    {
                        contents = file.ReadToEnd(); // File.ReadAllBytes() fails for special files such as /sys/class/dmi/id/product_uuid
                    }

                    contents = contents.Trim();

                    if (!_hashContents)
                    {
                        return contents;
                    }

                    return contents.Md5();
                }
                catch
                {
                    // Can fail if we have no permissions to access the file.
                }
            }

            return null;
        }
    }
}
