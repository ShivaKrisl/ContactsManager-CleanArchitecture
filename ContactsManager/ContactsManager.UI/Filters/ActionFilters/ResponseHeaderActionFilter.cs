using Microsoft.AspNetCore.Mvc.Filters;

namespace Filters.ActionFilters
{
    public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
    {

        public int Order { get; set; }

        private readonly ILogger<ResponseHeaderActionFilter> _logger;

        private readonly string _key;
        private readonly string _value;

        public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger, string key, string value, int order)
        {
            _logger = logger;
            _key = key;
            _value = value;
            Order = order;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // executes before action method
            _logger.LogInformation("-------- {FilterName}.{MethodName} called for Start --------", nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));

            await next();

            // executes after action method
            _logger.LogInformation("-------- {FilterName}.{MethodName} called for End --------", nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));

            // Add custom header to the response
            context.HttpContext.Response.Headers.Add(_key, _value);

        }

    }
}
