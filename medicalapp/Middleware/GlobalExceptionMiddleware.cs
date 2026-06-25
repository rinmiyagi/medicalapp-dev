using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using medicalapp.Exceptions;

namespace medicalapp.Middleware
{
    /// <summary>
    /// Catches unhandled exceptions, logging them and redirecting regular requests to an error page, or returning JSON for AJAX requests.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, $"Unhandled exception occurred: {ex.Message}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Check if it is an AJAX request
            bool isAjax = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (isAjax)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = exception is AppException ? 400 : 500;
                var jsonResponse = System.Text.Json.JsonSerializer.Serialize(new { error = exception.Message });
                return context.Response.WriteAsync(jsonResponse);
            }

            // For regular MVC requests, redirect to the error page
            context.Response.Redirect($"/Home/Error?message={Uri.EscapeDataString(exception.Message)}");
            return Task.CompletedTask;
        }
    }
}
