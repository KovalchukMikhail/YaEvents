using System.Collections.Concurrent;

namespace YaEvents.Infrastructure
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
