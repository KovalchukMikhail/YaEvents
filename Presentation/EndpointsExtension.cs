using YaEvents.Presentation.Endpoints;

namespace YaEvents.Presentation
{
    public static class EndpointsExtension
    {
        public static WebApplication AddEndpoints(this WebApplication app)
        {
            app.MapGet("/bookings/{id:Guid}", BookingEndpoints.GetBooking);

            app.MapPost("/events/{id:Guid}/book", EventEndpoints.PostBooking);

            return app;
        }
    }
}
