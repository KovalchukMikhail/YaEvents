using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;

namespace YaEvents.Presentation.Endpoints
{
    public class EventEndpoints
    {
        public static async Task<IResult> PostBooking(Guid id, IEventService eventService, IBookingService bookingService, HttpContext context, CancellationToken token = default)
        {
            var eventDto = await eventService.GetEvent(id, token: token);
            if (eventDto != null)
            {
                if (eventDto.Status != EventStatus.Removed)
                {
                    var bookingInfo = await bookingService.CreateBookingAsync(eventDto.Id, token: token);
                    var url = $"{context.Request.Scheme}://{context.Request.Host}/bookings/{bookingInfo.Id}";

                    return Results.Accepted(url, bookingInfo);
                }
                else
                    throw new ValidationException("Не удалось создать объект бронирования так как объект события помечен как удаленный") { EntityId = id };
            }
            else
                throw new NotFoundException("Не удалось создать объект бронирования так как объект события с указанным Id отсутствует") { EntityId = id };
        }
    }
}
