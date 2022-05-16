using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Noctus.Infrastructure
{
    public class FixedSizedQueue<T>
    {
        private readonly ConcurrentQueue<T> _queue = new();

        public int Size { get; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public void Enqueue(T obj)
        {
            _queue.Enqueue(obj);

            while (_queue.Count > Size)
            {
                _queue.TryDequeue(out _);
            }
        }

        public IEnumerable<T> Get() => _queue.ToList();
    }
}