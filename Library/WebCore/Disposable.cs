using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WebCore
{
    /// <summary>
    /// Provides a mechanism for releasing unmanaged resources.
    /// </summary>
    public class Disposable : IDisposable
    {
        private readonly ConcurrentStack<Action> _disposeTasks = new ConcurrentStack<Action>();

        private readonly Lazy<CancellationTokenSource> _lazyCancellationTokenSource = new Lazy<CancellationTokenSource>(() => new CancellationTokenSource(), true);

        protected CancellationToken CancellationToken => _lazyCancellationTokenSource.Value.Token;

        private long _disposeSignaled;

        /// <summary></summary>
        public bool IsDisposed => Interlocked.Read(ref _disposeSignaled) != 0;

        /// <summary></summary>
        protected void OnDispose(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _disposeTasks.Push(action);
        }

        /// <summary></summary>
        protected void OnDispose(IDisposable disposable)
        {
            if (disposable is null)
            {
                throw new ArgumentNullException(nameof(disposable));
            }

            OnDispose(disposable.Dispose);
        }

        /// <summary>Do cleanup here</summary>
        protected virtual void Cleanup()
        {
        }

        /// <summary></summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                bool hasDisposed = Interlocked.CompareExchange(ref _disposeSignaled, 1, 0) == 1;
                if (hasDisposed)
                {
                    return;
                }

                Cleanup();

                while (_disposeTasks.TryPop(out Action disposableAction))
                {
                    try
                    {
                        disposableAction();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                if (_lazyCancellationTokenSource.IsValueCreated)
                {
                    using (CancellationTokenSource cancellationTokenSource = _lazyCancellationTokenSource.Value)
                    {
                        cancellationTokenSource.Cancel(false);
                    }
                }
            }
        }

        /// <summary></summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Take yourself off the finalization queue to prevent finalization from executing a second time.
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Provides a mechanism for releasing unmanaged resources asynchronously.
    /// </summary>
    public abstract class AsyncDisposable : IAsyncDisposable
    {
        private readonly ConcurrentStack<Func<Task>> _disposeTasks = new ConcurrentStack<Func<Task>>();

        private readonly Lazy<CancellationTokenSource> _lazyCancellationTokenSource = new Lazy<CancellationTokenSource>(() => new CancellationTokenSource(), true);

        protected CancellationToken CancellationToken => _lazyCancellationTokenSource.Value.Token;

        private long _disposeSignaled;

        /// <summary></summary>
        public bool IsDisposed => Interlocked.Read(ref _disposeSignaled) != 0;

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
        public async ValueTask DisposeAsync()
        {
            bool hasDisposed = Interlocked.CompareExchange(ref _disposeSignaled, 1, 0) == 1;
            if (hasDisposed)
            {
                return;
            }

            await Cleanup().ConfigureAwait(false);

            while (_disposeTasks.TryPop(out Func<Task> disposable))
            {
                Task shutdownTask = disposable();
                if (shutdownTask is null) continue;
                try
                {
                    await shutdownTask.ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (_lazyCancellationTokenSource.IsValueCreated)
            {
                using (CancellationTokenSource cancellationTokenSource = _lazyCancellationTokenSource.Value)
                {
                    cancellationTokenSource.Cancel(false);
                }
            }

            // Take yourself off the finalization queue to prevent finalization from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary></summary>
        protected void OnDispose(Func<Task> disposeTask)
        {
            _disposeTasks.Push(async () => await disposeTask().ConfigureAwait(false));
        }

        /// <summary></summary>
        protected void OnDispose(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            OnDispose(() =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        /// <summary></summary>
        protected void OnDispose(IDisposable disposable)
        {
            if (disposable is null)
            {
                throw new ArgumentNullException(nameof(disposable));
            }

            OnDispose(() =>
            {
                disposable.Dispose();
                return Task.CompletedTask;
            });
        }

        /// <summary></summary>
        protected void OnDispose(IAsyncDisposable disposable)
        {
            if (disposable is null)
            {
                throw new ArgumentNullException(nameof(disposable));
            }

            OnDispose(() => disposable.DisposeAsync().AsTask());
        }

        /// <summary>Do cleanup here</summary>
        protected virtual Task Cleanup()
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Provides a class for releasing unmanaged resources.
    /// </summary>
    public sealed class EmptyDisposable : Disposable
    {
    }
}
