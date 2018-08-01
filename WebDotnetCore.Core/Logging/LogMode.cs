using System;

namespace WebCore.Logging
{
    [Flags]
    public enum LogMode
    {
        None = 0,
        Operations = 1, // High level info for operational users
        Information = 3 // Low level debug info
    }
}