using System;
using System.Threading;
using System.Threading.Tasks;

public class AsyncLock
{
    private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

    public async Task<IDisposable> LockAsync()
    {
        var releaser = new Releaser(semaphoreSlim);
        await semaphoreSlim.WaitAsync();
        return releaser;
    }

    private class Releaser : IDisposable
    {
        private SemaphoreSlim semaphoreSlim;

        public Releaser(SemaphoreSlim semaphoreSlim)
        {
            this.semaphoreSlim = semaphoreSlim;
        }

        public void Dispose()
        {
            semaphoreSlim.Release();
        }
    }
}
