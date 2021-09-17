using System;

namespace WebCore.Platform.Posix
{
    [Flags]
    public enum WhenceFlags : int
    {
        SEEK_SET = 0,
        SEEK_CUR = 1,
        SEEK_END = 2
    }
}
