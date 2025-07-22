using DotnetMongoStarter.Utils;
using MongoDB.Driver;
using System.Text.Json;
using static DotnetMongoStarter.Utils.ApiException;

namespace DotnetMongoStarter.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var path = context.Request.Path;
            var isDev = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false;

            var response = exception switch
            {
                UnauthorizedException ex => CreateResponse(context, ex, 401, path, "Unauthorized"),
                NotFoundException ex => CreateResponse(context, ex, 404, path, "Not Found"),
                ValidationException ex => CreateResponse(context, ex, 400, path, "Validation failed"),
                ForbiddenException ex => CreateResponse(context, ex, 403, path, "Forbidden"),
                ApiException ex => CreateResponse(context, ex, ex.StatusCode, path, "API error"),
                MongoException ex => CreateResponse(context, "Database error occurred.", 500, path, isDev ? ex.Message : null),
                _ => CreateResponse(context, "An unexpected error occurred.", 500, path, isDev ? exception.Message : null)
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private ApiResponse<object> CreateResponse(HttpContext context, ApiException ex, int statusCode, string path, string logMessage)
        {
            context.Response.StatusCode = statusCode;
            _logger.LogWarning(ex, $"{logMessage}: {path}");
            return ApiResponse<object>.Error(ex.Message, ex.Errors);
        }

        private ApiResponse<object> CreateResponse(HttpContext context, string message, int statusCode, string path, string? devError = null)
        {
            context.Response.StatusCode = statusCode;
            _logger.LogError($"{message}: {path}");
            return ApiResponse<object>.Error(message, devError != null ? new List<string> { devError } : null);
        }
    }
}
