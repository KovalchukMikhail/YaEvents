using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;

namespace YaEvents.Presentation.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        public EventsController(IEventService eventService, IBookingService bookingService)
        {
            _eventService = eventService;
            _bookingService = bookingService;
        }
        [HttpGet]
        public async Task<IActionResult> GetEvents(CancellationToken token, [FromQuery] string? title = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, int page = 1, int pageSize = 10)
        {
            ValidateEventsRequest(from, to, page, pageSize);

            if (ModelState.ErrorCount > 0)
                throw new ValidationException("В запросе на получение событий переданы некорректные параметры.") { ModelState = ModelState };

            var events = await _eventService.GetEvents(title, from, to, token: token);
            events = events.Where(e => e.Status == EventStatus.Existing).ToArray();
            return Ok(_eventService.GetEventsWithPagination(events, page, pageSize));
        }
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetEvent(Guid id, CancellationToken token)
        {
            var eventDto = await _eventService.GetEvent(id, token: token);
            if (eventDto != null)
            {
                if(eventDto.Status == EventStatus.Existing)
                    return Ok(eventDto);
                else
                    throw new ValidationException("Запрашиваемый объект события помечен как удаленный") { EntityId = id };
            }
            else
                throw new NotFoundException("Не удалось получить объект события") { EntityId = id };
        }
        [HttpPost]
        public async Task<IActionResult> PostEvent([FromBody] CreateEvent createEvent, CancellationToken token)
        {
            if(!CompareEventDates(createEvent.StartAt, createEvent.EndAt))
            {
                ModelState.AddModelError("EndAt", "Дата окончания события должна быть позже даты начала");

                throw new ValidationException("В запросе на добавление нового события переданы некорректные параметры.") { ModelState = ModelState };
            }

            var newEventDto = await _eventService.PostEvent(createEvent, token: token);
            var url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}/{newEventDto.Id}";

            return Created(url, newEventDto);
        }
        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> PutEvent(Guid id, [FromBody] CreateEvent createEvent, CancellationToken token)
        {
            if (!CompareEventDates(createEvent.StartAt, createEvent.EndAt))
            {
                ModelState.AddModelError("EndAt", "Дата окончания события должна быть позже даты начала");

                throw new ValidationException("В запросе на редактирование события переданы некорректные параметры.") { ModelState = ModelState, EntityId = id };
            }

            if (await _eventService.PutEvent(id, createEvent, token: token))
                return Ok();
            else
                throw new NotFoundException("Не удалось получить объект события") { EntityId = id };
        }
        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken token)
        {
            var requiredEvent = await _eventService.GetEvent(id, token);
            if(requiredEvent != null)
            {
                if(await _eventService.DeleteEvent(id, token: token))
                    return NoContent();
                else
                    throw new ValidationException("Объект события уже был отмечен как удаленный") { EntityId = id };
            }
            else
                throw new NotFoundException("Не удалось удалить объект события, так как событие не найдено") { EntityId = id };
        }
        private bool CompareEventDates(DateTime startAt, DateTime endAt)
        {
            return startAt < endAt;
        }

        private void ValidateEventsRequest(DateTime? from, DateTime? to, int page, int pageSize)
        {
            if (from != null && to != null && !CompareEventDates(from.Value, to.Value))
                ModelState.AddModelError("to", "Дата окончания события должна быть позже даты начала");

            if (page < 1)
                ModelState.AddModelError("page", "Номер страницы не может быть меньше 1");

            if (pageSize < 1)
                ModelState.AddModelError("pageSize", "Количество событий на странице не может быть меньше 1");


        }
    }
}
