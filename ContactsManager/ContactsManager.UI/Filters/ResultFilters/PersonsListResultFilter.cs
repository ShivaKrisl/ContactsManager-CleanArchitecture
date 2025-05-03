using Microsoft.AspNetCore.Mvc.Filters;

namespace Filters.ResultFilters
{
    public class PersonsListResultFilter : IAsyncResultFilter
    {
        private readonly ILogger<PersonsListResultFilter> _logger;

        public PersonsListResultFilter(ILogger<PersonsListResultFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // executes before returning IAction Result
            _logger.LogInformation("-------- {FilterName}.{MethodName} called --------", nameof(PersonsListResultFilter), nameof(OnResultExecutionAsync));


            await next(); // call subsequent result filters or the action result

            // executes after returning IAction Result
            _logger.LogInformation("-------- {FilterName}.{MethodName} called --------", nameof(PersonsListResultFilter), nameof(OnResultExecutionAsync));

            // Add custom header to the response
            context.HttpContext.Response.Headers.Add("X-ResultFilter", "Result-Value");

        }
    }
}
