using Application.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Presentation.Presentation.Endpoints
{
    public class EventEndpoints
    {
        [Authorize(Roles = "User, Admin")]
        public static async Task<IResult> PostBooking(Guid id, IEventService eventService, IBookingService bookingService, HttpContext context, IConfiguration configuration, CancellationToken token = default)
        {
            var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub);
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
