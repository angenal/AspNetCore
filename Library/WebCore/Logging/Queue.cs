using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebCore.Logging
{
    public class WebSocketMessageEntry
    {
        public MemoryStream Data;
        public TaskCompletionSource<object> Task;

        public readonly List<WebSocket> WebSocketsList = new List<WebSocket>();

        public override string ToString()
        {
            return Encodings.Utf8.GetString(Data.ToArray());
        }
    }

    public class CircularQueue<T>
    {
        private readonly T[] _data;
        private readonly int _queueSize;
        private volatile uint _readPos;
        private volatile uint _writePos;

        public CircularQueue(int queueSize)
        {
            _queueSize = queueSize;
            _data = new T[_queueSize];
        }

        private int PositionToArrayIndex(uint pos)
        {
            return (int)(pos % _queueSize);
        }

        public bool Enqueue(T entry)
        {
            var readIndex = PositionToArrayIndex(_readPos);
            var currentWritePos = _writePos;
            var writeIndex = PositionToArrayIndex(currentWritePos + 1);

            if (readIndex == writeIndex)
                return false; // queue full

            _data[PositionToArrayIndex(currentWritePos)] = entry;

            _writePos++;
            return true;
        }

        private int _numberOfTimeWaitedForEnqueue;

        public bool Enqueue(T entry, int timeout)
        {
            if (Enqueue(entry))
            {
                _numberOfTimeWaitedForEnqueue = 0;
                return true;
            }
            while (timeout > 0)
            {
                _numberOfTimeWaitedForEnqueue++;
                var timeToWait = _numberOfTimeWaitedForEnqueue / 2;
                if (timeToWait < 2)
                    timeToWait = 2;
                else if (timeToWait > timeout)
                    timeToWait = timeout;
                timeout -= timeToWait;
                Thread.Sleep(timeToWait);
                if (Enqueue(entry))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Dequeue(out T entry)
        {
            entry = default(T);
            var readIndex = PositionToArrayIndex(_readPos);
            var writeIndex = PositionToArrayIndex(_writePos);

            if (readIndex == writeIndex)
                return false; // queue empty

            entry = _data[readIndex];
            _data[readIndex] = default(T);
            _readPos++;

            return true;
        }
    }
}
