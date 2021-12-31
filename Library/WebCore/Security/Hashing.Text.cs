using System.Runtime.CompilerServices;
using System.Text;

namespace WebCore
{
    public static unsafe partial class Hashing
    {

        /// <summary>
        /// Get 64-bit hash code for a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ulong HashString(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return HashBytes(ref bytes);
        }

        /// <summary>
        /// Get 64-bit hash code for a byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static unsafe ulong HashBytes(ref byte[] bytes)
        {
            fixed (byte* b = bytes)
            {
                return HashBytes(b, bytes.Length);
            }
        }

        /// <summary>
        /// Get 64-bit hash code for a byte array
        /// </summary>
        /// <param name="src"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ulong HashBytes(byte* src, int len)
        {
            const long magicno = 40343;
            ulong hashState = (ulong)len;
            char* pwStr = (char*)src;
            int cbBuf = len / 2;

            for (int i = 0; i < cbBuf; i++, pwStr++)
            {
                hashState = magicno * hashState + *pwStr;
            }

            if ((len & 1) > 0)
            {
                byte* pC = (byte*)pwStr;
                hashState = magicno * hashState + *pC;
            }

            hashState = magicno * hashState;
            return (hashState >> 4) | (hashState << 60);
        }

        /// <summary>
        /// Compute XOR hash code of all provided bytes
        /// </summary>
        /// <param name="src"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ulong XorBytes(byte* src, int len)
        {
            ulong result = 0;
            byte* curr = src;
            byte* end = src + len;
            while (curr + 4 * sizeof(ulong) <= end)
            {
                result ^= *(ulong*)curr;
                result ^= *(1 + (ulong*)curr);
                result ^= *(2 + (ulong*)curr);
                result ^= *(3 + (ulong*)curr);
                curr += 4 * sizeof(ulong);
            }
            while (curr + sizeof(ulong) <= end)
            {
                result ^= *(ulong*)curr;
                curr += sizeof(ulong);
            }
            while (curr + 1 <= end)
            {
                result ^= *curr;
                curr++;
            }

            return result;
        }

        /// <summary>
        /// Compute 64-bit hash code for a deterministic string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long GetHashCode2(this string str)
        {
            unchecked
            {
                long hash1 = 352654597, hash2 = hash1;
                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                    {
                        break;
                    }
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }
                return hash1 + hash2 * 1566083941;
            }
        }

        /// <summary>
        /// Compute 64-bit hash code for a deterministic byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static long GetHashCode2(this byte[] bytes)
        {
            unchecked
            {
                long hash1 = 352654597, hash2 = hash1;
                for (int i = 0; i < bytes.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ bytes[i];
                    if (i == bytes.Length - 1)
                    {
                        break;
                    }
                    hash2 = ((hash2 << 5) + hash2) ^ bytes[i + 1];
                }
                return hash1 + hash2 * 1566083941;
            }
        }

    }
}
