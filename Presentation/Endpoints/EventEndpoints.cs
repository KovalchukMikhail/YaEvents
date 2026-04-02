using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
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
            var bookingInfo = await bookingService.CreateBookingAsync(id, token: token);
            var url = $"{context.Request.Scheme}://{context.Request.Host}/bookings/{bookingInfo.Id}";

            return Results.Accepted(url, bookingInfo);
        }
    }
}
