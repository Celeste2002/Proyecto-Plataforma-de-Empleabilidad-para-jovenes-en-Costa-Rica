using System.Net;
using services.exceptions;

namespace api.middleware;

public sealed class ErrorHandlingMiddleware(
    RequestDelegate next,
    IWebHostEnvironment webHostEnvironment,
    ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (RequestValidationException requestValidationException)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                message = requestValidationException.Message,
                errors = requestValidationException.ValidationErrors
            });
        }
        catch (EmailDeliveryException emailDeliveryException)
        {
            logger.LogError(emailDeliveryException, "Email delivery failed.");

            httpContext.Response.StatusCode = (int)HttpStatusCode.BadGateway;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                message = emailDeliveryException.Message,
                detail = webHostEnvironment.IsDevelopment()
                    ? emailDeliveryException.InnerException?.Message
                    : null
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled request error.");

            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                message = "Ocurrio un error inesperado al procesar la solicitud.",
                detail = webHostEnvironment.IsDevelopment() ? exception.Message : null
            });
        }
    }
}
