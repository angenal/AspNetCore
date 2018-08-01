using System;
using WebCore.Threading;

namespace WebCore.Utils
{
    public abstract class PooledItem : IDisposable
    {
        public MultipleUseFlag InUse = new MultipleUseFlag();
        public DateTime InPoolSince;

        public abstract void Dispose();
    }
}
