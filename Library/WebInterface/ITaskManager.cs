using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebInterface
{
    public interface ITaskManager
    {
        void Enqueue(Func<CancellationToken, Task> task);
        Task<Func<CancellationToken, Task>> Dequeue(CancellationToken cancellationToken);
        void RunOnceAt(Action job, DateTime time);
    }
}
