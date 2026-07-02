using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Presentation.Infrastructure.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
                RequestDelegate next,
                ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleException(httpContext, ex);
            }
        }

        private async Task HandleException(HttpContext httpContext, Exception ex)
        {
            _logger.LogError(
                ex,
                "Необработанное исключение. Метод={Method}, Path={Path}, RequestId={RequestId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Request.Headers["x-request-id"]);

            if (httpContext.Response.HasStarted)
                return;

            await SetResponse(ex, httpContext);
        }

        private static async Task SetResponse(Exception ex, HttpContext httpContext)
        {
            var error = new ProblemDetails();
            switch (ex)
            {
                case NotFoundException notFoundException:
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    error.Title = notFoundException.Message;
                    if (notFoundException.EntityId != null)
                    {
                        error.Detail = $"Id = {notFoundException.EntityId}";
                    }
                    break;
                case NoAvailableSeatsException notAvailableSeatsException:
                    httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                    error.Title = notAvailableSeatsException.Message;
                    if (notAvailableSeatsException.EntityId != null)
                    {
                        error.Detail = $"EventId = {notAvailableSeatsException.EntityId}";
                    }
                    break;
                case BookingPastEventException bookingPastEventException:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    error.Title = bookingPastEventException.Message;
                    break;
                case DomainValidationException domainValidationException:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    error.Title = domainValidationException.Message;
                    break;
                case LimitOfActiveBookingsExceededException limitOfActiveBookingsExceededException:
                    httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                    error.Title = limitOfActiveBookingsExceededException.Message;
                    if (limitOfActiveBookingsExceededException.LimitOfBookings != null)
                    {
                        var exDetail = $"Предельное количество бронирований для одного пользователя = {limitOfActiveBookingsExceededException.LimitOfBookings}";
                        if (limitOfActiveBookingsExceededException.CurrentBookingsCount != null)
                        {
                            exDetail += $". Текущее количество бронирований = {limitOfActiveBookingsExceededException.CurrentBookingsCount}";
                        }
                        error.Detail = exDetail;
                    }
                    break;
                case NoRightsToOperationException noRightsToOperationException:
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    error.Title = noRightsToOperationException.Message;
                    break;
                default:
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    error.Title = ex.GetType().Name;
                    error.Detail = ex.Message;
                    break;
            }
            error.Status = httpContext.Response.StatusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(error);
        }
    }
}
