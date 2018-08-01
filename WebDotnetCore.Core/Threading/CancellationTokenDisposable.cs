using System;
using System.Threading;

namespace WebCore.Threading
{
    public class CancellationTokenDisposable : IDisposable
    {
        public CancellationToken Token
        {
            get
            {
                return cts.Token;
            }
        }

        public void Dispose()
        {
            if (!cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
        }

        private readonly CancellationTokenSource cts = new CancellationTokenSource();
    }
}
