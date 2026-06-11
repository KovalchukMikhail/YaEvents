using Application.Services.Interfaces;
using Domain.Exceptions;

namespace Presentation.Presentation.Endpoints
{
    public class BookingEndpoints
    {
        public static async Task<IResult> GetBooking(Guid id, IBookingService bookingService, CancellationToken token = default)
        {
            var bookingDto = await bookingService.GetBookingByIdAsync(id, token: token);
            if (bookingDto != null)
                return Results.Ok(bookingDto);
            else
                throw new NotFoundException("Не удалось получить объект бронирования") { EntityId = id };
        }
    }
}
