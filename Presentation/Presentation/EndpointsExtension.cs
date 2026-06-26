using Presentation.Presentation.Endpoints;

namespace Presentation.Presentation
{
    public static class EndpointsExtension
    {
        public static WebApplication AddEndpoints(this WebApplication app)
        {
            app.MapGet("/bookings/{id:Guid}", BookingEndpoints.GetBooking);
            app.MapPost("/events/{id:Guid}/book", EventEndpoints.PostBooking);
            app.MapDelete("/bookings/{id:Guid}", BookingEndpoints.DeleteBooking);

            return app;
        }
    }
}
