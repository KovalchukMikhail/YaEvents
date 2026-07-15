using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Application.Semaphores
{
    public class AppSemaphores
    {
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _semaphores = new ConcurrentDictionary<Guid, SemaphoreSlim>();

        public static SemaphoreSlim GetSemaphore(Guid entityGuid)
        {
            return _semaphores.GetOrAdd(entityGuid, new SemaphoreSlim(1, 1));
        }
    }
}
