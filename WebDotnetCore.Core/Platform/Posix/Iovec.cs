﻿using System;

namespace Voron.Platform.Posix
{
    public struct Iovec
    {
        public IntPtr iov_base; // Starting address
        
        public UIntPtr iov_len;  // Number of bytes to transfer
    }


    // mode_t
}
