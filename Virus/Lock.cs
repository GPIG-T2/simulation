using System;
using System.Threading;
using System.Threading.Tasks;

namespace Virus
{
    public class Lock
    {
        private readonly SemaphoreSlim _semaphore = new(1);

        public async Task<IDisposable> Aquire()
        {
            await this._semaphore.WaitAsync();
            return new LockHandle(this._semaphore);
        }

        private class LockHandle : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public LockHandle(SemaphoreSlim semaphore) => this._semaphore = semaphore;

            public void Dispose() => this._semaphore.Release();
        }
    }
}
