using YaEvents.Data.Dto;
using YaEvents.Data.Models;

namespace YaEvents.Application.Services.Interfaces
{
    public interface IEventService
    {
        public Task<EventInfo?> GetEvent(Guid id, CancellationToken token = default);
        public Task<EventInfo> PostEvent(CreateEvent eventDto, CancellationToken token = default);
        public Task<bool> PutEvent(Guid id, CreateEvent eventDto, CancellationToken token = default);
        public Task<bool> DeleteEvent(Guid id, CancellationToken token = default);
        public Task<PaginatedResult<EventInfo>> GetEventsWithPagination(int pageNumber = 1, int pageSize = 10, string? title = null, DateTime? from = null, DateTime? to = null, CancellationToken token = default);

    }
}
