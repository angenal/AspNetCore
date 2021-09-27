using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using WebCore.Platform;
using WebCore.Platform.Posix;

namespace WebCore
{
    public static unsafe class UnmanagedMemory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Copy(byte* dest, byte* src, long count)
        {
            Debug.Assert(count >= 0);
            return OS.IsPosix
                ? Syscall.Copy(dest, src, count)
                : Win32UnmanagedMemory.Copy(dest, src, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(byte* b1, byte* b2, long count)
        {
            Debug.Assert(count >= 0);
            return OS.IsPosix
                ? Syscall.Compare(b1, b2, count)
                : Win32UnmanagedMemory.Compare(b1, b2, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Move(byte* dest, byte* src, long count)
        {
            Debug.Assert(count >= 0);
            return OS.IsPosix
                ? Syscall.Move(dest, src, count)
                : Win32UnmanagedMemory.Move(dest, src, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Set(byte* dest, int c, long count)
        {
            Debug.Assert(count >= 0);
            return OS.IsPosix
                ? Syscall.Set(dest, c, count)
                : Win32UnmanagedMemory.Set(dest, c, count);
        }
    }
}
