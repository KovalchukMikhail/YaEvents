using YaEvents.Data.Dto;
using YaEvents.Data.Models;

namespace YaEvents.Application.Services.Interfaces
{
    public interface IEventService
    {
        Task<EventDto[]> GetEvents(string? title = null, DateTime? from = null, DateTime? to = null, CancellationToken token = default);
        Task<EventDto?> GetEvent(Guid id, CancellationToken token = default);
        Task<EventDto> PostEvent(EventDtoLite eventDto, CancellationToken token = default);
        Task<bool> PutEvent(Guid id, EventDtoLite eventDto, CancellationToken token = default);
        Task<bool> DeleteEvent(Guid id, CancellationToken token = default);
        Task<PaginatedResult<EventDto>> GetEventsWithPagination(EventDto[] sourceEvents, int pageNumber, int pageSize, CancellationToken token = default);

    }
}
