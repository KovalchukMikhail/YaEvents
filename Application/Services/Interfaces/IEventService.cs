using YaEvents.Data.Dto;
using YaEvents.Data.Models;

namespace YaEvents.Application.Services.Interfaces
{
    public interface IEventService
    {
        Task<EventInfo[]> GetEvents(string? title = null, DateTime? from = null, DateTime? to = null, CancellationToken token = default);
        Task<EventInfo?> GetEvent(Guid id, CancellationToken token = default);
        Task<EventInfo> PostEvent(CreateEvent eventDto, CancellationToken token = default);
        Task<bool> PutEvent(Guid id, CreateEvent eventDto, CancellationToken token = default);
        Task<bool> DeleteEvent(Guid id, CancellationToken token = default);
        Task<PaginatedResult<EventInfo>> GetEventsWithPagination(EventInfo[] sourceEvents, int pageNumber, int pageSize, CancellationToken token = default);

    }
}
