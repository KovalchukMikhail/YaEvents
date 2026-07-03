using Application.Services.Interfaces;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Presentation.Endpoints
{
    public class BookingEndpoints
    {
        [Authorize(Roles = "User,Admin")]
        public static async Task<IResult> GetBooking(Guid id, IBookingService bookingService, HttpContext context, CancellationToken token = default)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Results.BadRequest("Идентификатор пользователя не найден");
            }
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            if(roleClaim == null)
            {
                return Results.BadRequest("Отсутсвует роль пользователя");
            }

            var bookingInfo = await bookingService.GetBooking(id, new Guid(userIdClaim.Value), roleClaim.Value == "Admin", token: token);

            if (bookingInfo != null)
                return Results.Ok(bookingInfo);
            else
                throw new NotFoundException("Не удалось получить объект бронирования") { EntityId = id };
        }
        [Authorize(Roles = "User,Admin")]
        public static async Task<IResult> DeleteBooking(Guid id, IBookingService bookingService, HttpContext context, CancellationToken token = default)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Results.BadRequest("Идентификатор пользователя не найден");
            }
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
            {
                return Results.BadRequest("Отсутсвует роль пользователя");
            }

            if (await bookingService.CancelBooking(id, new Guid(userIdClaim.Value), roleClaim.Value == "Admin", token: token))
                return Results.NoContent();
            else
                return Results.BadRequest("Бронирование уже помечено как отмененное.");
        }
        [Authorize(Roles = "User, Admin")]
        public static async Task<IResult> PostBooking(Guid id, IBookingService bookingService, HttpContext context, IConfiguration configuration, CancellationToken token = default)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Results.BadRequest("Идентификатор пользователя не найден");
            }
            var limitOfBookingsCount = configuration.GetValue<int>("LimitOfActiveBookings");
            var bookingInfo = await bookingService.CreateBookingAsync(id, new Guid(userIdClaim.Value), limitOfBookingsCount, token: token);
            var url = $"{context.Request.Scheme}://{context.Request.Host}/bookings/{bookingInfo.Id}";

            return Results.Accepted(url, bookingInfo);
        }
    }
}
