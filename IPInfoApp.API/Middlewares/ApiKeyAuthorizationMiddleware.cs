using IPInfoApp.Business.Options;
using IPInfoApp.Business.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace IPInfoApp.API.Middlewares
{
    public class ApiKeyAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiKeyOptions _apiKeyOptions;
        private const string APIKEYNAME = "ApiKey";

        public ApiKeyAuthorizationMiddleware(RequestDelegate next, IOptions<ApiKeyOptions> apiKeyOptions)
        {
            _next = next;
            _apiKeyOptions = apiKeyOptions.Value;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {

            foreach (var header in context.Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }

            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(Messages.ApiKeyNotProvided());
                return;
            }


            if (!_apiKeyOptions.ApiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(Messages.Unauthorised());
                return;
            }

            await _next(context);
        }
    }
}
