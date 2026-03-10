using YaEvents.Data.Dto;
using YaEvents.Data.Models;

namespace YaEvents.Application.Services.Interfaces
{
    public interface IEventService
    {
        EventDto[] GetAllEvents();
        EventDto GetEvent(int id);
        void PostEvent(EventDto eventDto);
        bool PutEvent(EventDto eventDto);
        bool DeleteEvent(int id);

    }
}
