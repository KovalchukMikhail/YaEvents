using Microsoft.AspNetCore.Mvc;
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
        public IActionResult GetEvents()
        {
            return Ok(_eventService.GetAllEvents());
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

                return BadRequest(ModelState);
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
    }
}
