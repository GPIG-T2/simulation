using System;

namespace Virus
{
    public class CycleQueue<T>
    {
        private readonly T[] _arr;
        private int _head = 0;

        public CycleQueue(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this._arr = new T[size];
        }

        /// <summary>
        /// The value that is currently at the head of the queue.
        /// </summary>
        public T Front
        {
            get => this._arr[this._head];
            set => this._arr[this._head] = value;
        }

        /// <summary>
        /// Pushes the given value onto the head of the queue. The oldest value
        /// in the queue is then dropped.
        /// </summary>
        /// <param name="value"></param>
        public void Push(T value)
        {
            this._head++;
            this._head %= this._arr.Length;
            this.Front = value;
        }

        /// <summary>
        /// Gets the value at the given offset from the head. Positive offsets
        /// will reference the oldest values first, negative offsets will
        /// reference the newest values first.
        /// </summary>
        /// <param name="offset">The index offset to look at.</param>
        /// <returns>The value at the given offset.</returns>
        public T this[int offset]
        {
            get => this._arr[this.CalculateIndexOffset(offset)];
            set => this._arr[this.CalculateIndexOffset(offset)] = value;
        }

        private int CalculateIndexOffset(int back)
        {
            int offset = back;

            if (back < 0)
            {
                offset = this._arr.Length - (-back % this._arr.Length);
            }

            return (this._head + offset) % this._arr.Length;
        }
    }
}
