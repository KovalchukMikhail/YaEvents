using YaEvents.Data.Dto;
using YaEvents.Data.Models;

namespace YaEvents.Application.Services.Interfaces
{
    public interface IEventService
    {
        EventDto[] GetAllEvents();
        EventDto GetEvent(int id);
        EventDto PostEvent(EventDtoLite eventDto);
        bool PutEvent(int id, EventDtoLite eventDto);
        bool DeleteEvent(int id);

    }
}
