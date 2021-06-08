using System;
using Xunit;

namespace Virus.Test
{
    public class TestCycleQueue
    {
        private readonly CycleQueue<int> _queue;

        public TestCycleQueue()
        {
            this._queue = new(5);
        }

        [Fact]
        public void TestFront()
        {
            this._queue.Front = 5;

            Assert.Equal(5, this._queue.Front);

            this._queue.Front = 10;

            Assert.Equal(10, this._queue.Front);
        }

        [Fact]
        public void TestPush()
        {
            this._queue.Front = 1;

            this._queue.Push(2);
            Assert.Equal(2, this._queue.Front);

            this._queue.Push(3);
            Assert.Equal(3, this._queue.Front);

            this._queue.Push(4);
            Assert.Equal(4, this._queue.Front);

            this._queue.Push(5);
            Assert.Equal(5, this._queue.Front);

            this._queue.Push(6);
            Assert.Equal(6, this._queue.Front);
        }

        [Fact]
        public void TestOffset()
        {
            this._queue.Front = 1;
            Assert.Equal(1, this._queue[0]);

            this._queue.Push(2);
            Assert.Equal(0, this._queue[1]);
            Assert.Equal(2, this._queue[0]);
            Assert.Equal(1, this._queue[-1]);

            this._queue.Push(3);
            Assert.Equal(0, this._queue[1]);
            Assert.Equal(3, this._queue[0]);
            Assert.Equal(2, this._queue[-1]);
            Assert.Equal(1, this._queue[-2]);

            this._queue.Push(4);
            Assert.Equal(0, this._queue[1]);
            Assert.Equal(4, this._queue[0]);
            Assert.Equal(3, this._queue[-1]);
            Assert.Equal(2, this._queue[-2]);
            Assert.Equal(1, this._queue[-3]);

            this._queue.Push(5);
            Assert.Equal(1, this._queue[1]);
            Assert.Equal(5, this._queue[0]);
            Assert.Equal(4, this._queue[-1]);
            Assert.Equal(3, this._queue[-2]);
            Assert.Equal(2, this._queue[-3]);
            Assert.Equal(1, this._queue[-4]);

            this._queue.Push(6);
            Assert.Equal(2, this._queue[6]);
            Assert.Equal(6, this._queue[5]);
            Assert.Equal(5, this._queue[4]);
            Assert.Equal(4, this._queue[3]);
            Assert.Equal(3, this._queue[2]);
            Assert.Equal(2, this._queue[1]);
            Assert.Equal(6, this._queue[0]);
            Assert.Equal(5, this._queue[-1]);
            Assert.Equal(4, this._queue[-2]);
            Assert.Equal(3, this._queue[-3]);
            Assert.Equal(2, this._queue[-4]);
            Assert.Equal(6, this._queue[-5]);
            Assert.Equal(5, this._queue[-6]);
        }
    }
}
