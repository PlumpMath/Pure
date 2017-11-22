﻿using System;
using System.Threading;

namespace Pure.Threading
{
    internal class SemaphoreContext : IDisposable
    {
        private SemaphoreSlim semaphore;

        public SemaphoreContext(SemaphoreSlim semaphore)
        {
            this.semaphore = semaphore;
            semaphore.Wait();
        }

        public void Dispose()
        {
            semaphore.Release();
        }
    }
}
