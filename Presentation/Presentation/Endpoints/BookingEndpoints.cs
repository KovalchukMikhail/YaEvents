using Application.DTO;
using Application.Services.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Presentation.Presentation.Endpoints
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

            var bookingInfo = await bookingService.GetBooking(new Guid(userIdClaim.Value), id, token: token);

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

            if(await bookingService.CancelBooking(new Guid(userIdClaim.Value), id, token: token))
                return Results.NoContent();
            else
                return Results.BadRequest("Бронирование уже помечено как отмененное.");
        }
    }
}
