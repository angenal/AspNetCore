using K4os.Compression.LZ4.Streams;
using System.IO;

namespace WebCore
{
    public static class FileExtensions
    {
        /// <summary>
        /// 文件访问时间戳
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string LastAccessTimeHex(this FileInfo file) => file.LastAccessTime.ToTimestampHex();
        /// <summary>
        /// 文件写入时间戳
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string LastWriteTimeHex(this FileInfo file) => file.LastWriteTime.ToTimestampHex();

        /// <summary>
        /// 压缩文件 LZ4
        /// </summary>
        /// <param name="lz4"></param>
        /// <param name="files"></param>
        public static void LZ4Encode(this FileInfo lz4, params FileInfo[] files)
        {
            const int buffer = 4096;
            if (files.Length == 0) return;
            if (lz4.Exists) lz4.Delete();
            using (var stream = LZ4Stream.Encode(lz4.Create()))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.AutoFlush = true;
                    foreach (var file in files)
                    {
                        using (var sr = file.OpenRead())
                        {
                            int offset = 0;
                            byte[] array = new byte[buffer];
                            while (0 < sr.Read(array, offset, buffer))
                            {
                                writer.Write(array);
                                offset += array.Length;
                                array = new byte[buffer];
                            }
                            foreach (byte b in array)
                            {
                                writer.Write(b);
                                if (b == 0) break;
                            }
                            sr.Close();
                        }
                    }
                    writer.Close();
                }
                stream.Close();
            }
        }


        ///// <summary>
        ///// 解缩文件 LZ4
        ///// </summary>
        ///// <param name="lz4"></param>
        ///// <param name="dir"></param>
        //public static FileInfo[] LZ4Decode(this FileInfo lz4, string dir = null)
        //{
        //    const int buffer = 4096;
        //    var files = new List<FileInfo>();
        //    if (!lz4.Exists) return files.ToArray();
        //    if (dir == null) dir = lz4.DirectoryName;
        //    using (var stream = lz4.OpenRead())
        //    {
        //        int i = 0;
        //        using (var sr = LZ4Stream.Decode(stream))
        //        {
        //            int offset = 0, offset1 = 0;
        //            var file = files[i];
        //            using (var sw = file.OpenWrite())
        //            {
        //                byte[] array = new byte[buffer];
        //                while (0 < sr.Read(array, offset, buffer))
        //                {
        //                    sw.Write(array, offset1, buffer);
        //                    offset += array.Length;
        //                    offset1 += array.Length;
        //                    array = new byte[buffer];
        //                }
        //                offset1 = 0;
        //                sw.Close();
        //            }
        //        }
        //        stream.Close();
        //    }
        //    return files.ToArray();
        //}

    }
}
