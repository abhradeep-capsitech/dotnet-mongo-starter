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
            var response = new ApiResponse<object>();

            switch (exception)
            {
                case UnauthorizedException unauthorizedEx:
                    context.Response.StatusCode = unauthorizedEx.StatusCode;
                    response = ApiResponse<object>.Error(unauthorizedEx.Message, unauthorizedEx.Errors);
                    break;

                case ApiException apiEx:
                    context.Response.StatusCode = apiEx.StatusCode;
                    response = ApiResponse<object>.Error(apiEx.Message, apiEx.Errors);
                    break;

                case MongoException mongoEx:
                    context.Response.StatusCode = 500;
                    response = ApiResponse<object>.Error("Database error occurred.", new List<string> { mongoEx.Message });
                    _logger.LogError(mongoEx, "MongoDB error occurred.");
                    break;

                default:
                    context.Response.StatusCode = 500;
                    response = ApiResponse<object>.Error("An unexpected error occurred.", new List<string> { exception.Message });
                    _logger.LogError(exception, "Unexpected error occurred.");
                    break;
            }

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
