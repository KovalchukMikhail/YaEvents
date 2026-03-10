using Microsoft.AspNetCore.Mvc;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;

namespace YaEvents.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }
        [HttpGet]
        public IActionResult GetEvents()
        {
            var events = _eventService.GetAllEvents();
            if(events.Length > 0)
                return Ok(events);
            else
                return NoContent();
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
        public IActionResult PostEvent([FromBody] EventDto eventDto)
        {
            if(!CompareEventDates(eventDto.StartAt, eventDto.EndAt))
            {
                ModelState.AddModelError("EndAt", "Дата окончания события должна быть позже даты начала");

                return BadRequest(ModelState);
            }

            _eventService.PostEvent(eventDto);

            return Created();
        }
        [HttpPut]
        public IActionResult PutEvent([FromBody] EventDto eventDto)
        {
            if (!CompareEventDates(eventDto.StartAt, eventDto.EndAt))
            {
                ModelState.AddModelError("EndAt", "Дата окончания события должна быть позже даты начала");

                return BadRequest(ModelState);
            }

            if (_eventService.PutEvent(eventDto))
                return Ok();
            else
                return NotFound();
        }
        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteEvent(int id)
        {
            if (_eventService.DeleteEvent(id))
                return Ok();
            else
                return NotFound();
        }

        private bool CompareEventDates(DateTime startAt, DateTime endAt)
        {
            return startAt < endAt;
        }
    }
}
