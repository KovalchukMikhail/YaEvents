using YaEvents.Data.Dto;
using YaEvents.Data.Models;

namespace YaEvents.Application.Services.Interfaces
{
    public interface IEventService
    {
        EventDto[] GetEvents(string? title = null, DateTime? from = null, DateTime? to = null);
        EventDto? GetEvent(int id);
        EventDto PostEvent(EventDtoLite eventDto);
        bool PutEvent(int id, EventDtoLite eventDto);
        bool DeleteEvent(int id);
        PaginatedResult<EventDto> GetEventsWithPagination(EventDto[] sourceEvents, int pageNumber, int pageSize);

    }
}
