using IPInfoApp.Business.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace IPInfoApp.API.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(ILogger<ErrorHandlerMiddleware> logger, RequestDelegate next)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                string result = "";
                response.ContentType = "application/json";
                switch (error)
                {
                    case DbUpdateConcurrencyException:
                        _logger.LogError(error, "Another user has modified the requested resource");
                        response.StatusCode = (int)HttpStatusCode.Conflict;
                        result = JsonSerializer.Serialize(new { message = error?.Message });
                        break;
                    case UnauthorizedException:
                        _logger.LogError(error, "Unauthorised to use this resource");
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        result = JsonSerializer.Serialize(new { message = error?.Message });
                        break;
                    case KeyNotFoundException:
                        _logger.LogError(error, "Requested resource not found");
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        result = JsonSerializer.Serialize(new { message = error?.Message });
                        break;
                    case ReferentialIntegrityException referentialException:
                        _logger.LogError(error, "Cannot delete entity because it is referenced elsewhere.");
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        result = JsonSerializer.Serialize(new { message = referentialException.Message });
                        break;
                    case CountryNotFoundException countryNotFoundException:
                        _logger.LogError(error, "Could Not found country details for the specific ip Address  using the ip2c service.");
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        result = JsonSerializer.Serialize(new { message = countryNotFoundException.Message });
                        break;
                    case IP2CException apiException:
                        _logger.LogError(error, "IP2C Service  error encountered");
                        response.StatusCode = (int)apiException.StatusCode;
                        result = JsonSerializer.Serialize(new { message = apiException.Message, details = apiException.ResponseBody });
                        break;
                    default:
                        _logger.LogError(error, "An unhandled error occured");
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        result = JsonSerializer.Serialize(new { message = error?.Message });
                        break;
                }


                await response.WriteAsync(result);
            }
        }
    }
}
