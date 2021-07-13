﻿namespace WebCore.Platform.Posix.macOS
{
    public struct xsw_usage
    {
        public ulong xsu_total;
        public ulong xsu_avail;
        public ulong xsu_used;
        public uint xsu_pagesize;
        public bool xsu_encrypted;
    };
}
