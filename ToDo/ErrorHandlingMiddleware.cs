namespace ToDoAPI
{
    /// <summary>
    /// Global error handler. 
    /// This middleware catches exceptions, log them and return a standard error response in JSON format.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // pass control to the next middleware/endpoint
                await _next(context);
            }
            catch (Exception ex)
            {
                // log error
                _logger.LogError(ex, "An unexpected error occurred");

                // compose standard response
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    Message = "An unexpected error occurred. Please ensure the request url and data are correct or try again later."
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}
