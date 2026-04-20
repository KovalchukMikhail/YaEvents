using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using YaEvents.Infrastructure.Exceptions;

namespace YaEvents.Application.Middleware
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
                case ValidationException validationException:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    error.Title = validationException.Message;
                    var detail = new StringBuilder();
                    if(validationException.EntityId != null)
                    {
                        detail.Append($"Id объекта: {validationException.EntityId}. ");
                    }
                    if (validationException.ModelState != null && validationException.ModelState.ErrorCount > 0)
                    {
                        detail.Append($"Детали ошибки: ");
                        foreach (var item in validationException.ModelState.Values)
                        {
                            foreach (var curError in item.Errors)
                            {
                                detail.Append($"{curError.ErrorMessage}. ");
                            }
                        }
                    }
                    error.Detail = detail.ToString();
                    break;
                case NotFoundException notFoundException:
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    error.Title = notFoundException.Message;
                    if(notFoundException.EntityId != null)
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
