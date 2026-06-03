using Application.Services.Interfaces;

namespace Presentation.Presentation.Endpoints
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
