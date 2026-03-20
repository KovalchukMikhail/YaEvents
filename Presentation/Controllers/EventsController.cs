using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;

namespace YaEvents.Presentation.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }
        [HttpGet]
        public IActionResult GetEvents([FromQuery] string? title = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, int page = 1, int pageSize = 10)
        {
            ValidateEventsRequest(from, to, page, pageSize);

            if (ModelState.ErrorCount > 0)
                return ValidationProblem(ModelState);

            var events = _eventService.GetEvents(title, from, to);
            return Ok(_eventService.GetEventsWithPagination(events, page, pageSize));
        }
        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetEvent(int id)
        {
            var eventDto = _eventService.GetEvent(id);
            if(eventDto != null)
                return Ok(eventDto);
            else
                return NotFound();
        }
        [HttpPost]
        public IActionResult PostEvent([FromBody] EventDtoLite eventDto)
        {
            if(!CompareEventDates(eventDto.StartAt, eventDto.EndAt))
            {
                ModelState.AddModelError("EndAt", "Дата окончания события должна быть позже даты начала");
                
                return ValidationProblem(ModelState);
            }

            var newEventDto = _eventService.PostEvent(eventDto);
            var url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}/{newEventDto.Id}";

            return Created(url, newEventDto);
        }
        [HttpPut]
        [Route("{id:int}")]
        public IActionResult PutEvent(int id, [FromBody] EventDtoLite eventDto)
        {
            if (!CompareEventDates(eventDto.StartAt, eventDto.EndAt))
            {
                ModelState.AddModelError("EndAt", "Дата окончания события должна быть позже даты начала");

                return ValidationProblem(ModelState);
            }

            if (_eventService.PutEvent(id, eventDto))
                return Ok();
            else
                return NotFound();
        }
        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteEvent(int id)
        {
            if (_eventService.DeleteEvent(id))
                return NoContent();
            else
                return NotFound();
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
