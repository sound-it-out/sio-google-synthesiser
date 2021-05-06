using System.Collections.Concurrent;
using System.Threading;

namespace SIO.Google.Credentials.Connections
{
    internal class BlockingQueue<T>
    {
        private readonly object _queueLock = new object();
        private readonly SemaphoreSlim _semaphore;        
        private readonly ConcurrentQueue<T> _queue;

        public BlockingQueue(params T[] items)
        {
            _semaphore = new SemaphoreSlim(items.Length);
            _queue = new ConcurrentQueue<T>(items);
        }

        public void Enqueue(T item)
        {
            lock (_queueLock)
            {
                _queue.Enqueue(item);
                _semaphore.Release();
            }
        }

        public bool TryDequeue(out T item, CancellationToken cancellationToken = default)
        {
            _semaphore.Wait(cancellationToken);
            lock (_queueLock)
            {
                return _queue.TryDequeue(out item);
            }
        }
    }
}
