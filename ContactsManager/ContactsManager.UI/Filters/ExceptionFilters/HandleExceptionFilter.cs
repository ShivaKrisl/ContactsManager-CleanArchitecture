using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Filters.ExceptionFilters
{
    public class HandleExceptionFilter : IAsyncExceptionFilter
    {

        private readonly ILogger<HandleExceptionFilter> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public HandleExceptionFilter(ILogger<HandleExceptionFilter> logger, IHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (_hostEnvironment.IsDevelopment())
            {
                _logger.LogError("Excpetion encountered {FilterName}.{MethodName} \n {ExceptionType} \n {ExceptionMessage}", nameof(HandleExceptionFilter), nameof(OnExceptionAsync), context.Exception.GetType().ToString(), context.Exception.Message);
                context.Result = new ContentResult()
                {
                    Content = $"<h1>Exception: {context.Exception.Message}</h1><br/><h2>StackTrace: {context.Exception.StackTrace}</h2>",
                    ContentType = "text/html",
                    StatusCode = 500
                };
            }
            else
            {
                _logger.LogError("Excpetion encountered {FilterName}.{MethodName} \n {ExceptionType} \n {ExceptionMessage}", nameof(HandleExceptionFilter), nameof(OnExceptionAsync), context.Exception.GetType().ToString(), context.Exception.Message);
                context.Result = new ContentResult()
                {
                    Content = "<h1>Something went wrong</h1>",
                    ContentType = "text/html",
                    StatusCode = 500
                };
            }



                return Task.CompletedTask;
        }
    }
}
