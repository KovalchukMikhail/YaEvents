using Presentation.Endpoints;

namespace Presentation.Infrastructure
{
    public static class EndpointsExtension
    {
        public static WebApplication AddEndpoints(this WebApplication app)
        {
            app.MapGet("/bookings/{id:Guid}", BookingEndpoints.GetBooking);
            app.MapPost("/events/{id:Guid}/book", BookingEndpoints.PostBooking);
            app.MapDelete("/bookings/{id:Guid}", BookingEndpoints.DeleteBooking);

            return app;
        }
    }
}
